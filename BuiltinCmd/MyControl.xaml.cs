using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;
using EnvDTE;
using EnvDTE80;
using Package = Microsoft.VisualStudio.Shell.Package;
using CmdHost;

namespace BuiltinCmd
{
	/// <summary>
	/// Interaction logic for MyControl.xaml
	/// </summary>
	public partial class MyControl : UserControl, UI
	{
		private readonly Controller controller;

		public MyControl()
		{
			InitializeComponent();

			Rst.BorderThickness = new Thickness(0, 0, 0, 0);
			Rst.UndoLimit = 100;
			Rst.Focus();

			controller = new Controller(this);
		}

		private DTE2 Dte;
		private Events2 events;
		private DTEEvents dteEvents;
		private SolutionEvents solutionEvents;

		private bool FirstRun = true;

		private void OnLoad(object sender, EventArgs e)
		{
			if (!FirstRun)
			{
				return;
			}

			FirstRun = false;

			Init();
		}

		private void Init()
		{
			Dte = Package.GetGlobalService(typeof(DTE)) as DTE2;

			if (Dte != null)
			{
				events = Dte.Events as Events2;
				if (events != null)
				{
					dteEvents = events.DTEEvents;
					solutionEvents = events.SolutionEvents;
					dteEvents.OnBeginShutdown += ShutDown;
					solutionEvents.Opened += () => SwitchStartupDir("\n====== Solution opening Detected ======\n");
				}
			}

			controller.setShell(optionMgr.Shell);

			controller.Init(GetProjectPath());

			controller.InvokeCmd("\n[Global Init Script ...]\n", optionMgr.getGlobalScript());
		}

		private void SwitchStartupDir(string msg)
		{
			var dir = GetProjectPath();
			controller.setPath(dir);
			controller.InvokeCmd(msg, optionMgr.cdPrefix() + dir);

			System.Threading.Thread.Sleep(200); //Wait dir changed
			
			controller.InvokeCmd("\n[Project Init Script ...]\n", optionMgr.getProjectScript());
		}

		private string GetProjectPath()
		{
			string path = Dte.Solution.FileName;
			if (string.IsNullOrEmpty(path))
			{
				return "c:\\";
			}

			path = Path.GetDirectoryName(path) ?? "c:\\";

			return path;
		}

		private void OnClear(object sender, EventArgs e)
		{
			controller.ClearOutput();
		}

		private void OnRestart(object sender, EventArgs e)
		{
			controller.setShell(optionMgr.Shell);
			controller.RestartProc();
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

		private void OnCopy(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Rst.SelectedText);
		}

		private void OnSwitch(object sender, RoutedEventArgs e)
		{
			SwitchStartupDir("\n====== Switch to solution dir ======\n");
		}

		private void ShutDown()
		{
			controller.Close();
		}

		public TextBox GetTextBox()
		{
			return this.Rst;
		}

		private void OnPaste(object sender, RoutedEventArgs e)
		{
			Rst.Paste();
		}
	}
}
