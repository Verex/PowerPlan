using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PowerPlan
{
    class Power
    {
        public static Dictionary<Guid, Plan> Plans { get; private set; } = new Dictionary<Guid, Plan>();

        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        private static extern uint PowerEnumerate(IntPtr RootPowerKey, IntPtr SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, UInt32 AcessFlags, UInt32 Index, ref Guid Buffer, ref UInt32 BufferSize);

        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        private static extern uint PowerReadFriendlyName(IntPtr RootPowerKey, ref Guid SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, IntPtr PowerSettingGuid, IntPtr Buffer, ref UInt32 BufferSize);

        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        private static extern uint PowerSetActiveScheme(IntPtr RootPowerKey, ref Guid SchemeGuid);

        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        private static extern uint PowerGetActiveScheme(IntPtr RootPowerKey, ref IntPtr ActivePolicyGuid);

        public enum AccessFlags : uint
        {
            ACCESS_SCHEME = 16,
            ACCESS_SUBGROUP = 17,
            ACCESS_INDIVIDUAL_SETTING = 18
        }

        private static IEnumerable<Guid> GetAll()
        {
            var schemeGuid = Guid.Empty;

            uint sizeSchemeGuid = (uint)Marshal.SizeOf(typeof(Guid));
            uint schemeIndex = 0;

            while (PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (uint)AccessFlags.ACCESS_SCHEME, schemeIndex, ref schemeGuid, ref sizeSchemeGuid) == 0)
            {
                yield return schemeGuid;
                schemeIndex++;
            }
        }

        private static string ReadFriendlyName(Guid schemeGuid)
        {
            uint sizeName = 1024;
            IntPtr pSizeName = Marshal.AllocHGlobal((int)sizeName);

            string friendlyName;

            try
            {
                PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, pSizeName, ref sizeName);
                friendlyName = Marshal.PtrToStringUni(pSizeName);
            }
            finally
            {
                Marshal.FreeHGlobal(pSizeName);
            }

            return friendlyName;
        }

        public static void SetActiveScheme(Guid schemeGuid)
        {
            if (Plans.ContainsKey(schemeGuid))
                PowerSetActiveScheme(IntPtr.Zero, ref schemeGuid);
        }
        
        public static Guid GetActiveScheme()
        {
            IntPtr activeGuid = IntPtr.Zero;

            if (PowerGetActiveScheme(IntPtr.Zero, ref activeGuid) != 0)
                throw new Win32Exception();

            return (Guid)Marshal.PtrToStructure(activeGuid, typeof(Guid));
        }

        public static void GetPlans()
        {
            Guid activeScheme = GetActiveScheme();
            var guidPlans = GetAll();

            foreach (Guid guidPlan in guidPlans)
            {
                Plans[guidPlan] = new Plan() { Name = ReadFriendlyName(guidPlan), Active = guidPlan == activeScheme };
            }
        }
    }
}
