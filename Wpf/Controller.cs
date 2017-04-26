using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf
{
	public class Controller
	{
		private readonly MainWindow mainWindow;
		private readonly object Locker = new object();
		public readonly HistoryCommand historyCommand;
		public readonly CmdReader cmdReader;
		public readonly TabHandler tabHandler;

		public int RstLen;
		public TextBox Rst;

		public Controller(MainWindow mainWindow)
		{
			historyCommand = new HistoryCommand();
			cmdReader = new CmdReader(this);
			tabHandler = new TabHandler(this);
			
			this.mainWindow = mainWindow;
			Rst = mainWindow.Rst;
		}

		public void AddData(string outputs)
		{
			tabHandler.ExtractDir(ref outputs);
			tabHandler.Reset(RstLen);

			Action act = () =>
			{
				mainWindow.Rst.AppendText(outputs);
				RstLen = mainWindow.Rst.Text.Length;
				mainWindow.Rst.Select(RstLen, 0);
			};

			mainWindow.Dispatcher.BeginInvoke(act);
		}

		private void RunCmd(string cmd)
		{
			if (cmd == "cls")
			{
				Action act = () =>
				{
					mainWindow.Rst.Text = "";
					RstLen = 0;

					cmdReader.Input("");
				};

				mainWindow.Dispatcher.BeginInvoke(act);
			}
			else
			{
				lock (Locker)
				{
					mainWindow.Rst.Text = mainWindow.Rst.Text.Substring(0, RstLen);

					cmdReader.Input(cmd);
				}
			}

			historyCommand.Add(cmd);
		}


		public void HandleInput(KeyEventArgs e)
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

				if (tabHandler.HandleTab()) return;
			}

			tabHandler.ResetTabComplete(e.Key);

			if (e.Key == Key.Up)
			{
				string cmd = historyCommand.SelectPreviuos();
				if (cmd != null)
				{
					Rst.Text = Rst.Text.Substring(0, RstLen) + cmd;
					Rst.Select(Rst.Text.Length, 0);
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Down)
			{
				string previousCmd = historyCommand.SelectNext();

				if (previousCmd != null)
				{
					Rst.Text = Rst.Text.Substring(0, RstLen) + previousCmd;
					Rst.Select(Rst.Text.Length, 0);
				}

				e.Handled = true;
			}
			else if (e.Key == Key.C && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)) //Keyboard.IsKeyDown(Key.LeftCtrl)
			{
				cmdReader.SendCtrlC();

				e.Handled = true;
			}
			else if (e.Key == Key.Return)
			{
				string cmd = Rst.Text.Substring(RstLen, Rst.Text.Length - RstLen);

				RunCmd(cmd);

				e.Handled = true;
			}
		}

		public void Close()
		{
			cmdReader.Close();
		}

		public void ClearOutput()
		{
			Rst.Text = "";
			RstLen = 0;
			cmdReader.Input("");
		}

		public void RestartProc()
		{
			cmdReader.Restart();
			Rst.Text = "";
			RstLen = 0;
		}
	}
}