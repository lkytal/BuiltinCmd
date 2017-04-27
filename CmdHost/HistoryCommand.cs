using System.Collections.Generic;

namespace CmdHost
{
	public class HistoryCommand
	{
		private readonly List<string> historyList = new List<string>();
		public int Index { get; private set; }

		public int Size => historyList.Count;

		public HistoryCommand()
		{
			Index = -1;
		}

		public void Add(string cmd)
		{
			if (cmd != null && !cmd.Equals(GetItem(Size - 1)))
			{
				historyList.Add(cmd);
			}

			Index = historyList.Count;
		}

		public string GetItem(int position)
		{
			if (position < 0 || position > Size - 1)
			{
				return null;
			}

			return historyList[position];
		}

		public string GetCurrent()
		{
			return GetItem(Index);
		}

		public string SelectPreviuos()
		{
			if (Index >= 1)
			{
				Index -= 1;
				return GetCurrent();
			}

			return null;
		}

		public string SelectNext()
		{
			string cmd = null;

			if (Index <= Size - 2)
			{
				Index += 1;
				cmd = GetCurrent();
			}

			return cmd;
		}
	}
}