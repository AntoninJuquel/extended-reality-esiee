using System.Collections.Generic;
using UnityEngine;

public class WaterFiller : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterParticleSystem;
    [SerializeField] private float fillRate, maxFillRate = 1f;
    private HashSet<WaterClip> _waterClips = new HashSet<WaterClip>();

    bool IsFilling => fillRate > 0;

    public void AddWaterClip(GameObject waterClip)
    {
        if (waterClip.TryGetComponent(out WaterClip clip))
        {
            _waterClips.Add(clip);
        }
    }

    public void RemoveWaterClip(GameObject waterClip)
    {
        if (waterClip.TryGetComponent(out WaterClip clip))
        {
            _waterClips.Remove(clip);
        }
    }

    public void SetFillRate(float percent)
    {
        fillRate = Mathf.Lerp(0, maxFillRate, percent);

        if (waterParticleSystem)
        {
            if (percent > 0 && !waterParticleSystem.isPlaying)
            {
                waterParticleSystem.Play();
            }
            else if (percent <= 0 && waterParticleSystem.isPlaying)
            {
                waterParticleSystem.Stop();
            }
        }
    }

    private void Update()
    {
        if (!IsFilling) return;

        foreach (var waterClip in _waterClips)
        {
            waterClip.FillWater(fillRate * Time.deltaTime);
        }
    }
}