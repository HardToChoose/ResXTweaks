using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;

namespace ResXTweaks
{
    static class ErrorHandler
    {
        public static void ShowErrorMessage(IServiceProvider serviceProvider, string message)
        {
            VsShellUtilities.ShowMessageBox(
                serviceProvider,
                message,
                "An unexpected error ocurred",
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public static void LogException(Exception ex)
        {

        }
    }
}
