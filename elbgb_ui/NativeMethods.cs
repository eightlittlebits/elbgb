using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_ui
{
	static class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct Message
		{
			public IntPtr hWnd;
			public uint msg;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public Point p;
		}

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		public static extern System.IntPtr SelectObject(
			[In] System.IntPtr hdc,
			[In] System.IntPtr hgdiobj);

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject(
			[In] System.IntPtr hObject);

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("gdi32.dll", EntryPoint = "BitBlt")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BitBlt(
			[In] System.IntPtr hdc, int x, int y, int cx, int cy,
			[In] System.IntPtr hdcSrc, int x1, int y1, uint rop);

		public static class RasterOperation
		{
			public const uint SRCCOPY = 0x00CC0020;
		}

		[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
		public static extern uint TimeBeginPeriod(uint uMilliseconds);
	}
}
