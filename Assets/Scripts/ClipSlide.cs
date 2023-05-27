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

    // Lock in place for physics
    private bool _lockedInPlace = false;
    private GrabberArea _grabClipArea;
    private float _lastEjectTime;

    private void Awake()
    {
        _grabClipArea = GetComponentInChildren<GrabberArea>();

        // Check to see if we started with a loaded magazine
        if (_heldMagazine != null)
        {
            AttachGrabbableMagazine(_heldMagazine, _heldMagazine.GetComponent<Collider>());
        }
    }

    private void LateUpdate()
    {
        // Are we trying to grab the clip from the weapon
        CheckGrabClipInput();

        // There is a magazine inside the slide. Position it properly
        if (_heldMagazine != null)
        {
            _heldMagazine.transform.parent = transform;

            // Lock in place immediately
            if (_lockedInPlace)
            {
                _heldMagazine.transform.localPosition = Vector3.zero;
                _heldMagazine.transform.localEulerAngles = Vector3.zero;
                return;
            }

            Vector3 localPos = _heldMagazine.transform.localPosition;

            // Make sure magazine is aligned with MagazineSlide
            _heldMagazine.transform.localEulerAngles = Vector3.zero;

            // Only allow Y translation. Don't allow to go up and through clip area
            float localY = localPos.y;
            if (localY > 0)
            {
                localY = 0;
            }

            MoveMagazine(new Vector3(0, localY, 0));

            _magazineDistance = Vector3.Distance(transform.position, _heldMagazine.transform.position);

            bool clipRecentlyGrabbed = Time.time - _heldMagazine.LastGrabTime < 1f;

            // Snap Magazine In Place
            if (_magazineDistance < clipSnapDistance)
            {
                // Snap in place
                if (!_magazineInPlace && !RecentlyEjected() && !clipRecentlyGrabbed)
                {
                    AttachMagazine();
                }

                // Make sure magazine stays in place if not being grabbed
                if (!_heldMagazine.BeingHeld)
                {
                    MoveMagazine(Vector3.zero);
                }
            }
            // Stop aligning clip with slide if we exceed this distance
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
        // No need to check for grabbing a clip out if none exists
        if (_heldMagazine == null || _grabClipArea == null)
        {
            return;
        }

        // Don't grab clip if the weapon isn't being held
        if (attachedWeapon != null && !attachedWeapon.BeingHeld)
        {
            return;
        }

        Grabber nearestGrabber = _grabClipArea.GetOpenGrabber();
        if (_grabClipArea != null && nearestGrabber != null)
        {
            if (nearestGrabber.HandSide == ControllerHand.Left && InputBridge.Instance.LeftGripDown)
            {
                // grab clip
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
        // Drop Item
        var grabber = _heldMagazine.GetPrimaryGrabber();
        _heldMagazine.DropItem(grabber, false, false);

        // Play Sound
        if (clipAttachSound && Time.timeSinceLevelLoad > 0.1f)
        {
            VRUtils.Instance.PlaySpatialClipAt(clipAttachSound, transform.position, 1f);
        }

        // Move to desired location before locking in place
        MoveMagazine(Vector3.zero);

        // Add fixed joint to make sure physics work properly
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

            // If attached to a Raycast weapon, let it know we attached something
            onAttachMagazine?.Invoke(_heldMagazine.gameObject);
        }

        // Don't let anything try to grab the magazine while it's within the weapon
        // We will use a grabbable proxy to grab the clip back out instead
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

        // Remove fixed joint
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

        // Reset Collider
        if (_heldCollider != null)
        {
            _heldCollider.enabled = true;
            _heldCollider = null;
        }

        // Let wep know we detached something
        onDetachMagazine?.Invoke(_heldMagazine.gameObject);

        // Can be grabbed again
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

        StartCoroutine(EjectMagRoutine(ejectedMag));
    }

    private IEnumerator EjectMagRoutine(Grabbable ejectedMag)
    {
        if (ejectedMag != null && ejectedMag.GetComponent<Rigidbody>() != null)
        {
            Rigidbody ejectRigid = ejectedMag.GetComponent<Rigidbody>();

            // Wait before ejecting

            // Move clip down before we eject it
            ejectedMag.transform.parent = transform;

            if (ejectedMag.transform.localPosition.y > -clipSnapDistance)
            {
                ejectedMag.transform.localPosition = new Vector3(0, -0.1f, 0);
            }

            // Eject with physics force
            ejectedMag.transform.parent = null;
            ejectRigid.AddForce(-ejectedMag.transform.up * ejectForce, ForceMode.VelocityChange);

            yield return new WaitForFixedUpdate();
            ejectedMag.transform.parent = null;
        }

        yield return null;
    }

    // Pull out magazine from clip area
    public void OnGrabClipArea(Grabber grabbedBy)
    {
        if (_heldMagazine != null)
        {
            // Store reference so we can eject the clip first
            Grabbable temp = _heldMagazine;

            // Make sure the magazine can be gripped
            _heldMagazine.enabled = true;

            // Eject clip into hand
            DetachMagazine();

            // Now transfer grab to the grabber
            temp.enabled = true;

            grabbedBy.GrabGrabbable(temp);
        }
    }

    public virtual void AttachGrabbableMagazine(Grabbable mag, Collider magCollider)
    {
        _heldMagazine = mag;
        _heldMagazine.transform.parent = transform;

        _heldCollider = magCollider;

        // Disable the collider while we're sliding it in to the weapon
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