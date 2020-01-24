using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace BuiltinCmd
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	// This attribute registers a tool window exposed by this package.
	[ProvideToolWindow(typeof(MyToolWindow))]
	[Guid(GuidList.guidBuiltinCmdPkgString)]
	[ProvideAutoLoad(UIContextGuids80.NoSolution)]
	[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
	[ProvideOptionPage(typeof(OptionsPage), "BuiltinCmd", "General", 0, 0, true)]

	public sealed class BuiltinCmdPackage : Package
	{
		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require
		/// any Visual Studio service because at this point the package object is created but
		/// not sited yet inside Visual Studio environment. The place to do all the other
		/// initialization is the Initialize method.
		/// </summary>
		public BuiltinCmdPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}

		/// <summary>
		/// This function is called when the user clicks the menu item that shows the
		/// tool window. See the Initialize method to see how the menu item is associated to
		/// this function using the OleMenuCommandService service and the MenuCommand class.
		/// </summary>
		private void ShowToolWindow(object sender, EventArgs e)
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
			if (window?.Frame == null)
			{
				throw new NotSupportedException(Resources.CanNotCreateWindow);
			}

			IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;

			if (windowFrame.IsVisible() == VSConstants.S_OK)
			{
				ErrorHandler.ThrowOnFailure(windowFrame.Hide());
			}
			else
			{
				ErrorHandler.ThrowOnFailure(windowFrame.Show());
			}
		}

		public static OptionsPage OptionsPage;

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Debug.WriteLine ($"Entering Initialize() of: {this}");
			base.Initialize();

			OptionsPage = base.GetDialogPage(typeof(OptionsPage)) as OptionsPage;

			// Add our command handlers for menu (commands must exist in the .vsct file)
			if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
			{
				// Create the command for the tool window
				CommandID toolwndCommandID = new CommandID(GuidList.guidBuiltinCmdCmdSet, (int)PkgCmdIDList.CmdWnd);
				MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
				mcs.AddCommand(menuToolWin);
			}
		}
	}
}
