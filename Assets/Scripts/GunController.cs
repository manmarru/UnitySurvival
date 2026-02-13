using Assets.Scripts;
using System;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    // 활성화 여부
    public static bool isActivate = false;

    // 현재 장착된 총
    [SerializeField]
    private Gun currentGun;
    
    // 연사속도 계산
    private float currentFireRate;

    // 상태 변수
    private bool isReload = false;
    [HideInInspector]
    private bool isFineSightMode = false;

    //본래 포지션 값
    [SerializeField]
    private Vector3 OriginPos;

    // 효과음 재생
    private AudioSource audioSource;

    // 레이 충돌 정보 받아옴
    private RaycastHit hitInfo;

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCam;
    private Crosshair theCrosshair;

    // 피격 이펙트
    [SerializeField]
    private GameObject hit_effect_prefab;


    void Start()
    {
        OriginPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        theCrosshair = FindAnyObjectByType<Crosshair>();

        WeaponManager.currentWeapon = currentGun.transform;
        WeaponManager.currentWeaponAnim = currentGun.anim;
    }

    void Update()
    {
        if (isActivate)
        {
            GunFireRateClac();
            TryFire();
            TryReload();
            TryFineSight();
        }
    }

    // 연사속도 재계산
    private void GunFireRateClac()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime; // 60분의  1 = 1
        }
    }

    // 발사 시도
    private void TryFire()
    {
        if (Mouse.current.leftButton.isPressed && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    // 발사 전 계산
    private void Fire()
    {
        if (!isReload)
        {
            if (currentGun.CurrentBulletCount > 0)
            {
                Shoot();
            }
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());
            }
        }
    }

    // 발사 후 계산
    private void Shoot()
    {
        theCrosshair.FireAnimation();
        currentGun.CurrentBulletCount--;
        currentFireRate = currentGun.FireRate; // 연사 속도 재계산.
        PlaySE(currentGun.Fire_sound);
        currentGun.MuzzleFlash.Play();
        Hit();
        StopAllCoroutines();
        StartCoroutine(RetroActionCoroutune());
    }

    private void Hit()
    {
        if(Physics.Raycast(theCam.transform.position, theCam.transform.forward + 
            new Vector3(UnityEngine.Random.Range(-theCrosshair.GetAccuracy() - currentGun.Accuracy, theCrosshair.GetAccuracy()) + currentGun.Accuracy
                        , UnityEngine.Random.Range(-theCrosshair.GetAccuracy() - currentGun.Accuracy, theCrosshair.GetAccuracy()) + currentGun.Accuracy
                         , 0)
            , out hitInfo, currentGun.Range))
        {
            var clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
        }
    }

    // 재장전 시도
    private void TryReload()
    {
        if(Keyboard.current.rKey.isPressed && !isReload && currentGun.CurrentBulletCount < currentGun.ReloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }

    // 재장전
    IEnumerator ReloadCoroutine()
    {
        if(currentGun.CarryBulletCount > 0)
        {
            isReload = true;

            currentGun.anim.SetTrigger("Reload");

            currentGun.CarryBulletCount += currentGun.CurrentBulletCount;
            currentGun.CurrentBulletCount = 0;

            yield return new WaitForSeconds(currentGun.ReloadTime);

            if(currentGun.CarryBulletCount >= currentGun.ReloadBulletCount)
            {
                currentGun.CurrentBulletCount = currentGun.ReloadBulletCount;
                currentGun.CarryBulletCount -= currentGun.ReloadBulletCount;
            }
            else
            {
                currentGun.CurrentBulletCount = currentGun.CarryBulletCount;
                currentGun.CarryBulletCount = 0;
            }

            isReload = false;
        }
        else
        {
            Debug.Log("소유한 총알이 없음");
        }
    }

    public void CancelReload()
    {
        if(isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }

    // 정조준 시도
    private void TryFineSight()
    {
        if(Mouse.current.rightButton.wasPressedThisFrame && !isReload)
        {
            FineSight();
        }
    }

    // 정조준 취소
    public void CancelFineSight()
    {
        if(isFineSightMode)
        {
            FineSight();
        }
    }

    // 정조준 로직
    private void FineSight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode);
        theCrosshair.FineSightAnimation(isFineSightMode);

        if(isFineSightMode)
        {
            StopAllCoroutines();
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeActivateCoroutine());
        }
    }

    // 정조준 활성화
    IEnumerator FineSightActivateCoroutine()
    {
        while(currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null; // 매프레임 실행
        }
    }

    // 정조준 비활성화
    IEnumerator FineSightDeActivateCoroutine()
    {
        while (currentGun.transform.localPosition != OriginPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, OriginPos, 0.2f);
            yield return null;
        }
    }

    // 반동
    IEnumerator RetroActionCoroutune()
    {
        Vector3 recoliBack = new Vector3(currentGun.RetroActionForce, OriginPos.y, OriginPos.z);
        Vector3 retroActionRecoilBack = new Vector3(currentGun.RetroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z);

        if (!isFineSightMode)
        {
            currentGun.transform.localPosition = OriginPos;

            //반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.RetroActionForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoliBack, 0.4f);
                yield return null;
            }

            //원위치
            while (currentGun.transform.localPosition != OriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, OriginPos, 0.1f);
                yield return null;
            }
        }
        else
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;

            //반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.RetroActionFineSightForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            //원위치
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
    }

    // 사운드 재생
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    public Gun GetGun()
    {
        return currentGun;
    }

    public bool GetFineSightMode()
    {
        return isFineSightMode;
    }

    public void GunChange(Gun _gun)
    {
        if(WeaponManager.currentWeapon != null)
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false);
        }

        currentGun = _gun;
        WeaponManager.currentWeapon = currentGun.transform;
        WeaponManager.currentWeaponAnim = currentGun.anim;


        currentGun.transform.localPosition = Vector3.zero;
        currentGun.gameObject.SetActive(true);
        isActivate = true;
    }
}
