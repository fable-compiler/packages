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

    let tryGetFromQueryParams (searchParams : Browser.Types.URLSearchParams) =
        match searchParams.get "sortBy" with
        | Some "relevance" -> Some SortBy.Relevance
        | Some "downloads" -> Some SortBy.Downloads
        | Some "recentlyUpdated" -> Some SortBy.RecentlyUpdated
        | _ -> None

    let toQueryParams (sortBy: SortBy) =
        match sortBy with
        | SortBy.Relevance -> "relevance"
        | SortBy.Downloads -> "downloads"
        | SortBy.RecentlyUpdated -> "recentlyUpdated"
