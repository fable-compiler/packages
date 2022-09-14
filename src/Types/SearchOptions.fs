namespace Fable.Packages.Types

type SearchOptions = {
    TextField: string
    Targets: Set<Target>
    PackageTypes: Set<PackageType>
    SortBy: SortBy
    Options: Set<NuGetOption>
}

module SearchOptions =

    let initial = {
        TextField = ""
        Targets = Set.empty
        PackageTypes = Set.empty
        SortBy = SortBy.Relevance
        Options =
            Set.ofList [
                NuGetOption.IncludePreRelease
            ]
    }
