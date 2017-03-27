using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Text.RegularExpressions;

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
		private System.Threading.Tasks.Task OutputTask, ErrorTask;
		private readonly object Locker = new object();
		private int RstLen = 0;
		public CancellationTokenSource CancelToken;

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

			if (Proc == null)
			{
				Rst.Text = "Create cmd process error.";
				return;
			}

			Proc.EnableRaisingEvents = true;

			OutputTask = new System.Threading.Tasks.Task(() => ReadRoutine(Proc.StandardOutput, CancelToken));
			OutputTask.Start();
			ErrorTask = new System.Threading.Tasks.Task(() => ReadRoutine(Proc.StandardError, CancelToken));
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

		private void ExtractDir(ref string outputs)
		{
			string lastLine = outputs.Substring(outputs.LastIndexOf('\n') + 1);

			if (Regex.IsMatch(lastLine, @"^\w:\\\S*>$"))
			{
				dir = lastLine.Substring(0, lastLine.Length - 1);
			}
		}

		private void AddData(string outputs)
		{
			Action act = () =>
			{
				ExtractDir(ref outputs);

				Rst.AppendText(outputs);
				RstLen = Rst.Text.Length;
				Rst.Select(RstLen, 0);
				//Rst.ScrollToEnd();
			};

			Dispatcher.BeginInvoke(act);
		}

		private void ReadRoutine(StreamReader outputSteam, CancellationTokenSource cancelToken)
		{
			const int buffLength = 4096;

			char[] data = new char[buffLength];

			while (!cancelToken.Token.IsCancellationRequested)
			{
				try
				{
					Thread.Sleep(50);
					int len = outputSteam.Read(data, 0, buffLength);

					StringBuilder str = new StringBuilder();
					str.Append(data, 0, len);

					AddData(str.ToString());
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
				Rst.UndoLimit = 100;

				EnvDTE.DTEEvents eventsObj = CmdCSharpPackage.Dte.Events.DTEEvents;
				eventsObj.OnBeginShutdown += ShutDown;

				Init();
				//new System.Threading.Tasks.Task(Init).Start();
			}
		}

		private readonly List<string> CmdList = new List<string>();
		private int CmdPos = -1;
		private int tabIndex = 0;
		private int tabEnd = 0;
		private string dir = "";

		private void ResetTabComplete(Key key)
		{
			tabIndex = 0;

			switch (key)
			{
				case Key.Delete:
					tabEnd = Rst.Text.Length - 1;
					break;
				case Key.Back:
					tabEnd = Rst.Text.Length - 1;
					break;
				default:
					tabEnd = Rst.Text.Length + 1;
					break;
			}
		}

		private void OnText(object sender, KeyEventArgs e)
		{
			if (Rst.CaretIndex < RstLen)
			{
				if (e.Key != Key.Left && e.Key != Key.Right)
				{
					e.Handled = true;
				}

				return;
			}

			if (e.Key == Key.Back && Rst.CaretIndex <= RstLen)
			{
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Tab)
			{
				e.Handled = true;

				string cmd = Rst.Text.Substring(RstLen, tabEnd - RstLen);

				int pos = cmd.LastIndexOf('"');
				if (pos == -1)
				{
					pos = cmd.LastIndexOf(' ');
				}

				string tabHit = cmd.Substring(pos + 1);

				try
				{
					string AdditionalPath = "\\";

					if (tabHit.LastIndexOf('\\') != -1)
					{
						AdditionalPath += tabHit.Substring(0, tabHit.LastIndexOf('\\'));
						tabHit = tabHit.Substring(tabHit.LastIndexOf('\\') + 1);
					}

					var files = Directory.GetFileSystemEntries(dir + AdditionalPath, tabHit + "*");

					if (files.Length == 0)
					{
						return; //no match
					}

					if (tabIndex >= files.Length)
					{
						tabIndex = 0;
					}

					Rst.Text = Rst.Text.Remove(tabEnd - tabHit.Length);

					string tabFile = files[tabIndex++];
					string tabName = tabFile.Substring(tabFile.LastIndexOf('\\') + 1);
					Rst.AppendText(tabName);
					Rst.Select(Rst.Text.Length, 0);
				}
				catch (ArgumentException ex)
				{
					Debug.WriteLine(ex);
					tabIndex = 0;
				}

				return;
			}

			ResetTabComplete(e.Key);

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
			else if (e.Key == Key.C && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)) //Keyboard.IsKeyDown(Key.LeftCtrl)
			{
				Win32Api.SendCtrlC(Proc);

				e.Handled = true;
			}
			else if (e.Key == Key.Return)
			{
				string cmd = Rst.Text.Substring(RstLen, Rst.Text.Length - RstLen);

				RunCmd(cmd);

				e.Handled = true;
			}
		}

		private void RunCmd(string cmd)
		{
			if (cmd == "cls")
			{
				Action act = () =>
				{
					Rst.Text = "";
					RstLen = 0;

					Proc.StandardInput.WriteLine("");
				};

				Dispatcher.BeginInvoke(act);
			}
			else
			{
				lock (Locker)
				{
					Rst.Text = Rst.Text.Substring(0, RstLen);
					Proc.StandardInput.WriteLine(cmd);
				}
			}

			CmdList.Add(cmd);
			CmdPos = CmdList.Count - 1;
		}

		private void OnCopy(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Rst.SelectedText);
		}

		private void OnClear(object sender, EventArgs e)
		{
			Rst.Text = "";
			RstLen = 0;
			Proc.StandardInput.WriteLine("");
		}
		private void OnRestart(object sender, EventArgs e)
		{
			Rst.Text = "";
			RstLen = 0;
			Proc.Kill();
			//Restart();
		}

		private void OnSave(object sender, EventArgs e)
		{
			SaveFileDialog saveDlg = new SaveFileDialog
			{
				Filter = "txt文件|*.txt|所有文件|*.*",
				FilterIndex = 2,
				RestoreDirectory = true,
				DefaultExt = ".txt",
				AddExtension = true,
				Title = "Save Cmd Results"
			};

			if (saveDlg.ShowDialog() == true)
			{
				FileStream saveStream = new FileStream(saveDlg.FileName, FileMode.Create);
				byte[] data = new UTF8Encoding().GetBytes(Rst.Text);
				saveStream.Write(data, 0, data.Length);
				saveStream.Flush();
				saveStream.Close();
			}
		}

		private void ShutDown()
		{
			Proc.EnableRaisingEvents = false;
			Dispose(true);
		}
	}
}
