using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdHost
{
	public interface ICmdReceiver
	{
		void AddData(string output);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class CmdReader
	{
		private readonly List<ICmdReceiver> receivers = new List<ICmdReceiver>();
		private Process cmdProc;
		private Task outputTask, errorTask;
		private CancellationTokenSource cancelToken;

		public string InitDir { get; set; }
		public string Shell { get; set; } = "Cmd.exe";

		public void Register(ICmdReceiver newReceiver)
		{
			receivers.Add(newReceiver);
		}

		public bool Init()
		{
			cancelToken = new CancellationTokenSource();

			if ((cmdProc = CreateProc()) == null)
			{
				return false;
			}

			outputTask = new Task(() => ReadRoutine(cmdProc.StandardOutput, cancelToken));
			outputTask.Start();
			errorTask = new Task(() => ReadRoutine(cmdProc.StandardError, cancelToken));
			errorTask.Start();

			cmdProc.EnableRaisingEvents = true;

			cmdProc.Exited += (sender, e) =>
			{
				Close();
				Init();
			};

			return true;
		}

		private Process CreateProc()
		{
			ProcessStartInfo proArgs = new ProcessStartInfo(Shell)
			{
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};

			if (!string.IsNullOrEmpty(InitDir))
			{
				proArgs.WorkingDirectory = InitDir;
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

					Notify(str.ToString());
				}
				catch (Exception)
				{
					return; //Process terminated
				}
			}
		}

		public void Notify(string data)
		{
			foreach (var receiver in receivers)
			{
				receiver.AddData(data);
			}
		}

		public void Close()
		{
			if (cmdProc != null && !cmdProc.HasExited)
			{
				cmdProc.EnableRaisingEvents = false;
				cmdProc.Kill();
			}

			if (cancelToken != null && !cancelToken.IsCancellationRequested)
			{
				cancelToken.Cancel();
				outputTask?.Wait(100);
				errorTask?.Wait(100);

				cancelToken.Dispose();
			}
		}

		public void Input(string text)
		{
			cmdProc.StandardInput.WriteLine(text);
		}

		public void Restart()
		{
			cmdProc.Kill();
		}

		public void SendCtrlC()
		{
			NativeMethods.SendCtrlC(cmdProc);
		}

		~CmdReader()
		{
			Close();
		}
	}
}