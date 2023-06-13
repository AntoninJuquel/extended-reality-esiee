using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trash
{
    public class TrashBin : MonoBehaviour
    {
        [SerializeField] private Text trashCountText;
        [SerializeField] private TrashCategory trashCategory;
        [SerializeField] private UnityEvent onTrashThrown, onFinalTrashThrown;
        [SerializeField] private float repelForce = 2f;
        private HashSet<TrashGarbage> _outside = new(), _inside = new();
        private Rigidbody _rigidbody;
        private int InsideTrash => _inside.Count;
        private int OutsideTrash => _outside.Count;
        private int TotalTrash => OutsideTrash + InsideTrash;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Open()
        {
            _rigidbody.isKinematic = true;
        }

        public void Close()
        {
            _rigidbody.isKinematic = false;
        }

        public void NewTrash(TrashGarbage trashGarbage)
        {
            if (trashGarbage.TrashCategory != trashCategory)
            {
                return;
            }

            _outside.Add(trashGarbage);

            if (trashCountText)
            {
                trashCountText.text = $"{InsideTrash}/{TotalTrash}";
            }
        }

        public void ThrowTrash(GameObject trash)
        {
            var trashGarbage = trash.GetComponent<TrashGarbage>();
            
            if (trashGarbage == null)
            {
                var rigidbody = trash.GetComponent<Rigidbody>();
                if (rigidbody)
                {
                    rigidbody.AddForce(Vector3.up * repelForce, ForceMode.Impulse);
                }
                return;
            }

            if (trashGarbage.TrashCategory != trashCategory)
            {
                var rigidbody = trash.GetComponent<Rigidbody>();
                if (rigidbody)
                {
                    rigidbody.AddForce(Vector3.up * repelForce, ForceMode.Impulse);
                }

                return;
            }

            var isOutSide = _outside.Contains(trashGarbage);
            if (!isOutSide)
            {
                return;
            }

            var isInside = _inside.Contains(trashGarbage);
            if (isInside)
            {
                return;
            }

            _outside.Remove(trashGarbage);
            _inside.Add(trashGarbage);
            onTrashThrown?.Invoke();

            if (trashCountText)
            {
                trashCountText.text = $"{InsideTrash}/{TotalTrash}";
            }

            if (InsideTrash >= TotalTrash)
            {
                onFinalTrashThrown?.Invoke();
            }
        }

        public void RemoveTrash(GameObject trash)
        {
            var trashGarbage = trash.GetComponent<TrashGarbage>();
            if (trashGarbage == null)
            {
                return;
            }

            if (!_inside.Contains(trashGarbage))
            {
                return;
            }

            if (_outside.Contains(trashGarbage))
            {
                return;
            }


            if (trashGarbage.TrashCategory != trashCategory)
            {
                return;
            }

            _inside.Remove(trashGarbage);
            _outside.Add(trashGarbage);

            if (trashCountText)
            {
                trashCountText.text = $"{InsideTrash}/{TotalTrash}";
            }
        }
    }
}