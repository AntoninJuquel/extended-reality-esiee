using System.Collections.Generic;
using UnityEngine;
using BNG;
using UnityEngine.Events;
using UnityEngine.UI;

public class WaterCanon : GrabbableEvents
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem waterParticles;
    [SerializeField] private float waterForce = 10f, drainRate = 0.1f, triggerThreshold = 0.2f;
    [SerializeField] private Image waterLevelImage;

    [Header("Inputs : ")] [Tooltip("Controller Input used to eject clip")] [SerializeField]
    private List<GrabbedControllerBinding> EjectInput = new List<GrabbedControllerBinding>()
        { GrabbedControllerBinding.Button2Down };

    [SerializeField] private UnityEvent onEjectClip;
    private WaterClip _currentClip;


    private void Start()
    {
        UpdateWaterLevel();
    }
    
    private void CheckEjectInput()
    {
        // Check for bound controller button to eject magazine
        for (int x = 0; x < EjectInput.Count; x++)
        {
            if (InputBridge.Instance.GetGrabbedControllerBinding(EjectInput[x], thisGrabber.HandSide))
            {
                DetachClip();
                onEjectClip?.Invoke();
                break;
            }
        }
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

            if (audioSource)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
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
        if(audioSource && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public override void OnTrigger(float triggerValue)
    {
        if (triggerValue > triggerThreshold)
        {
            DoJet(triggerValue);
        }
        else
        {
            StopJet();
        }
        
        CheckEjectInput();
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