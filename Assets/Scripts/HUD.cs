using Assets.Scripts;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    //필요한 컴포넌트
    [SerializeField]
    private GunController theGunController;
    private Gun currentGun;

    // 필요하면 HUD 호출, 필요없으면 HUD 비활성화
    [SerializeField]
    private GameObject go_BulletHUD;

    //총알 개수 텍스트에 반영
    [SerializeField]
    private TextMeshProUGUI[] Text_Bullet;

    void Update()
    {
        CheckBullet();
    }

    private void CheckBullet()
    {
        currentGun = theGunController.GetGun();
        Text_Bullet[0].text = currentGun.CarryBulletCount.ToString();
        Text_Bullet[1].text = currentGun.ReloadBulletCount.ToString();
        Text_Bullet[2].text = currentGun.CurrentBulletCount.ToString();
    }
}
