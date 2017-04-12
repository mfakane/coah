using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Linearstar.Coah
{
	static class NativeMethods
	{
		public const int GWL_STYLE = -16;
		public const int GWL_EXSTYLE = -20;
		public const int WS_EX_APPWINDOW = 0x40000;
		public const int WS_EX_NOACTIVATE = 0x08000000;
		public const int SWP_NOSIZE = 0x0001;
		public const int SWP_NOMOVE = 0x0002;
		public const int SWP_NOZORDER = 0x0004;
		public const int SWP_NOACTIVATE = 0x0010;
		public const int SWP_FRAMECHANGED = 0x0020;
		public const int SM_CXPADDEDBORDER = 92;

		public static IntPtr GetWindowLong(this IntPtr hWnd, int nIndex) => Environment.Is64BitProcess ? GetWindowLong64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
		public static IntPtr SetWindowLong(this IntPtr hWnd, int nIndex, IntPtr dwNewLong) => Environment.Is64BitProcess ? SetWindowLong64(hWnd, nIndex, dwNewLong) : SetWindowLong32(hWnd, nIndex, dwNewLong);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "GetWindowLong")]
		static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "GetWindowLongPtr")]
		static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "SetWindowLong")]
		static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "SetWindowLongPtr")]
		static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
		[SuppressUnmanagedCodeSecurity, DllImport("user32")]
		public static extern bool SetWindowPos(this IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags);
		[SuppressUnmanagedCodeSecurity, DllImport("user32")]
		public static extern int GetSystemMetrics(int nIndex);
		[SuppressUnmanagedCodeSecurity, DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
		[SuppressUnmanagedCodeSecurity, DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PostMessage(this IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
	}
}
