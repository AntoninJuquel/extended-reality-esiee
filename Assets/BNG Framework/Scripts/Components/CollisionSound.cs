using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG
{
    /// <summary>
    /// Plays a Sound Clip OnCollisionEnter
    /// </summary>
    public class CollisionSound : MonoBehaviour
    {
        public AudioClip CollisionAudio;
        public AudioClip[] CollisionAudios;
        AudioSource audioSource;
        float startTime;

        Collider col;

        Grabbable grab;

        /// <summary>
        /// Volume will never be played below this amount. 0-1
        /// </summary>
        public float MinimumVolume = 0.25f;

        /// <summary>
        /// Cap volume at this level 0 - 1
        /// </summary>
        public float MaximumVolume = 1f;

        public bool RecentlyPlayedSound = false;

        float lastPlayedSound;
        public float LastRelativeVelocity = 0;
        public float playCooldown = 0.1f;
        public bool playOnCollisionEnter = true, playOnCollisionStay;
        public float minVelocity = 0.1f;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
            }

            startTime = Time.time;
            col = GetComponent<Collider>();
            if (!col)
            {
                col = GetComponentInChildren<Collider>();
            }

            grab = GetComponent<Grabbable>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (playOnCollisionEnter)
            {
                HandleCollision(collision);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (playOnCollisionStay)
            {
                HandleCollision(collision);
            }
        }

        public void HandleCollision(Collision collision)
        {
            // Just spawned, don't fire collision sound immediately
            if (Time.time - startTime < 1f)
            {
                return;
            }

            // No Collider present, don't play sound
            if (col == null || !col.enabled)
            {
                return;
            }

            float colVelocity = collision.relativeVelocity.magnitude;

            bool otherColliderPlayedSound = false;
            var colSound = collision.collider.GetComponent<CollisionSound>();
            // Don't play a sound if something else is playing the same sound.
            // This prevents overlap
            if (colSound)
            {
                otherColliderPlayedSound = colSound.RecentlyPlayedSound && colSound.CollisionAudio == CollisionAudio;
            }

            float soundVolume = Mathf.Clamp(collision.relativeVelocity.magnitude / 10, MinimumVolume, MaximumVolume);
            bool minVelReached = colVelocity > minVelocity;

            // If object is being held play the sound very lightly
            if (!minVelReached && grab != null && grab.BeingHeld)
            {
                minVelReached = true;
                soundVolume = 0.1f;
            }

            bool audioValid = audioSource != null && CollisionAudio != null;

            if (minVelReached && audioValid && !otherColliderPlayedSound && !RecentlyPlayedSound)
            {
                LastRelativeVelocity = colVelocity;

                // Play Shot
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }

                var selectedClip = CollisionAudios is { Length: > 0 }
                    ? CollisionAudios[Random.Range(0, CollisionAudios.Length)]
                    : CollisionAudio;

                audioSource.clip = selectedClip;
                audioSource.pitch = Time.timeScale;
                audioSource.volume = soundVolume;
                audioSource.Play();

                RecentlyPlayedSound = true;

                Invoke("resetLastPlayedSound", playCooldown);
            }
        }

        void resetLastPlayedSound()
        {
            RecentlyPlayedSound = false;
        }
    }
}