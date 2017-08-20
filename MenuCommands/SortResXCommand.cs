using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.ComponentModel.Design;

namespace ResXTweaks
{
    internal sealed class SortResXCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("7de2cdae-f945-4bd2-8d7c-7aaba43eb8bb");

        private readonly Package package;
        private string[] resxFiles;

        private IServiceProvider ServiceProvider => package;

        private SortResXCommand(Package package)
        {
            this.package = package;

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += BeforeQueryStatus;

                commandService.AddCommand(menuItem);
            }
        }

        public static SortResXCommand Instance { get; private set; }

        public static void Initialize(Package package)
        {
            Instance = new SortResXCommand(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
            var monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            var commandState = SortCommandHelper.GetCommandState(monitorSelection);

            command.Visible = commandState.IsVisible;
            command.Enabled = commandState.IsEnabled;
            resxFiles = commandState.SelectedFiles;

            command.Text = (resxFiles != null && resxFiles.Length > 1)
                ? Strings.SortSelectedResX
                : Strings.SortCurrentResX;
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            if (resxFiles != null)
                SortCommandHelper.SortResXFiles(ServiceProvider, resxFiles, false);
        }
    }
}
