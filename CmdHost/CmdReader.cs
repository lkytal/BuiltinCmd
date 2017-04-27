using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdHost
{
	public class CmdReader
	{
		private readonly Controller controller;
		private Process Proc;
		private Task OutputTask;
		private Task ErrorTask;
		private CancellationTokenSource CancelToken;

		public CmdReader(Controller _controller)
		{
			controller = _controller;
		}

		public void Init()
		{
			CancelToken = new CancellationTokenSource();

			ProcessStartInfo proArgs = new ProcessStartInfo("cmd.exe")
			{
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};

			Proc = Process.Start(proArgs);

			if (Proc == null) return;

			Proc.EnableRaisingEvents = true;

			OutputTask = new Task(() => ReadRoutine(Proc.StandardOutput, CancelToken));
			OutputTask.Start();
			ErrorTask = new Task(() => ReadRoutine(Proc.StandardError, CancelToken));
			ErrorTask.Start();

			Proc.Exited += (sender, e) =>
			{
				CancelToken.Cancel();
				OutputTask.Wait();
				ErrorTask.Wait();
				CancelToken.Dispose();
				Init();
			};
		}

		private void ReadRoutine(StreamReader output, CancellationTokenSource cancelToken)
		{
			char[] data = new char[4096];

			while (!cancelToken.Token.IsCancellationRequested)
			{
				try
				{
					Thread.Sleep(50);

					int len = output.Read(data, 0, 4096);

					StringBuilder str = new StringBuilder();
					str.Append(data, 0, len);

					controller.AddData(str.ToString());
				}
				catch (IOException)
				{
					return; //Proc terminated
				}
			}
		}

		public void Close()
		{
			Proc.EnableRaisingEvents = false;
			CancelToken.Cancel();
			Proc.Kill();
			OutputTask.Wait();
			ErrorTask.Wait();
			CancelToken.Dispose();
		}

		public void Input(string text)
		{
			Proc.StandardInput.WriteLine(text);
		}

		public void Restart()
		{
			Proc.Kill();
		}

		public void SendCtrlC()
		{
			SendCtrlC(Proc);
		}

		#region win32
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AttachConsole(uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool SetConsoleCtrlHandler(uint dwProcessId, bool state);

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

		private void SendCtrlC(Process proc)
		{
			FreeConsole();

			//This does not require the console window to be visible.
			if (AttachConsole((uint)proc.Id))
			{
				//Disable Ctrl-C handling for our program
				SetConsoleCtrlHandler(0, true);
				GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

				// Must wait here. If we don't and re-enable Ctrl-C
				// handling below too fast, we might terminate ourselves.
				//proc.WaitForExit(500);

				Thread.Sleep(100);

				//Re-enable Ctrl-C handling or any subsequently started
				//programs will inherit the disabled state.
				SetConsoleCtrlHandler(0, false);
			}
		}
		#endregion
	}
}