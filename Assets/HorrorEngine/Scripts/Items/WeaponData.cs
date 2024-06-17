using UnityEngine;

namespace HorrorEngine
{

    [CreateAssetMenu(menuName = "Horror Engine/Items/Weapon")]
    public class WeaponData : EquipableItemData
    {
        public AnimatorStateHandle AimingAnim;
        public AnimatorStateHandle ReloadAnim;
        public float ReloadDuration;
    }
}