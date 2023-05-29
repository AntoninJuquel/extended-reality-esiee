using UnityEngine;
using UnityEngine.Events;

public class Toggler : MonoBehaviour
{
    [SerializeField] private bool isOn;
    [SerializeField] private UnityEvent<bool> onToggle;
    [SerializeField] private UnityEvent onToggleOn, onToggleOff;

    public void ToggleValue()
    {
        isOn = !isOn;
        onToggle?.Invoke(isOn);
        if (isOn)
        {
            onToggleOn?.Invoke();
        }
        else
        {
            onToggleOff?.Invoke();
        }
    }
}