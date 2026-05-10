using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(XRGrabInteractable))]
public class RayCastShoot : MonoBehaviour
{
    public int gunDamage = 1;
    public float fireRate = 0.25f;
    public float weaponRange = 50f;
    public float hitForce = 100f;
    public Transform gunEnd;

    private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);
    private AudioSource gunAudio;
    private LineRenderer laserLine;
    private float nextFire;

    private XRGrabInteractable _grabbable;
    private Transform _heldByTransform;
    private InputAction _activeTrigger;

    [Header("Tutorial")]
    public GameObject instructionUI;
    private bool _pickedUpOnce = false;

    void Awake()
    {
        laserLine = GetComponent<LineRenderer>();
        gunAudio = GetComponent<AudioSource>();
        _grabbable = GetComponent<XRGrabInteractable>();
        _grabbable.selectEntered.AddListener(OnGrabbed);
        _grabbable.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        _grabbable.selectEntered.RemoveListener(OnGrabbed);
        _grabbable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _heldByTransform = args.interactorObject.transform;

        var controller = args.interactorObject.transform
            .GetComponent<ActionBasedController>();
        if (controller != null)
            _activeTrigger = controller.activateAction.action;

        if (!_pickedUpOnce)
        {
            _pickedUpOnce = true;
            if (instructionUI != null)
                instructionUI.SetActive(false);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        _heldByTransform = null;
        _activeTrigger = null;
    }

    void Update()
    {
        if (_heldByTransform == null) return;
        if (_activeTrigger == null || !_activeTrigger.WasPressedThisFrame()) return;
        if (Time.time <= nextFire) return;

        nextFire = Time.time + fireRate;
        StartCoroutine(ShotEffect());

        Vector3 rayOrigin = gunEnd.position;
        Vector3 rayDirection = gunEnd.forward;
        RaycastHit hit;

        laserLine.SetPosition(0, gunEnd.position);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, weaponRange))
        {
            laserLine.SetPosition(1, hit.point);

            UFOTarget health = hit.collider.GetComponent<UFOTarget>();
            if (health != null)
                health.Damage(gunDamage);

            if (hit.rigidbody != null)
                hit.rigidbody.AddForce(-hit.normal * hitForce);
        }
        else
        {
            laserLine.SetPosition(1, rayOrigin + rayDirection * weaponRange);
        }
    }

    private IEnumerator ShotEffect()
    {
        gunAudio.Play();
        laserLine.enabled = true;
        yield return shotDuration;
        laserLine.enabled = false;
    }
}