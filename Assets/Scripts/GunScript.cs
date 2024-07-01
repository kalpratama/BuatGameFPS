using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GunScript : MonoBehaviour
{
    // Public variables
    public int damage = 10;
    public float range = 100f;
    public float fireRate = 15f;
    public int maxAmmo = 10;
    public float reloadTime = 1f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public Animator gunAnimator; // Add reference to the Animator

    // Private variables
    private PlayerInput playerInput;
    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private PlayerInput.OnFootActions onFoot;

    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        // Setup the input system
        onFoot.Shoot.performed += ctx => Shoot();
        onFoot.Reload.performed += ctx => StartCoroutine(Reload());
    }

    void OnEnable()
    {
        onFoot.Enable();
    }

    void OnDisable()
    {
        onFoot.Disable();
    }

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Time.time >= nextTimeToFire)
        {
            if (onFoot.Shoot.triggered)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void Shoot()
    {
        if (isReloading || currentAmmo <= 0)
            return;

        muzzleFlash.Play(); // Trigger muzzle flash effect

        gunAnimator.SetTrigger("Shoot"); // Trigger shooting animation

        currentAmmo--;

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 2f);
        }
    }
}
