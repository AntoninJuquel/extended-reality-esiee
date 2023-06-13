using BNG;
using UnityEngine;

namespace Trash
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Grabbable))]
    [RequireComponent(typeof(CollisionSound))]
    public class TrashGarbage : MonoBehaviour
    {
        [field: SerializeField] public TrashCategory TrashCategory { get; private set; }

        private void OnValidate()
        {
            var meshCollider = GetComponent<MeshCollider>();
            meshCollider.convex = true;

            var grabbable = GetComponent<Grabbable>();
            grabbable.Grabtype = HoldType.HoldDown;
            grabbable.CanBeSnappedToSnapZone = false;
            grabbable.handPoseType = HandPoseType.AutoPoseOnce;
        }

        private void Start()
        {
            var trashBins = FindObjectsOfType<TrashBin>();
            foreach (var trashBin in trashBins)
            {
                trashBin.NewTrash(this);
            }
        }
    }
}