#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.Native
{
	public static class User32
	{
		private const string USER32_DLL = "user32.dll";

		/// <summary>
		///     Retrieves a handle to the top-level window whose class name and window name match the specified strings. This
		///     function does not search child windows. This function does not perform a case-sensitive search. To search child
		///     windows, beginning with a specified child window, use the
		///     <see cref="!:https://msdn.microsoft.com/en-us/library/windows/desktop/ms633500%28v=vs.85%29.aspx">FindWindowEx</see>
		///     function.
		///     <para>
		///         Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633499%28v=vs.85%29.aspx for FindWindow
		///         information or https://msdn.microsoft.com/en-us/library/windows/desktop/ms633500%28v=vs.85%29.aspx for
		///         FindWindowEx
		///     </para>
		/// </summary>
		/// <param name="lpClassName">
		///     C++ ( lpClassName [in, optional]. Type: LPCTSTR )<br />The class name or a class atom created by a previous call to
		///     the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the
		///     high-order word must be zero.
		///     <para>
		///         If lpClassName points to a string, it specifies the window class name. The class name can be any name
		///         registered with RegisterClass or RegisterClassEx, or any of the predefined control-class names.
		///     </para>
		///     <para>If lpClassName is NULL, it finds any window whose title matches the lpWindowName parameter.</para>
		/// </param>
		/// <param name="lpWindowName">
		///     C++ ( lpWindowName [in, optional]. Type: LPCTSTR )<br />The window name (the window's
		///     title). If this parameter is NULL, all window names match.
		/// </param>
		/// <returns>
		///     C++ ( Type: HWND )<br />If the function succeeds, the return value is a handle to the window that has the
		///     specified class name and window name. If the function fails, the return value is NULL.
		///     <para>To get extended error information, call GetLastError.</para>
		/// </returns>
		/// <remarks>
		///     If the lpWindowName parameter is not NULL, FindWindow calls the <see cref="M:GetWindowText" /> function to
		///     retrieve the window name for comparison. For a description of a potential problem that can arise, see the Remarks
		///     for <see cref="M:GetWindowText" />.
		/// </remarks>
		[DllImport(USER32_DLL, SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		/// <summary>
		///     Changes the text of the specified window's title bar (if it has one). If the specified window is a control, the
		///     text of the control is changed. However, SetWindowText cannot change the text of a control in another application.
		///     <para>
		///         Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633546%28v=vs.85%29.aspx for more
		///         information
		///     </para>
		/// </summary>
		/// <param name="hwnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window or control whose text is to be changed.</param>
		/// <param name="lpString">C++ ( lpString [in, optional]. Type: LPCTSTR )<br />The new title or control text.</param>
		/// <returns>
		///     If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.<br />
		///     To get extended error information, call GetLastError.
		/// </returns>
		/// <remarks>
		///     If the target window is owned by the current process, <see cref="SetWindowText" /> causes a WM_SETTEXT message to
		///     be sent to the specified window or control. If the control is a list box control created with the WS_CAPTION style,
		///     however, <see cref="SetWindowText" /> sets the text for the control, not for the list box entries.<br />To set the
		///     text of a control in another process, send the WM_SETTEXT message directly instead of calling
		///     <see cref="SetWindowText" />. The <see cref="SetWindowText" /> function does not expand tab characters (ASCII code
		///     0x09). Tab characters are displayed as vertical bar(|) characters.<br />For an example go to
		///     <see cref="!:https://msdn.microsoft.com/en-us/library/windows/desktop/ms644928%28v=vs.85%29.aspx#sending">
		///         Sending a
		///         Message.
		///     </see>
		/// </remarks>
		[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowText(IntPtr hwnd, string lpString);

		[DllImport(USER32_DLL, SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		// When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
		[DllImport(USER32_DLL)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

		public static IntPtr FindWindowByCaption(string windowName)
		{
			return FindWindow(null, windowName);
		}
	}
}