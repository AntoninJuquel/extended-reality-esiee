using UnityEngine;

public class LinearRotation : MonoBehaviour
{
    [SerializeField] private Transform toRotate;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        toRotate.Rotate(rotationAxis, speed * Time.deltaTime);
    }
}