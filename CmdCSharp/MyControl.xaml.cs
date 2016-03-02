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

		protected virtual void Dispose(bool disposing)
		{
			if (!MDisposed)
			{
				if (disposing)
				{
					Proc.Kill();
					CancelToken.Dispose();
					OutputTask.Wait();
					ErrorTask.Wait();
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
			char[] Data = new char[4096];

			while (!cancelToken.Token.IsCancellationRequested)
			{
				try
				{
					if (output.Peek() == -1)
					{
						output.DiscardBufferedData();
						Thread.SpinWait(50);
						continue;
					}

					int Len = output.Read(Data, 0, 4096);
					StringBuilder Str = new StringBuilder();
					Str.Append(Data, 0, Len);

					Action Act = () =>
					{
						Rst.AppendText(Str.ToString());
						RstLen = Rst.Text.Length;
						Rst.ScrollToEnd();
						Rst.Select(RstLen, 0);
					};

					this.Dispatcher.BeginInvoke(Act);
				}
				catch (Exception e)
				{
					//MessageBox.Show(e.Message);
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
		
		private void RunCmd()
		{
			Proc.StandardInput.WriteLine(CmdLine.Text);
			CmdList.Add(CmdLine.Text);
			CmdPos = CmdList.Count - 1;
			CmdLine.Text = "";
		}
		
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				RunCmd();
			}
			else if (e.Key == Key.Up)
			{
				if (CmdPos < 0) return;
				CmdLine.Text = CmdList[CmdPos];
				CmdPos -= 1;
				CmdLine.Select(CmdLine.Text.Length, 0);
			}
			else if (e.Key == Key.Down)
			{
				if (CmdPos >= CmdList.Count - 2) return;
				CmdPos += 1;
				CmdLine.Text = CmdList[CmdPos + 1];
				CmdLine.Select(CmdLine.Text.Length, 0);
			}
		}

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
				Rst.Text = Rst.Text.Substring(0, RstLen);

				Proc.StandardInput.WriteLine(Cmd);
				CmdList.Add(Cmd);
				CmdPos = CmdList.Count - 1;
				e.Handled = true;
			}
		}
	}
}
