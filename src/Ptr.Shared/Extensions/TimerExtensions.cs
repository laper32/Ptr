using System.Runtime.CompilerServices;
using Sharp.Shared;
using Sharp.Shared.Enums;

namespace Ptr.Shared.Extensions;

public static class TimerExtension
{
    extension(IModSharp sharp)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvokeNextFrame(Action action)
        {
            sharp.PushTimer(action, 0.0001);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvokeNextFrameThisRound(Action action)
        {
            sharp.PushTimer(action, 0.0001, GameTimerFlags.StopOnRoundEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid DelayCall(double interval, Action call)
        {
            return sharp.PushTimer(call, interval);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid DelayCall(double interval, Func<TimerAction> call)
        {
            return sharp.PushTimer(call, interval);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid DelayCallThisRound(double interval, Action call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.StopOnRoundEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid DelayCallThisRound(double interval, Func<TimerAction> call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.StopOnRoundEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid DelayCallThisMap(double interval, Action call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.StopOnMapEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid DelayCallThisMap(double interval, Func<TimerAction> call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.StopOnMapEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid RepeatCall(double interval, Action call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.Repeatable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid RepeatCall(double interval, Func<TimerAction> call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.Repeatable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid RepeatCallThisRound(double interval, Action call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.Repeatable | GameTimerFlags.StopOnRoundEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid RepeatCallThisRound(double interval, Func<TimerAction> call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.Repeatable | GameTimerFlags.StopOnRoundEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid RepeatCallThisMap(double interval, Action call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.Repeatable | GameTimerFlags.StopOnMapEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid RepeatCallThisMap(double interval, Func<TimerAction> call)
        {
            return sharp.PushTimer(call, interval, GameTimerFlags.Repeatable | GameTimerFlags.StopOnMapEnd);
        }
    }
}