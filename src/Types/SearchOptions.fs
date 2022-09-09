namespace Fable.Packages.Types

type SearchOptions = {
    TextField: string
    Targets: Set<Target>
    PackageTypes: Set<PackageType>
    SortBy: NuGetSortBy
    Options: Set<NuGetOption>
}

module SearchOptions =

    let initial = {
        TextField = ""
        Targets = Set.empty
        PackageTypes = Set.empty
        SortBy = NuGetSortBy.Relevance
        Options =
            Set.ofList [
                NuGetOption.IncludePreRelease
            ]
    }
