using System;
using System.Windows.Input;

namespace Wpf
{
	public class Controller
	{
		private readonly MainWindow mainWindow;
		private readonly HistoryCommand historyCommand;
		private readonly CmdReader cmdReader;
		private readonly TabHandler tabHandler;
		public readonly Terminal terminal;

		public string Input = "";

		public Controller(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;

			historyCommand = new HistoryCommand();
			cmdReader = new CmdReader(this);
			tabHandler = new TabHandler(this);
			terminal = new Terminal(mainWindow.Rst);
		}

		public void Init()
		{
			cmdReader.Init();
		}

		public void AddData(string outputs)
		{
			tabHandler.ExtractDir(ref outputs);
			tabHandler.ResetTabComplete();

			Action act = () =>
			{
				terminal.AppendText(outputs);
			};

			mainWindow.Dispatcher.BeginInvoke(act);
		}

		private void RunCmd()
		{
			string cmd = terminal.GetInput();

			if (cmd == "cls")
			{
				Action act = () =>
				{
					terminal.Clear();
				};

				mainWindow.Dispatcher.Invoke(act);

				cmdReader.Input("");
			}
			else
			{
				Action act = () =>
				{
					terminal.removeInput();
				};

				mainWindow.Dispatcher.Invoke(act); //No async, ensure is done

				cmdReader.Input(cmd);
			}

			historyCommand.Add(cmd);
		}

		public void HandleInput(KeyEventArgs e)
		{
			if (terminal.Rst.CaretIndex < terminal.DataLen)
			{
				if (e.Key != Key.Left && e.Key != Key.Right)
				{
					e.Handled = true;
				}

				return;
			}

			if (e.Key == Key.Back && terminal.Rst.CaretIndex <= terminal.DataLen)
			{
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Tab)
			{
				e.Handled = true;

				if (tabHandler.HandleTab(Input)) return;
			}

			tabHandler.ResetTabComplete();

			if (e.Key == Key.Up)
			{
				string cmd = historyCommand.SelectPreviuos();
				if (cmd != null)
				{
					terminal.Text = terminal.Text.Substring(0, terminal.DataLen) + cmd;
					terminal.Rst.Select(terminal.Text.Length, 0);
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Down)
			{
				string previousCmd = historyCommand.SelectNext();

				if (previousCmd != null)
				{
					terminal.Text = terminal.Text.Substring(0, terminal.DataLen) + previousCmd;
					terminal.Rst.Select(terminal.Text.Length, 0);
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
				RunCmd();

				e.Handled = true;
			}
		}

		public void Close()
		{
			cmdReader.Close();
		}

		public void ClearOutput()
		{
			terminal.Clear();
			cmdReader.Input("");
		}

		public void RestartProc()
		{
			terminal.Clear();
			cmdReader.Restart();
		}

		public void Inputed(KeyEventArgs ev)
		{
			if (ev.Key != Key.Tab)
			{
				Input = terminal.GetInput();
			}
		}
	}
}