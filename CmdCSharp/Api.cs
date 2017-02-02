using System;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace Lx.CmdCSharp
{
	public static class Win32Api
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AttachConsole(uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool SetConsoleCtrlHandler(EventHandler handler, bool state);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool FreeConsole();

		// Enumerated type for the control messages sent to the handler routine
		enum CtrlTypes : uint
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

		public static void SendCtrlC(Process proc)
		{
			FreeConsole();

			//This does not require the console window to be visible.
			if (AttachConsole((uint)proc.Id))
			{
				//Disable Ctrl-C handling for our program
				SetConsoleCtrlHandler(null, true);
				GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

				// Must wait here. If we don't and re-enable Ctrl-C
				// handling below too fast, we might terminate ourselves.
				//proc.WaitForExit(500);

				Thread.Sleep(100);

				//Re-enable Ctrl-C handling or any subsequently started
				//programs will inherit the disabled state.
				SetConsoleCtrlHandler(null, false);
			}
		}
	}
}
