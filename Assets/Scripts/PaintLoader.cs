using System.Collections;
using UnityEngine;

public class PaintLoader : MonoBehaviour
{
    [SerializeField] private Brush brush;
    private Mesh _mesh;
    private PaintTarget _paintTarget;

    private void Awake()
    {
        _paintTarget = GetComponent<PaintTarget>();
    }

    private IEnumerator Start()
    {
        if (!_paintTarget) yield break;

        yield return new WaitForEndOfFrame();
        Paint();
    }

    [ContextMenu("Paint")]
    private void Paint()
    {
        _mesh = _paintTarget.GetComponent<MeshFilter>().mesh;
        for (var i = 0; i < _mesh.vertices.Length; i++)
        {
            var point = _mesh.vertices[i] + _paintTarget.transform.position;
            var normal = _mesh.normals[i];
            PaintTarget.PaintObject(_paintTarget, point, normal, brush);
        }
    }
}