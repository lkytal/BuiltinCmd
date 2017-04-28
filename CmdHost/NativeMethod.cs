using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace CmdHost
{
	public class NativeMethods
	{
		// ReSharper disable UnusedMember.Local
		private enum CtrlTypes : uint
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		private delegate bool ConsoleCtrlDelegate(CtrlTypes CtrlType);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AttachConsole(uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate Handler, bool Add);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		private static extern bool FreeConsole();

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