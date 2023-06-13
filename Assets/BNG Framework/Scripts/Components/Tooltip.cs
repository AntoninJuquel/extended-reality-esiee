using UnityEngine;

namespace BNG
{
    public class Tooltip : MonoBehaviour
    {
        /// <summary>
        /// Offset from Object we are providing tip to
        /// </summary>
        public Vector3 TipOffset = new Vector3(1.5f, 0.2f, 0);

        /// <summary>
        /// If true Y axis will be in World Coordinates. False for local coords.
        /// </summary>
        public bool UseWorldYAxis = true;

        /// <summary>
        /// Hide the tooltip if Camera is farther away than this. In meters.
        /// </summary>
        public float MaxViewDistance = 10f;

        /// <summary>
        /// Hide this if farther than MaxViewDistance
        /// </summary>
        Transform childTransform;

        public Transform DrawLineTo;
        LineToTransform lineTo;
        Transform lookAt;
        Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        void Start()
        {
            lookAt = mainCamera.transform;
            lineTo = GetComponentInChildren<LineToTransform>();

            childTransform = transform.GetChild(0);

            if (DrawLineTo && lineTo)
            {
                lineTo.ConnectTo = DrawLineTo;
            }
        }

        void Update()
        {
            UpdateTooltipPosition();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, MaxViewDistance);
        }

        public virtual void UpdateTooltipPosition()
        {
            if (lookAt)
            {
                transform.LookAt(mainCamera.transform);
            }
            else if (mainCamera != null)
            {
                lookAt = mainCamera.transform;
            }
            else if (mainCamera == null)
            {
                mainCamera = Camera.main;
                return;
            }

            transform.parent = DrawLineTo;
            transform.localPosition = TipOffset;

            if (UseWorldYAxis)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
                transform.position += new Vector3(0, TipOffset.y, 0);
            }

            if (childTransform)
            {
                childTransform.gameObject.SetActive(
                    Vector3.Distance(transform.position, mainCamera.transform.position) <= MaxViewDistance);
            }
        }
    }
}