using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Lx.CmdCSharp
{
	/// <summary>
	/// Interaction logic for MyControl.xaml
	/// </summary>
	public partial class MyControl : UserControl, IDisposable
	{
		public MyControl()
		{
			InitializeComponent();
		}

		~ MyControl()
		{
			Dispose(false);
		}

		private bool MDisposed = false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "ErrorTask"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "OutputTask")]
		protected virtual void Dispose(bool disposing)
		{
			if (!MDisposed)
			{
				if (disposing)
				{
					CancelToken.Cancel();
					Proc.Kill();
					OutputTask.Wait();
					ErrorTask.Wait();
					CancelToken.Dispose();
				}
				// free native resources

				MDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private Process Proc;
		private System.Threading.Tasks.Task OutputTask = null, ErrorTask = null;
		private int RstLen = 0;
		public CancellationTokenSource CancelToken = null;

		private void Init()
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

			if (Proc != null)
			{
				Proc.EnableRaisingEvents = true;

				OutputTask = new System.Threading.Tasks.Task(() => ReadRoutine(Proc.StandardOutput, CancelToken));
				OutputTask.Start();
				ErrorTask = new System.Threading.Tasks.Task(() => ReadRoutine(Proc.StandardError, CancelToken));
				ErrorTask.Start();

				Proc.Exited += (sender, e) => Restart();
			}
		}

		private void Restart()
		{
			CancelToken.Cancel();
			OutputTask.Wait();
			ErrorTask.Wait();
			CancelToken.Dispose();
			Init();
		}

		private void AddData(string outputs)
		{
			Action act = () =>
			{
				Rst.AppendText(outputs);
				RstLen = Rst.Text.Length;
				Rst.Select(RstLen, 0);
				Rst.ScrollToEnd();
			};

			this.Dispatcher.BeginInvoke(act);
		}

		private object Locker = new object();
		private bool CmdRepl = false;

		private void ReadRoutine(StreamReader output, CancellationTokenSource cancelToken)
		{
			char[] data = new char[4096];

			while (!cancelToken.Token.IsCancellationRequested)
			{
				try
				{
					if (output.Peek() == -1)
					{
						output.DiscardBufferedData();
						Thread.Sleep(50);
						continue;
					}

					int len = output.Read(data, 0, 4096);
					StringBuilder str = new StringBuilder();
					str.Append(data, 0, len);

					string outputs = str.ToString();

					lock (Locker)
					{
						if (CmdRepl)
						{
							CmdRepl = false;
							outputs = outputs.Substring(outputs.IndexOf('\n'));
						}
					}

					AddData(outputs);
				}
				catch (IOException)
				{
					if (!cancelToken.Token.IsCancellationRequested)
					{
						Proc.Kill();
					}
				}
			}
		}

		private bool FirstRun = true;

		private void OnLoad(object sender, EventArgs e)
		{
			if (FirstRun)
			{
				FirstRun = false;
				Rst.BorderThickness = new Thickness(0, 0, 0, 0);

				EnvDTE.DTEEvents eventsObj = CmdCSharpPackage.Dte.Events.DTEEvents;
				eventsObj.OnBeginShutdown += ShutDown;

				new System.Threading.Tasks.Task(Init).Start();
			}
		}

		private List<string> CmdList = new List<string>();
		private int CmdPos = -1;

		private void OnText(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Back && Rst.CaretIndex <= RstLen)
			{
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Return && Rst.CaretIndex <= RstLen - 1)
			{
				e.Handled = true;
				return;
			}

			if (Rst.CaretIndex < RstLen)
			{
				Rst.CaretIndex = Rst.Text.Length; //RstLen;
				return;
			}

			if (e.Key == Key.Up)
			{
				if (CmdPos >= 0)
				{
					Rst.Text = Rst.Text.Substring(0, RstLen) + CmdList[CmdPos];
					CmdPos -= 1;
					Rst.Select(Rst.Text.Length, 0);
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Down)
			{
				if (CmdPos < CmdList.Count - 2)
				{
					CmdPos += 1;
					Rst.Text = Rst.Text.Substring(0, RstLen) + CmdList[CmdPos];
					Rst.Select(Rst.Text.Length, 0);
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Tab)
			{
				e.Handled = true;
			}
			else if (e.Key == Key.Return)
			{
				string cmd = Rst.Text.Substring(RstLen, Rst.Text.Length - RstLen);
				RunCmd(cmd);

				e.Handled = true;
			}
		}

		private void RunCmd(string Cmd)
		{
			if (Cmd == "cls")
			{
				Action act = () =>
				{
					Rst.Text = "";
					RstLen = 0;

					Proc.StandardInput.WriteLine("");
				};

				this.Dispatcher.BeginInvoke(act);
			}
			else
			{
				lock (Locker)
				{
					RstLen = Rst.Text.Length; //protect input texts
					CmdRepl = true;
					Proc.StandardInput.WriteLine(Cmd);
				}
			}

			CmdList.Add(Cmd);
			CmdPos = CmdList.Count - 1;
		}

		private void ShutDown()
		{
			Dispose(true);
		}
	}
}
