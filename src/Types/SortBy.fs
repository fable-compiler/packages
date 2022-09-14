namespace Fable.Packages.Types


[<RequireQualifiedAccess>]
type SortBy =
    | Relevance
    | Downloads
    | RecentlyUpdated


module SortBy =

    let toLabel (sortBy: SortBy) =
        match sortBy with
        | SortBy.Relevance -> "Relevance"
        | SortBy.Downloads -> "Downloads"
        | SortBy.RecentlyUpdated -> "Recently Updated"
