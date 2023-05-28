using UnityEngine;
using UnityEngine.Events;

public class HingePercent : MonoBehaviour
{
    private HingeJoint _hingeJoint;
    public float min, max;
    [SerializeField] private UnityEvent<float> onPercentChange;

    private void Awake()
    {
        _hingeJoint = GetComponent<HingeJoint>();
    }

    public void AngleToPercent(float angle)
    {
        var percent = Mathf.InverseLerp(min, max, angle);
        onPercentChange.Invoke(percent);
    }
}