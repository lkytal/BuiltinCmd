using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
//using Microsoft.VisualStudio.Shell;
using System.Threading;
using System.Threading.Tasks;

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
		private Task OutputTask = null, ErrorTask = null;
		private int RstLen = 0;
		CancellationTokenSource CancelToken = null;

		private void Init()
		{
			CancelToken = new CancellationTokenSource();

			ProcessStartInfo ProArgs = new ProcessStartInfo("cmd.exe");
			ProArgs.CreateNoWindow = true;
			ProArgs.RedirectStandardOutput = true;
			ProArgs.RedirectStandardInput = true;
			ProArgs.RedirectStandardError = true;
			ProArgs.UseShellExecute = false;

			Proc = Process.Start(ProArgs);
			Proc.EnableRaisingEvents = true;

			OutputTask = new Task(() => ReadRoutine(Proc.StandardOutput, CancelToken));
			OutputTask.Start();
			ErrorTask = new Task(() => ReadRoutine(Proc.StandardError, CancelToken));
			ErrorTask.Start();

			Proc.Exited += (sender, e) => Restart();
		}

		private void Restart()
		{
			CancelToken.Cancel();
			OutputTask.Wait();
			ErrorTask.Wait();
			CancelToken.Dispose();
			Init();
		}

		private void AddData(String outputs)
		{
			Action Act = () =>
			{
				Rst.AppendText(outputs);
				RstLen = Rst.Text.Length;
				Rst.Select(RstLen, 0);
				Rst.ScrollToEnd();
			};

			this.Dispatcher.BeginInvoke(Act);
		}

		private bool CmdRepl = false;

		private void ReadRoutine(StreamReader output, CancellationTokenSource cancelToken)
		{
			char[] Data = new char[4096];

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

					int Len = output.Read(Data, 0, 4096);
					StringBuilder Str = new StringBuilder();
					Str.Append(Data, 0, Len);

					String Outputs = Str.ToString();

					if (CmdRepl)
					{
						CmdRepl = false;
						Outputs = Outputs.Substring(Outputs.IndexOf('\n'));
					}

					AddData(Outputs);
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

				new Task(() => Init()).Start();
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
				Rst.CaretIndex = RstLen;
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
			else if (e.Key == Key.Return)
			{
				string Cmd = Rst.Text.Substring(RstLen, Rst.Text.Length - RstLen);

				CmdRepl = true;
				Proc.StandardInput.WriteLine(Cmd);

				CmdList.Add(Cmd);
				CmdPos = CmdList.Count - 1;
				e.Handled = true;
			}
		}
	}
}
