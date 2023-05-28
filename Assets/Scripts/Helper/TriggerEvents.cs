using UnityEngine;
using UnityEngine.Events;

public class TriggerEvents : MonoBehaviour
{
    [SerializeField] private UnityEvent<GameObject> onTriggerEnter, onTriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter?.Invoke(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        onTriggerExit?.Invoke(other.gameObject);
    }
}