namespace Fable.Packages.Types


[<RequireQualifiedAccess>]
type NuGetSortBy =
    | Relevance
    | Downloads
    | RecentlyUpdated


module NuGetSortBy =

    let toLabel (sortBy: NuGetSortBy) =
        match sortBy with
        | NuGetSortBy.Relevance -> "Relevance"
        | NuGetSortBy.Downloads -> "Downloads"
        | NuGetSortBy.RecentlyUpdated -> "Recently Updated"
