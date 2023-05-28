using System.Collections;
using UnityEngine;
using BNG;
using UnityEngine.Events;

public class ClipSlide : MonoBehaviour
{
    /// <summary>
    /// Clip transform name must contain this to be considered valid
    /// </summary>
    [Tooltip("Clip transform name must contain this to be considered valid")]
    public string acceptableMagazineName = "Clip";

    /// <summary>
    /// The weapon this magazine is attached to (optional)
    /// </summary>RaycastWeapon
    [SerializeField] private Grabbable attachedWeapon;

    [SerializeField] private float clipSnapDistance = 0.075f;
    [SerializeField] private float clipUnsnapDistance = 0.15f;
    [SerializeField] private float ejectForce = 1f;
    [SerializeField] private AudioClip clipAttachSound;
    [SerializeField] private AudioClip clipDetachSound;
    [SerializeField] private UnityEvent<GameObject> onAttachMagazine;
    [SerializeField] private UnityEvent<GameObject> onDetachMagazine;

    private Grabbable _heldMagazine = null;
    private Collider _heldCollider = null;
    private float _magazineDistance = 0f;

    private bool _magazineInPlace = false;

    private bool _lockedInPlace = false;
    private GrabberArea _grabClipArea;
    private float _lastEjectTime;

    private void Awake()
    {
        _grabClipArea = GetComponentInChildren<GrabberArea>();

        if (_heldMagazine != null)
        {
            AttachGrabbableMagazine(_heldMagazine, _heldMagazine.GetComponent<Collider>());
        }
    }

    private void LateUpdate()
    {
        CheckGrabClipInput();

        if (_heldMagazine != null)
        {
            _heldMagazine.transform.parent = transform;

            if (_lockedInPlace)
            {
                _heldMagazine.transform.localPosition = Vector3.zero;
                _heldMagazine.transform.localEulerAngles = Vector3.zero;
                return;
            }

            Vector3 localPos = _heldMagazine.transform.localPosition;

            _heldMagazine.transform.localEulerAngles = Vector3.zero;

            float localY = localPos.y;
            if (localY > 0)
            {
                localY = 0;
            }

            MoveMagazine(new Vector3(0, localY, 0));

            _magazineDistance = Vector3.Distance(transform.position, _heldMagazine.transform.position);

            bool clipRecentlyGrabbed = Time.time - _heldMagazine.LastGrabTime < 1f;

            if (_magazineDistance < clipSnapDistance)
            {
                if (!_magazineInPlace && !RecentlyEjected() && !clipRecentlyGrabbed)
                {
                    AttachMagazine();
                }

                if (!_heldMagazine.BeingHeld)
                {
                    MoveMagazine(Vector3.zero);
                }
            }
            else if (_magazineDistance >= clipUnsnapDistance && !RecentlyEjected())
            {
                DetachMagazine();
            }
        }
    }

    private bool RecentlyEjected()
    {
        return Time.time - _lastEjectTime < 0.1f;
    }

    private void MoveMagazine(Vector3 localPosition)
    {
        _heldMagazine.transform.localPosition = localPosition;
    }

    public void CheckGrabClipInput()
    {
        if (_heldMagazine == null || _grabClipArea == null)
        {
            return;
        }

        if (attachedWeapon != null && !attachedWeapon.BeingHeld)
        {
            return;
        }

        Grabber nearestGrabber = _grabClipArea.GetOpenGrabber();
        if (_grabClipArea != null && nearestGrabber != null)
        {
            if (nearestGrabber.HandSide == ControllerHand.Left && InputBridge.Instance.LeftGripDown)
            {
                OnGrabClipArea(nearestGrabber);
            }
            else if (nearestGrabber.HandSide == ControllerHand.Right && InputBridge.Instance.RightGripDown)
            {
                OnGrabClipArea(nearestGrabber);
            }
        }
    }

    private void AttachMagazine()
    {
        var grabber = _heldMagazine.GetPrimaryGrabber();
        _heldMagazine.DropItem(grabber, false, false);

        if (clipAttachSound && Time.timeSinceLevelLoad > 0.1f)
        {
            VRUtils.Instance.PlaySpatialClipAt(clipAttachSound, transform.position, 1f);
        }

        MoveMagazine(Vector3.zero);

        if (transform.parent != null)
        {
            Rigidbody parentRb = transform.parent.GetComponent<Rigidbody>();
            if (parentRb)
            {
                FixedJoint fj = _heldMagazine.gameObject.AddComponent<FixedJoint>();
                fj.autoConfigureConnectedAnchor = true;
                fj.axis = new Vector3(0, 1, 0);
                fj.connectedBody = parentRb;
            }

            onAttachMagazine?.Invoke(_heldMagazine.gameObject);
        }

        _heldMagazine.enabled = false;

        _lockedInPlace = true;
        _magazineInPlace = true;
    }

    /// <summary>
    /// Detach Magazine from it's parent. Removes joint, re-enables collider, and calls events
    /// </summary>
    /// <returns>Returns the magazine that was ejected or null if no magazine was attached</returns>
    private Grabbable DetachMagazine()
    {
        if (_heldMagazine == null)
        {
            return null;
        }

        VRUtils.Instance.PlaySpatialClipAt(clipDetachSound, transform.position, 1f, 0.9f);

        _heldMagazine.transform.parent = null;

        if (transform.parent != null)
        {
            Rigidbody parentRb = transform.parent.GetComponent<Rigidbody>();
            if (parentRb)
            {
                FixedJoint fj = _heldMagazine.gameObject.GetComponent<FixedJoint>();
                if (fj)
                {
                    fj.connectedBody = null;
                    Destroy(fj);
                }
            }
        }

        if (_heldCollider != null)
        {
            _heldCollider.enabled = true;
            _heldCollider = null;
        }

        onDetachMagazine?.Invoke(_heldMagazine.gameObject);

        _heldMagazine.enabled = true;
        _magazineInPlace = false;
        _lockedInPlace = false;
        _lastEjectTime = Time.time;

        var returnGrab = _heldMagazine;
        _heldMagazine = null;

        return returnGrab;
    }

    public void EjectMagazine()
    {
        Grabbable ejectedMag = DetachMagazine();
        _lastEjectTime = Time.time;

        ejectedMag.transform.parent = null;
        ejectedMag.GetComponent<Rigidbody>().AddForce(-ejectedMag.transform.up * ejectForce, ForceMode.Impulse);
    }


    public void OnGrabClipArea(Grabber grabbedBy)
    {
        if (_heldMagazine != null)
        {
            Grabbable temp = _heldMagazine;

            _heldMagazine.enabled = true;

            DetachMagazine();

            temp.enabled = true;

            grabbedBy.GrabGrabbable(temp);
        }
    }

    public virtual void AttachGrabbableMagazine(Grabbable mag, Collider magCollider)
    {
        _heldMagazine = mag;
        _heldMagazine.transform.parent = transform;

        _heldCollider = magCollider;

        if (_heldCollider != null)
        {
            _heldCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabbable grab = other.GetComponent<Grabbable>();
        if (_heldMagazine == null && grab != null && grab.transform.name.Contains(acceptableMagazineName))
        {
            AttachGrabbableMagazine(grab, other);
        }
    }
}