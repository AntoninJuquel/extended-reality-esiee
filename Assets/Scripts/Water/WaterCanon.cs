using UnityEngine;
using BNG;
using UnityEngine.UI;

public class WaterCanon : GrabbableEvents
{
    [SerializeField] private ParticleSystem waterParticles;
    [SerializeField] private float waterForce = 10f, drainRate = 0.1f;
    [SerializeField] private Image waterLevelImage;
    private WaterClip _currentClip;

    private void Start()
    {
        UpdateWaterLevel();
    }

    private void UpdateWaterLevel()
    {
        if (!waterLevelImage)
        {
            return;
        }

        waterLevelImage.fillAmount = _currentClip ? _currentClip.WaterPercent : 0f;
    }

    private void DoJet(float triggerValue)
    {
        if (_currentClip && !_currentClip.isEmpty)
        {
            _currentClip.DrainWater(drainRate * Time.deltaTime);

            UpdateWaterLevel();

            if (waterParticles)
            {
                var main = waterParticles.main;
                main.startSpeed = triggerValue * waterForce;

                if (!waterParticles.isPlaying)
                {
                    waterParticles.Play();
                }
            }

            if (input && thisGrabber)
            {
                input.VibrateController(0.1f, 0.5f, 0.2f, thisGrabber.HandSide);
            }
        }
        else
        {
            StopJet();
        }
    }

    private void StopJet()
    {
        if (waterParticles && waterParticles.isPlaying)
        {
            waterParticles.Stop();
        }
    }

    public override void OnTrigger(float triggerValue)
    {
        if (triggerValue > 0.25f)
        {
            DoJet(triggerValue);
        }
        else
        {
            StopJet();
        }

        base.OnTrigger(triggerValue);
    }

    public override void OnRelease()
    {
        StopJet();
    }

    public override void OnTriggerUp()
    {
        StopJet();
        base.OnTriggerUp();
    }

    public void AttachClip(GameObject clip)
    {
        if (clip.TryGetComponent(out _currentClip))
        {
            _currentClip.ClipIn();
            UpdateWaterLevel();
        }
    }

    public void DetachClip()
    {
        if (_currentClip)
        {
            _currentClip.ClipOut();
            _currentClip = null;
            UpdateWaterLevel();
        }
    }
}