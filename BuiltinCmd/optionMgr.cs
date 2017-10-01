using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuiltinCmd
{
	static class optionMgr
	{
		static public string getGlobalScript()
		{
			return BuiltinCmdPackage.OptionsPage?.initScript ?? "";
		}

		static public string getProjectScript()
		{
			return BuiltinCmdPackage.OptionsPage?.projectInitScript ?? "";
		}

		static public bool usePS => BuiltinCmdPackage.OptionsPage?.usePS ?? false;

		static public string cdPrefix()
		{
			if (usePS)
			{
				return "cd ";
			}

			return "cd /d ";
		}

		static public string Shell
		{
			get
			{
				if (usePS)
				{
					return "powershell.exe";
				}

				return "cmd.exe";
			}
		}
	}
}
