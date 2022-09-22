namespace Fable.Packages

open Feliz.Router
open Fable.Packages.Types
open Browser.Url
open System

[<RequireQualifiedAccess>]
module Router =

    module private Helpers =

        module URLSearchParams =

            let tryGetBool
                (value: string)
                (searchParams: Browser.Types.URLSearchParams)
                =
                match searchParams.get value with
                | Some ("1" | "true") -> Some true
                | Some ("0" | "false") -> Some false
                | _ -> None

            let tryGetSortBy (searchParams: Browser.Types.URLSearchParams) =
                match searchParams.get "sortBy" with
                | Some "downloads" -> Some SortBy.Downloads
                | Some "recentlyUpdated" -> Some SortBy.RecentlyUpdated
                | _ -> None

    type PackageParameters = {
        PackageId: string
        Version: string option
    }

    type SearchParameters = {|
        TargetDotnet: bool
        TargetJavaScript: bool
        TargetRust: bool
        TargetPython: bool
        TargetDart: bool
        TargetAll: bool
        SearchForBindings: bool
        SearchForLibraries: bool
        SortBy: SortBy
        IncludePrerelease: bool
        Query : string
    |}

    module SearchParameters =

        let initial =
            {|
                TargetDotnet = false
                TargetJavaScript = false
                TargetRust = false
                TargetPython = false
                TargetDart = false
                TargetAll = false
                SearchForBindings = false
                SearchForLibraries = false
                SortBy = SortBy.Relevance
                IncludePrerelease = true
            |}

    [<RequireQualifiedAccess>]
    type Page =
        | Search of SearchParameters option
        | Package of PackageParameters
        | NotFound

    let parseUrl =
        function
        | []
        | [ "search" ] -> Page.Search None
        | [ "search"; queryParams ] ->
            let searchParams = URLSearchParams.Create(queryParams)

            {|
                TargetDotnet =
                    Helpers.URLSearchParams.tryGetBool "dotnet" searchParams
                    |> Option.defaultValue false
                TargetJavaScript =
                    Helpers.URLSearchParams.tryGetBool "javascript" searchParams
                    |> Option.defaultValue false
                TargetRust =
                    Helpers.URLSearchParams.tryGetBool "rust" searchParams
                    |> Option.defaultValue false
                TargetPython =
                    Helpers.URLSearchParams.tryGetBool "python" searchParams
                    |> Option.defaultValue false
                TargetDart =
                    Helpers.URLSearchParams.tryGetBool "dart" searchParams
                    |> Option.defaultValue false
                TargetAll =
                    Helpers.URLSearchParams.tryGetBool "all" searchParams
                    |> Option.defaultValue false
                SearchForBindings =
                    Helpers.URLSearchParams.tryGetBool "bindings" searchParams
                    |> Option.defaultValue false
                SearchForLibraries =
                    Helpers.URLSearchParams.tryGetBool "libraries" searchParams
                    |> Option.defaultValue false
                SortBy =
                    SortBy.tryGetFromQueryParams searchParams
                    |> Option.defaultValue SortBy.Relevance
                IncludePrerelease =
                    Helpers.URLSearchParams.tryGetBool
                        "includePrerelease"
                        searchParams
                    |> Option.defaultValue true
                Query =
                    searchParams.get "query"
                    |> Option.defaultValue ""
            |}
            |> Some
            |> Page.Search
        | [ "package"; packageId ] ->
            Page.Package
                {
                    PackageId = packageId
                    Version = None
                }
        | [ "package"; packageId; version ] ->
            Page.Package
                {
                    PackageId = packageId
                    Version = Some version
                }
        | _ -> Page.NotFound

    let toUrl (page: Page) =
        match page with
        | Page.Search None -> Router.format ("search")
        | Page.Search (Some parameters) ->
            Router.format (
                "search",
                [
                    "dotnet", string parameters.TargetDotnet
                    "javascript", string parameters.TargetJavaScript
                    "rust", string parameters.TargetRust
                    "python", string parameters.TargetPython
                    "dart", string parameters.TargetDart
                    "all", string parameters.TargetAll
                    "bindings", string parameters.SearchForBindings
                    "libraries", string parameters.SearchForLibraries
                    "sortBy", SortBy.toQueryParams parameters.SortBy
                    "includePrerelease", string parameters.IncludePrerelease
                    if not (String.IsNullOrEmpty parameters.Query) then
                        "query", parameters.Query
                ]
            )
        | Page.Package parameters ->
            match parameters.Version with
            | Some version ->
                Router.format ("package", parameters.PackageId, version)
            | None -> Router.format ("package", parameters.PackageId)
        | Page.NotFound -> Router.format ("not-found")
