using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;

namespace Ptr.Shared.Extensions;

public static class BaseWeaponExtension
{
    extension(IBaseWeapon weapon)
    {
        public void SetClip(int clip, bool setVData = false)
        {
            weapon.Clip = clip;
            if (setVData)
            {
                weapon.GetWeaponData().MaxClip = clip;
            }
        }

        public void SetReserveAmmo(int reserveAmmo, bool setVData = false)
        {
            weapon.ReserveAmmo = reserveAmmo;
            if (setVData)
            {
                weapon.GetWeaponData().PrimaryReserveAmmoMax = reserveAmmo;
            }
        }
    }

    extension(IBaseWeapon weapon)
    {
        public bool IsGrenade
        {
            get
            {
                var itemDef = (EconItemId)weapon.ItemDefinitionIndex;
                return itemDef is
                    EconItemId.Flashbang or
                    EconItemId.Hegrenade or
                    EconItemId.SmokeGrenade or
                    EconItemId.Decoy or
                    EconItemId.IncGrenade;
            }
        }

        public bool IsProjectile => weapon.IsGrenade;

        public bool IsItem
        {
            get
            {
                var itemDef = (EconItemId)weapon.ItemDefinitionIndex;
                return itemDef is
                    EconItemId.Kevlar or
                    EconItemId.Defuser or
                    EconItemId.HeavyAssaultSuit or
                    EconItemId.Cutters;
            }
        }

        public bool IsPistol
        {
            get
            {
                var itemDef = (EconItemId)weapon.ItemDefinitionIndex;
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

        public bool IsSmg
        {
            get
            {
                var itemDef = (EconItemId)weapon.ItemDefinitionIndex;
                return itemDef is
                    EconItemId.Mp5Sd or
                    EconItemId.Mac10 or
                    EconItemId.Ump45 or
                    EconItemId.Mp9 or
                    EconItemId.Bizon or
                    EconItemId.Mp7 or
                    EconItemId.P90;
            }
        }

        public bool IsSniper
        {
            get
            {
                var itemDef = (EconItemId)weapon.ItemDefinitionIndex;
                return itemDef is
                    EconItemId.Awp or
                    EconItemId.G3Sg1 or
                    EconItemId.Scar20;
            }
        }

        public bool IsShotgun
        {
            get
            {
                var itemDef = (EconItemId)weapon.ItemDefinitionIndex;
                return itemDef is
                    EconItemId.Xm1014 or
                    EconItemId.Nova or
                    EconItemId.Sawedoff or
                    EconItemId.Mag7;
            }
        }

        public bool IsMachineGun
        {
            get
            {
                var itemDef = (EconItemId)weapon.ItemDefinitionIndex;
                return itemDef is
                    EconItemId.Negev or
                    EconItemId.M249;
            }
        }

        public bool IsAutoRifle
        {
            get
            {
                var itemDef = (EconItemId)weapon.ItemDefinitionIndex;
                return itemDef is
                    EconItemId.Ak47 or
                    EconItemId.M4A1 or
                    EconItemId.M4A1Silencer or
                    EconItemId.GalilAR or
                    EconItemId.Famas or
                    EconItemId.Sg556 or
                    EconItemId.Aug;
            }
        }
    }
}