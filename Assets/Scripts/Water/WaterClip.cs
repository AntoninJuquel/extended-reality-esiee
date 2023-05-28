using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WaterClip : MonoBehaviour
{
    [SerializeField] private float maxAmount = 1f, waterAmount = 1f;
    [SerializeField] private Image[] waterLevelImages;
    [SerializeField] private Text waterLevelText;
    [SerializeField] private UnityEvent onClippedIn, onClippedOut;
    public float WaterPercent => waterAmount / maxAmount;
    public bool isEmpty => waterAmount <= 0f;

    private void Start()
    {
        UpdateUi();
    }

    private void UpdateUi()
    {
        if (waterLevelImages != null)
        {
            foreach (var image in waterLevelImages)
            {
                image.fillAmount = WaterPercent;
            }
        }

        if (waterLevelText != null)
        {
            waterLevelText.text = $"{WaterPercent:P0}";
        }
    }

    public void DrainWater(float amount)
    {
        waterAmount = Mathf.Clamp(waterAmount - amount, 0f, maxAmount);
        UpdateUi();
    }

    public void FillWater(float amount)
    {
        waterAmount = Mathf.Clamp(waterAmount + amount, 0f, maxAmount);
        UpdateUi();
    }

    public void ClipIn()
    {
        onClippedIn?.Invoke();
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void ClipOut()
    {
        onClippedOut?.Invoke();
        GetComponent<Rigidbody>().isKinematic = false;
    }
}