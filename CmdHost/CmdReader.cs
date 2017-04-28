using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdHost
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
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

		public void Init(string projectPath = null)
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

			if (!string.IsNullOrEmpty(projectPath))
			{
				proArgs.WorkingDirectory = projectPath;
			}

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
			//OutputTask.Wait();
			//ErrorTask.Wait();
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
			NativeMethods.SendCtrlC(Proc);
		}
	}
}