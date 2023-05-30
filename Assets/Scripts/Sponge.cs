using UnityEngine;
using UnityEngine.UI;

public class Sponge : MonoBehaviour
{
    private int dirtiness = 0;
    public float dirtinessThreshold = 100f;
    public Text text;
    public Brush brush;
    private CollisionPainter _collisionPainter;
    private PaintTarget _paintTarget;

    private void Awake()
    {
        _collisionPainter = GetComponent<CollisionPainter>();
        _paintTarget = GetComponent<PaintTarget>();
    }
    public void DirtySponge()
    {
        dirtiness++;
        brush.splatScale = dirtiness;
        text.text = dirtiness.ToString();
        PaintTarget.PaintObject(_paintTarget, transform.position, Vector3.up, brush);
        if (dirtiness >= dirtinessThreshold)
        {
            _collisionPainter.enabled = false;
        }
    }
}
