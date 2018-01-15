using System.Runtime.CompilerServices;

namespace elbgb_core
{
    class SystemClock
    {
        public const uint ClockFrequency = 4194304;

        public ulong Timestamp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddMachineCycle()
        {
            Timestamp += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddMachineCycles(uint machineCycleCount)
        {
            Timestamp += machineCycleCount * 4;
        }
    }
}
