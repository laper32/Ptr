using Ptr.Shared.Bridge;
using Sharp.Shared.GameEntities;

namespace Ptr.Shared.Extensions;

public static class BaseEntityExtensions
{
    extension(IBaseEntity self)
    {
        public void SetAnimationLooping(string anim)
        {
            self.AcceptInput("SetAnimationLooping", value: anim);
        }

        public void SetAnimationNoResetLooping(string anim)
        {
            self.AcceptInput("SetAnimationNoResetLooping", value: anim);
        }

        public void SetIdleAnimationLooping(string anim)
        {
            self.AcceptInput("SetIdleAnimationLooping", value: anim);
        }

        public void SetAnimationNotLooping(string anim)
        {
            self.AcceptInput("SetAnimationNotLooping", value: anim);
        }

        public void SetAnimationNoResetNotLooping(string anim)
        {
            self.AcceptInput("SetAnimationNoResetNotLooping", value: anim);
        }

        public void SetIdleAnimationNotLooping(string anim)
        {
            self.AcceptInput("SetIdleAnimationNotLooping", value: anim);
        }

        public void SetHealth(int value, bool setMax = true)
        {
            self.Health = value;
            if (setMax)
            {
                self.MaxHealth = value;
            }
        }

        public string GetModel()
        {
            return self.GetBodyComponent().GetSceneNode()?.AsSkeletonInstance?.GetModelState().ModelName ??
                   string.Empty;
        }

        public void SetParent(IBaseEntity? other)
        {
            self.AcceptInput("SetParent", other, self, "!activator");
        }

        public void ClearParent()
        {
            self.AcceptInput("ClearParent");
        }

        public void DelayRemove(float delay)
        {
            if (!self.IsValidEntity)
            {
                return;
            }

            if (self.GetAbsPtr() == nint.Zero)
            {
                return;
            }

            InterfaceBridge.Instance.ModSharp.DelayCall(delay, () =>
            {
                if (!self.IsValidEntity)
                {
                    return;
                }

                if (self.GetAbsPtr() == nint.Zero)
                {
                    return;
                }

                self.AcceptInput("Kill");
            });
        }
    }
}