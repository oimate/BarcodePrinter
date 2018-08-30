using System;
using System.Runtime.InteropServices;

namespace BarcodePrinter
{
    internal class WinUser32
    {
        public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessage", CharSet = CharSet.Auto)]
        public static extern ushort RegisterWindowMessage(string lpString);
    }

    internal class CommLib
    {
        public const int CL_MSG_Long = 129;
        public const int CL_MSG_Short = 17;
        public const int CL_ATE_Group = 129;
        public const int CL_ATE_Source = 33;

        [DllImport("commlib.dll", EntryPoint = "SymbolX_Advise", CharSet = CharSet.Ansi)]
        public static extern ushort SymbolX_Advise(IntPtr handle, string plc, string tag);

        [DllImport("commlib.dll", EntryPoint = "SymbolX_Shutdown", CharSet = CharSet.Ansi)]
        public static extern ushort SymbolX_Shutdown(IntPtr handle);

        [DllImport("commlib.dll", EntryPoint = "SymbolX_PassPointers", CharSet = CharSet.Ansi)]
        public static extern ushort SymbolX_PassPointers(IntPtr handle);

        [DllImport("commlib.dll", EntryPoint = "SymbolX_Poke", CharSet = CharSet.Ansi)]
        public static extern ushort SymbolX_Poke(IntPtr handle, string plc, string tag, string value, ushort storeData);

        [DllImport("commlib.dll", EntryPoint = "CommLib_Show", CharSet = CharSet.Ansi)]
        public static extern bool CommLib_Show();

        [DllImport("commlib.dll", EntryPoint = "CommLib_CommDiag", CharSet = CharSet.Ansi)]
        public static extern bool CommLib_CommDiag(IntPtr handle);

        [DllImport("commlib.dll", EntryPoint = "CommLib_AddIOManager", CharSet = CharSet.Ansi)]
        public static extern bool CommLib_AddIOManager(string host);

        [DllImport("commlib.dll", EntryPoint = "CommLib_RemoveIOManager", CharSet = CharSet.Ansi)]
        public static extern bool CommLib_RemoveIOManager(string host);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public sealed class DataRec
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CL_MSG_Long)]
            public string Tagname;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CL_MSG_Long)]
            public string Source;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CL_MSG_Long)]
            public string Value;

            public byte Quality;
            public int UserRights;
        }
    }
}