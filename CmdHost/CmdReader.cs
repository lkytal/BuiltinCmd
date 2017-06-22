using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdHost
{
	public interface CmdReceiver
	{
		void AddData(string output);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class CmdReader
	{
		private readonly CmdReceiver Receiver;
		private Process CmdProc;
		private Task OutputTask, ErrorTask;
		private CancellationTokenSource CancelToken;

		public CmdReader(CmdReceiver _receiver)
		{
			Receiver = _receiver;
		}

		public bool Init(string projectPath = null)
		{
			CancelToken = new CancellationTokenSource();

			if ((CmdProc = CreateProc(projectPath)) == null)
			{
				return false;
			}

			OutputTask = new Task(() => ReadRoutine(CmdProc.StandardOutput, CancelToken));
			OutputTask.Start();
			ErrorTask = new Task(() => ReadRoutine(CmdProc.StandardError, CancelToken));
			ErrorTask.Start();

			CmdProc.EnableRaisingEvents = true;

			CmdProc.Exited += (sender, e) =>
			{
				Close();
				Init();
			};

			return true;
		}

		private Process CreateProc(string projectPath)
		{
			ProcessStartInfo proArgs = new ProcessStartInfo("cmd.exe")
			{
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};

			if (!string.IsNullOrEmpty(projectPath))
			{
				proArgs.WorkingDirectory = projectPath;
			}

			return Process.Start(proArgs);
		}

		private void ReadRoutine(StreamReader output, CancellationTokenSource cancelToken)
		{
			char[] data = new char[4096];

			while (!cancelToken.IsCancellationRequested)
			{
				Thread.Sleep(50);

				try
				{
					int len = output.Read(data, 0, 4096);

					StringBuilder str = new StringBuilder();
					str.Append(data, 0, len);

					Receiver.AddData(str.ToString());
				}
				catch (Exception)
				{
					return; //Process terminated
				}
			}
		}

		public void Close()
		{
			if (CmdProc != null && !CmdProc.HasExited)
			{
				CmdProc.EnableRaisingEvents = false;
				CmdProc.Kill();
			}

			if (CancelToken != null && !CancelToken.IsCancellationRequested)
			{
				CancelToken.Cancel();
				OutputTask?.Wait(100);
				ErrorTask?.Wait(100);

				CancelToken.Dispose();
			}
		}

		public void Input(string text)
		{
			CmdProc.StandardInput.WriteLine(text);
		}

		public void Restart()
		{
			CmdProc.Kill();
		}

		public void SendCtrlC()
		{
			NativeMethods.SendCtrlC(CmdProc);
		}

		~CmdReader()
		{
			Close();
		}
	}
}