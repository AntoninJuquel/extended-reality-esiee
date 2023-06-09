using UnityEngine;
using UnityEngine.Events;

public class CollisionPainter : MonoBehaviour
{
    public Brush brush;
    public bool RandomChannel = false;
    public UnityEvent<Collision> onPaint;
    private void Start()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            PaintTarget paintTarget = contact.otherCollider.GetComponent<PaintTarget>();
            if (paintTarget != null)
            {
                if (RandomChannel) brush.splatChannel = Random.Range(0, 4);
                PaintTarget.PaintObject(paintTarget, contact.point, contact.normal, brush);
                onPaint?.Invoke(collision);
            }
        }
    }
}