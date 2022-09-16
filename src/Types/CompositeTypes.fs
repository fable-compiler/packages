/// <summary>
/// This namespace contains types that are composed of other types
/// </summary>
namespace Fable.Packages.Types.CompositeTypes

open Fable.Packages.Types

type IndexedNuGetPackage = {
    Package: V3.SearchResponse.Package
    LastVersionInfo: NuGetRegistration5Semver1
}
