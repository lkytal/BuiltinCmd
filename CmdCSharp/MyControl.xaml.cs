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
using Microsoft.VisualStudio.Shell;

namespace Lx.CmdCSharp
{
	/// <summary>
	/// Interaction logic for MyControl.xaml
	/// </summary>
	public partial class MyControl : UserControl
	{
		public MyControl()
		{
			InitializeComponent();
		}

		private Process Proc;
		delegate void TextDelegate(string txt);
		delegate void ScrollDelegate();

		private void AddResult(string text)
		{
			TextDelegate AppendText = new TextDelegate(Rst.AppendText);
			this.Dispatcher.Invoke(AppendText, text + Environment.NewLine);

			ScrollDelegate Scroll = new ScrollDelegate(Rst.ScrollToEnd);
			this.Dispatcher.Invoke(Scroll);
		}

		private void Init()
		{
			ProcessStartInfo ProArgs = new ProcessStartInfo("cmd.exe");
			ProArgs.CreateNoWindow = true;
			ProArgs.RedirectStandardOutput = true;
			ProArgs.RedirectStandardInput = true;
			ProArgs.RedirectStandardError = true;
			ProArgs.UseShellExecute = false;

			Proc = Process.Start(ProArgs);
			Proc.EnableRaisingEvents = true;
			Proc.OutputDataReceived += (sender, events) => AddResult(events.Data);
			Proc.ErrorDataReceived += (sender, events) => AddResult(events.Data);
			Proc.BeginOutputReadLine();
			Proc.BeginErrorReadLine();

			Proc.Exited += (sender, events) => Init();
		}

		private bool FirstRun = true;

		private void OnLoad(object sender, EventArgs e)
		{
			if (FirstRun)
			{
				FirstRun = false;
				Rst.BorderThickness = new Thickness(0, 0, 0, 0);
				Init();
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

		private void Run(object sender, EventArgs e)
		{
			RunCmd();
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
	}
}
