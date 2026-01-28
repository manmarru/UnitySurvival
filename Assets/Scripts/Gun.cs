using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Gun : MonoBehaviour
    {
        public string GunName; // 총 이름
        public float Range; // 사정거리
        public float Accuracy; // 정확도
        public float FireRate; // 연사속도
        public float ReloadTime; // 재장전 속도

        public int Damage; // 총의 데미지
 
        public int ReloadBulletCount; // 총알 재장전 개수
        public int CurrentBulletCount; // 현재 탄알집에 남아있는 총알
        public int MaxBulletCount; // 최대 소유가능 총알 개수
        public int CarryBulletCount; // 현재 소유하고 있는 총알 개수

        public float RetroActionForce; // 반동 세기
        public float RetroActionFineSightForce; // 정조준시의 반동 세기

        public Vector3 fineSightOriginPos;

        public Animator anim;
        public ParticleSystem MuzzleFlash;

        public AudioClip Fire_sound;

    }
}