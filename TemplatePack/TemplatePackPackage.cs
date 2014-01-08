using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TemplatePack
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidSideWafflePackage)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideBindingPath]
    public class TemplatePackPackage : Package
    {
    }
}
