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
		private CancellationTokenSource cancelSource;
		private bool running = false;

		public string InitDir { get; set; }
		public string Shell { get; set; } = "Cmd.exe";

		public void Register(ICmdReceiver newReceiver)
		{
			receivers.Add(newReceiver);
		}

		public bool Init()
		{
			cancelSource = new CancellationTokenSource();

			if ((cmdProc = CreateProc()) == null)
			{
				return false;
			}

			outputTask = new Task(() => ReadRoutine(cmdProc.StandardOutput, cancelSource.Token), cancelSource.Token);
			outputTask.Start();
			errorTask = new Task(() => ReadRoutine(cmdProc.StandardError, cancelSource.Token), cancelSource.Token);
			errorTask.Start();

			cmdProc.EnableRaisingEvents = true;

			cmdProc.Exited += (sender, e) =>
			{
				Close();
				Init();
			};

			running = true;

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

		private void ReadRoutine(StreamReader output, CancellationToken cancelToken)
		{
			var data = new char[4096];

			while (!cancelToken.IsCancellationRequested)
			{
				Thread.Sleep(50);

				try
				{
					int len = output.Read(data, 0, 4096);

					string str = new string(data, 0, len);

					Notify(str);
				}
				catch (Exception)
				{
					break; //Process terminated
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
			if (!running)
			{
				return;
			}

			cancelSource?.Cancel();

			if (cmdProc != null && !cmdProc.HasExited)
			{
				cmdProc.EnableRaisingEvents = false;
				cmdProc.Kill();
			}

			//outputTask?.Wait(100);
			//errorTask?.Wait(100);

			cancelSource?.Dispose();

			running = false;
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