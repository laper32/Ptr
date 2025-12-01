using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;

namespace Ptr.Shared.Extensions;

public static class BaseWeaponExtension
{
    extension(IBaseWeapon weapon)
    {
        public void SetClip(int clip, bool setMaxClip = true)
        {
            weapon.Clip = clip;
            if (setMaxClip)
            {
                weapon.GetWeaponData().MaxClip = clip;
            }
        }

        public void SetReserveAmmo(int reserveAmmo, bool setWeaponData = true)
        {
            weapon.ReserveAmmo = reserveAmmo;
            if (setWeaponData)
            {
                weapon.GetWeaponData().PrimaryReserveAmmoMax = reserveAmmo;
            }
        }
    }

    extension(IBaseWeapon weapon)
    {


        public bool IsShotgun
        {
            get
            {
                var itemDef = weapon.ItemDefinitionIndex.Cast<EconItemId>();
                return itemDef is
                    EconItemId.Xm1014 or
                    EconItemId.Nova or
                    EconItemId.Sawedoff or
                    EconItemId.Mag7;
            }
        }

        public bool IsSniper
        {
            get
            {
                var itemDef = weapon.ItemDefinitionIndex.Cast<EconItemId>();
                return itemDef is
                    EconItemId.Awp or
                    EconItemId.G3Sg1 or
                    EconItemId.Scar20;
            }
        }

        public bool IsPistol
        {
            get
            {
                var itemDef = weapon.ItemDefinitionIndex.Cast<EconItemId>();
                return itemDef is
                    EconItemId.Deagle or
                    EconItemId.Elite or
                    EconItemId.FiveSeven or
                    EconItemId.Glock or
                    EconItemId.Hkp2000 or
                    EconItemId.P250 or
                    EconItemId.Tec9 or
                    EconItemId.UspSilencer or
                    EconItemId.Cz75A or
                    EconItemId.Revolver;
            }
        }
    }
}