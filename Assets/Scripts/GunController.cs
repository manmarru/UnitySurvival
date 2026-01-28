using Assets.Scripts;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [SerializeField]
    private Gun currentGun;
    private float currentFireRate;

    private AudioSource audioSource;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
        GunFireRateClac();
        TryFire();
    }

    private void GunFireRateClac()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime; // 60분의  1 = 1
        }
    }

    private void TryFire()
    {
        if (Mouse.current.leftButton.isPressed && currentFireRate <= 0)
        {
            Fire();
        }
    }

    private void Fire()
    {
        currentFireRate = currentGun.FireRate;
        Shoot();
    }
    private void Shoot()
    {
        PlaySE(currentGun.Fire_sound);
        currentGun.MuzzleFlash.Play();
        Debug.Log("발사");
    }

    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
