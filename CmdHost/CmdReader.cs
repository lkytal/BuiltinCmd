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

		public void Init(string projectPath = null)
		{
			CancelToken = new CancellationTokenSource();

			if ((CmdProc = CreateProc(projectPath)) == null) return;

			OutputTask = new Task(() => ReadRoutine(CmdProc.StandardOutput, CancelToken));
			OutputTask.Start();
			ErrorTask = new Task(() => ReadRoutine(CmdProc.StandardError, CancelToken));
			ErrorTask.Start();

			CmdProc.EnableRaisingEvents = true;

			CmdProc.Exited += (sender, e) =>
			{
				CancelToken.Cancel();
				OutputTask.Wait();
				ErrorTask.Wait();
				CancelToken.Dispose();
				Init();
			};
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

			while (!cancelToken.Token.IsCancellationRequested)
			{
				try
				{
					Thread.Sleep(50);

					int len = output.Read(data, 0, 4096);

					StringBuilder str = new StringBuilder();
					str.Append(data, 0, len);

					Receiver.AddData(str.ToString());
				}
				catch (IOException)
				{
					return; //Process terminated
				}
			}
		}

		public void Close()
		{
			CmdProc.EnableRaisingEvents = false;
			CancelToken.Cancel();
			CmdProc.Kill();
			//OutputTask.Wait();
			//ErrorTask.Wait();
			CancelToken.Dispose();
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
	}
}