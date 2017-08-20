using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Runtime.InteropServices;

namespace ResXTweaks
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [Guid(Constants.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ResXTweaksPackage : Package
    {
        #region Package Members

        protected override void Initialize()
        {
            base.Initialize();

            SortResXCommand.Initialize(this);
            SortRelatedResXCommand.Initialize(this);
        }

        #endregion
    }
}
