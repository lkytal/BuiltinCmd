using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Wpf
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Rst.BorderThickness = new Thickness(0, 0, 0, 0);
			this.Closing += (s, e) =>
			{
				CancelToken.Cancel();
				Proc.Kill();
				ReadThread.Wait();
				ErrThread.Wait();
			};
		}

		private Process Proc = null;
		private Task ReadThread = null, ErrThread = null;
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

			ReadThread = new Task(() => ReadRoutine(CancelToken));  
			ReadThread.Start();
			ErrThread = new Task(() => ErrorRoutine(CancelToken)); 
			ErrThread.Start();

			Proc.Exited += (sender, e) =>
			{
				CancelToken.Cancel();
				ReadThread.Wait();
				ErrThread.Wait();
				Init();
			};
			
		}

		private void ReadRoutine(CancellationTokenSource cancelToken)
		{
			StreamReader Output = Proc.StandardOutput;
			char[] Data = new char[4096];

			while (!cancelToken.Token.IsCancellationRequested)
			{
				try
				{
					if (Output.Peek() == -1)
					{
						Output.DiscardBufferedData();
						Thread.SpinWait(50);
						continue;
					}

					int Len = Output.Read(Data, 0, 4096);
					StringBuilder Str = new StringBuilder();
					Str.Append(Data, 0, Len);

					Action Act = () =>
					{
						Rst.AppendText(Str.ToString());
						RstLen = Rst.Text.Length;
						Rst.Select(RstLen, 0);
					};

					this.Dispatcher.Invoke(Act);
				}
				catch (Exception e)
				{
					//MessageBox.Show(e.Message);
				}
			}
		}

		private void ErrorRoutine(CancellationTokenSource cancelToken)
		{
			StreamReader Err = Proc.StandardError;
			char[] Data = new char[4096];

			while (!cancelToken.Token.IsCancellationRequested)
			{
				try
				{
					if (Err.Peek() == -1)
					{
						Err.DiscardBufferedData();
						Thread.SpinWait(50);
						continue;
					}

					int Len = Err.Read(Data, 0, 4096);
					StringBuilder Str = new StringBuilder();
					Str.Append(Data, 0, Len);

					Action Act = () =>
					{
						Rst.AppendText(Str.ToString());
						RstLen = Rst.Text.Length;
						Rst.Select(RstLen, 0);
					};

					this.Dispatcher.Invoke(Act);
				}
				catch (Exception e)
				{
					//MessageBox.Show(e.Message);
				}
			}
		}

		private void OnLoad(object sender, EventArgs e)
		{
			Init();
		}

		private List<string> PreCmd = new List<string>();
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
					Rst.Text = Rst.Text.Substring(0, RstLen) + PreCmd[CmdPos];
					CmdPos -= 1;
					Rst.Select(Rst.Text.Length, 0);
				}
				
				e.Handled = true;
			}
			else if (e.Key == Key.Down)
			{
				if (CmdPos < PreCmd.Count - 2)
				{
					CmdPos += 1;
					Rst.Text = Rst.Text.Substring(0, RstLen) + PreCmd[CmdPos];
					Rst.Select(Rst.Text.Length, 0);
				}
				
				e.Handled = true;
			}
			else if (e.Key == Key.Return)
			{
				string Cmd = Rst.Text.Substring(RstLen, Rst.Text.Length - RstLen);
				Rst.Text = Rst.Text.Substring(0, RstLen);

				Proc.StandardInput.WriteLine(Cmd);
				PreCmd.Add(Cmd);
				CmdPos = PreCmd.Count - 1;
				e.Handled = true;
			}
		}
	}
}
