using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Weapon : MonoBehaviour
{
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 0.3f;

    public float spreadIntensity = 0.075f;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 500f;
    public float bulletPrefabLifeTime = 3f;

    public GameObject muzzleEffect;
    private Animator animator;

    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;


    public enum ShootingMode
    {
        Single,
        Auto
    }

    public ShootingMode currentShootingMode;

    private void Awake()
    {
        readyToShoot = true;
        animator = GetComponent<Animator>();
        bulletsLeft = magazineSize;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (currentShootingMode == ShootingMode.Auto)
            {
                currentShootingMode = ShootingMode.Single;
            }
            else
            {
                currentShootingMode = ShootingMode.Auto;
            }
        }

        if (bulletsLeft == 0 && isShooting)
        {
            SoundManager.Instance.emptyMagazineSoundSciFiHandGun.Play();
        }

        if (currentShootingMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (currentShootingMode == ShootingMode.Single)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading)
        {
            Reload();
        }

        if (readyToShoot && !isShooting && !isReloading && bulletsLeft <= 0)
        {
            SoundManager.Instance.emptyMagazineSoundSciFiHandGun.Play();
            Invoke("Reload", 0.3f);
        }



        if (readyToShoot && isShooting && bulletsLeft > 0)
        {
            FireWeapon();
        }

        if (AmmoManager.Instance.ammoDisplay != null)
        {
            AmmoManager.Instance.ammoDisplay.text = $"{bulletsLeft}/{magazineSize}\n{currentShootingMode}";
        }
    }

    private void FireWeapon()
    {
        bulletsLeft--;

        muzzleEffect.GetComponent<ParticleSystem>().Play();
        animator.SetTrigger("RECOIL");

        SoundManager.Instance.shootingSoundSciFiHandGun.Play();

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;


        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        
        bullet.transform.forward = shootingDirection;
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }
    }

    private void Reload()
    {
        SoundManager.Instance.reloadingSoundSciFiHandGun.Play();
        isReloading = true;
        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted()
    {
        bulletsLeft = magazineSize;
        isReloading = false;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - bulletSpawn.position;

        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
