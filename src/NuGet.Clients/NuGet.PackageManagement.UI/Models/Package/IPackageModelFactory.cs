using NuGet.VisualStudio.Internal.Contracts;
using ContractItemFilter = NuGet.VisualStudio.Internal.Contracts.ItemFilter;

namespace NuGet.PackageManagement.UI.Models.Package
{
    public interface IPackageModelFactory
    {
        PackageModel Create(string identity, VersionInfoContextInfo version);
        PackageModel Create(PackageSearchMetadataContextInfo metadata, ContractItemFilter itemFilter);
    }
}
