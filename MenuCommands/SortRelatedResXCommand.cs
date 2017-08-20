using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.ComponentModel.Design;

namespace ResXTweaks
{
    internal sealed class SortRelatedResXCommand
    {
        public const int CommandId = 0x0200;

        public static readonly Guid CommandSet = new Guid("7de2cdae-f945-4bd2-8d7c-7aaba43eb8bb");

        private readonly Package package;
        private string[] resxFiles;

        private SortRelatedResXCommand(Package package)
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

        public static SortRelatedResXCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider => package;

        public static void Initialize(Package package)
        {
            Instance = new SortRelatedResXCommand(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
            var monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            var commandState = SortCommandHelper.GetCommandState(monitorSelection);

            command.Visible = commandState.IsVisible;
            command.Enabled = commandState.IsEnabled;
            resxFiles = commandState.SelectedFiles;

            command.Text = Strings.SortAllRelatedResX;
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            if (resxFiles != null)
                SortCommandHelper.SortResXFiles(ServiceProvider, resxFiles, true);
        }
    }
}
