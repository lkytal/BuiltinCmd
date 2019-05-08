using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuiltinCmd
{
	static class OptionMgr
	{
		public static string GetGlobalScript()
		{
			return BuiltinCmdPackage.OptionsPage?.InitScript ?? "";
		}

		public static string GetProjectScript()
		{
			return BuiltinCmdPackage.OptionsPage?.ProjectInitScript ?? "";
		}

		public static string Font => BuiltinCmdPackage.OptionsPage?.Font ?? "Consolas";

		public static bool UsePowerShell => BuiltinCmdPackage.OptionsPage?.UsePs ?? false;

		public static string CdPrefix()
		{
			if (UsePowerShell)
			{
				return "cd ";
			}

			return "cd /d ";
		}

		public static string Shell
		{
			get
			{
				if (UsePowerShell)
				{
					return "powershell.exe";
				}

				return "cmd.exe";
			}
		}
	}
}
