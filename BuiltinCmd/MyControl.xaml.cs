using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using Microsoft.Win32;
using EnvDTE;
using EnvDTE80;
using Package = Microsoft.VisualStudio.Shell.Package;
using CmdHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuiltinCmd
{
	/// <summary>
	/// Interaction logic for MyControl.xaml
	/// </summary>
	public partial class MyControl : UserControl, ITerminalBoxProvider
	{
		private readonly TerminalController terminalController;

		public MyControl()
		{
			InitializeComponent();

			Rst.BorderThickness = new Thickness(0, 0, 0, 0);
			Rst.UndoLimit = 100;
			Rst.FontFamily = new FontFamily(OptionMgr.Font);
			Rst.FontSize = OptionMgr.FontSize;
			Rst.Focus();

			terminalController = new TerminalController(this);
		}

		private DTE2 dte;
		private Events2 events;
		private DTEEvents dteEvents;
		private SolutionEvents solutionEvents;

		private bool firstRun = true;

		private void OnLoad(object sender, EventArgs e)
		{
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!firstRun)
			{
				return;
			}

			firstRun = false;

			Init();
		}

		private void Init()
		{
            ThreadHelper.ThrowIfNotOnUIThread();
            dte = Package.GetGlobalService(typeof(DTE)) as DTE2;

			if (dte != null)
			{
				events = dte.Events as Events2;
				if (events != null)
				{
					dteEvents = events.DTEEvents;
					solutionEvents = events.SolutionEvents;
					dteEvents.OnBeginShutdown += ShutDown;
					solutionEvents.Opened += () => SwitchStartupDir("\n====== Solution opening Detected ======\n");
				}
			}

			terminalController.SetShell(OptionMgr.Shell);

            bool createSuccess = terminalController.Init(GetProjectPath());

			if (!createSuccess)
			{
				VsShellUtilities.ShowMessageBox(
					ServiceProvider.GlobalProvider,
					"Can not create console process, check your configuration and reopen this window",
					"Can not create process",
					OLEMSGICON.OLEMSGICON_CRITICAL,
					OLEMSGBUTTON.OLEMSGBUTTON_OK,
					OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
			}

			terminalController.InvokeCmd("\n[Global Init Script ...]\n", OptionMgr.GetGlobalScript());
		}

		private void SwitchStartupDir(string msg)
		{
            ThreadHelper.ThrowIfNotOnUIThread();
            string dir = GetProjectPath();
			terminalController.SetPath(dir);
			terminalController.InvokeCmd(msg, OptionMgr.CdPrefix() + dir);

			System.Threading.Thread.Sleep(200); //Wait dir changed

			terminalController.InvokeCmd("\n[Project Init Script ...]\n", OptionMgr.GetProjectScript());
		}

		private string GetProjectPath()
		{
            ThreadHelper.ThrowIfNotOnUIThread();
            string path = dte.Solution.FileName;
			if (string.IsNullOrEmpty(path))
			{
				return "c:\\";
			}

			path = Path.GetDirectoryName(path) ?? "c:\\";

			return path;
		}

		private void OnClear(object sender, EventArgs e)
		{
			terminalController.ClearOutput();
		}

		private void OnRestart(object sender, EventArgs e)
		{
			terminalController.SetShell(OptionMgr.Shell);
			terminalController.RestartProc();
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
            ThreadHelper.ThrowIfNotOnUIThread();
            SwitchStartupDir("\n====== Switch to solution dir ======\n");
		}

		private void ShutDown()
		{
			terminalController.Close();
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
