using System.Collections;
using UnityEngine;

public class MudGenerator : MonoBehaviour
{
    [SerializeField] private Transform[] sources;
    [SerializeField] private Brush brush;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        Paint();
    }

    private void Paint()
    {
        foreach (var source in sources)
        {
            var ray = new Ray(source.position, source.forward);
            PaintTarget.PaintRay(ray, brush);
        }
    }

    private void OnDrawGizmos()
    {
        if (sources == null)
        {
            return;
        }

        foreach (var source in sources)
        {
            if (source == null)
            {
                continue;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawRay(source.position, source.forward);
        }
    }
}