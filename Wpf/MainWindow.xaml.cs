using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
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
				OutputTask.Wait();
				ErrorTask.Wait();
			};
		}
		
		~MainWindow()
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
					return; //Proc terminated
				}
			}
		}
		
		private void OnClear(object sender, EventArgs e)
		{
			Rst.Text = "";
			Proc.StandardInput.WriteLine("");
		}
		private void OnRestart(object sender, EventArgs e)
		{
			Proc.Kill();
			Rst.Text = "";
			Restart();
		}

		private void OnSave(object sender, EventArgs e)
		{
			SaveFileDialog SaveDlg = new SaveFileDialog();
			SaveDlg.Filter = "txt文件|*.txt|所有文件|*.*";
			SaveDlg.FilterIndex = 2;
			SaveDlg.RestoreDirectory = true;
			SaveDlg.DefaultExt = ".txt";
			SaveDlg.AddExtension = true;
			SaveDlg.Title = "Save Cmd Results";

			if (SaveDlg.ShowDialog() == true)
			{
				FileStream SaveStream = new FileStream(SaveDlg.FileName, FileMode.Create);
				byte[] Data = new UTF8Encoding().GetBytes(this.Rst.Text);
				SaveStream.Write(Data, 0, Data.Length);
				SaveStream.Flush();
				SaveStream.Close();
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
			else if (e.Key == Key.Tab)
			{
				e.Handled = true;
			}
			else if (e.Key == Key.Return)
			{
				string Cmd = Rst.Text.Substring(RstLen, Rst.Text.Length - RstLen);

				CmdRepl = true;
				Proc.StandardInput.WriteLine(Cmd);

				PreCmd.Add(Cmd);
				CmdPos = PreCmd.Count - 1;
				e.Handled = true;
			}
		}
	}
}
