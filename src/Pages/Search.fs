module Fable.Packages.Pages.Search

open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Fable.Packages.Types
open Fable.Packages.Components
open Fable.Packages.Components.SearchForm
open Fable.Packages.Components.Pagination
open Fable.Packages.Components.NuGetPackageMedia
open Fable.Packages.Components.SearchIndexing
open Fable.SimpleHttp
open Thoth.Json
open Fable.Core.JsInterop

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type private Steps =
    | IndexingInProgress
    | Ready of ResizeArray<IndexedNuGetPackage>

let private init () = IndexingInProgress

let private filterByPackageTypes
    (searchedPackageTypes: Set<PackageType>)
    (package: IndexedNuGetPackage)
    =
    // User didn't request a specific package type so no filter is applied
    // Always return true
    if searchedPackageTypes.IsEmpty then
        true
    else
        match package.Package.Tags with
        // The packages has some tags, so we can check if it contains
        // one of the searched package type
        | Some tags ->
            searchedPackageTypes
            |> Seq.exists (fun searchedPackageType ->
                List.contains (PackageType.toTag searchedPackageType) tags
            )
        // The package doesn't have any tags, so we apply the search on the package type
        // Discard that package
        | None -> false

let private filterByTargets
    (searchedTargets: Set<Target>)
    (package: IndexedNuGetPackage)
    =
    // User didn't request a specific package type so no filter is applied
    // Always return true
    if searchedTargets.IsEmpty then
        true
    else
        match package.Package.Tags with
        // The packages has some tags, so we can check if it contains
        // one of the searched package type
        | Some tags ->
            searchedTargets
            |> Seq.exists (fun searchedTarget ->
                List.contains (Target.toTag searchedTarget) tags
            )
        // The package doesn't have any tags, so we apply the search on the package type
        // Discard that package
        | None -> false

type Components with

    [<ReactComponent>]
    static member private ShowSearchFormAndResults
        (indexedPackages: ResizeArray<IndexedNuGetPackage>)
        =

        let activeSearchOptions, setActiveSearchOptions =
            React.useState SearchOptions.initial

        let elementsPerPage = 10
        // Store the current page displayed
        let currentPage, setCurrentPage = React.useState 0
        // Store the current packages list to display
        let matchedPackages, setMatchedPackages = React.useState (ResizeArray())

        let onSearch =
            React.useCallbackRef(fun (newSearchOptions: SearchOptions) ->
                if newSearchOptions <> activeSearchOptions then
                    // Reset the current page
                    setCurrentPage 0
                    // Update the search options
                    setActiveSearchOptions newSearchOptions
            )

        React.useEffect (
            fun () ->
                let result =
                    indexedPackages
                    // Filter by package type
                    |> Seq.filter (
                        filterByPackageTypes activeSearchOptions.PackageTypes
                    )
                    // Filter by target
                    |> Seq.filter (
                        filterByTargets activeSearchOptions.Targets
                    )
                    |> Seq.sortWith (fun packageA packageB ->
                        match activeSearchOptions.SortBy with
                        | SortBy.Relevance ->
                            compare packageA.Package.TotalDownloads packageB.Package.TotalDownloads * -1
                        | SortBy.Downloads ->
                            compare packageA.Package.TotalDownloads packageB.Package.TotalDownloads * -1
                        | SortBy.RecentlyUpdated ->
                            compare packageA.LastVersionInfo.Published packageB.LastVersionInfo.Published * -1
                    )
                    |> ResizeArray

                setMatchedPackages result
            , [|
                box indexedPackages
                box activeSearchOptions
                box currentPage
            |]
        )

        Html.div [
            Bulma.section [
                Components.SearchForm
                    {|
                        OnSearch = onSearch
                    |}
            ]

            if matchedPackages.Count > 0 then
                Html.div [
                    prop.className "packages-list"
                    matchedPackages
                    // Skip the previous pages
                    |> Seq.skip (currentPage * elementsPerPage)
                    // Take only the current page
                    |> Seq.truncate elementsPerPage
                    |> Seq.map (fun indexedPackage ->
                        Components.NuGetPackageMedia indexedPackage.Package
                    )
                    |> prop.children
                ]
            else
                Bulma.text.p [
                    text.hasTextCentered
                    text.isItalic
                    size.isSize4
                    color.hasTextGrey
                    prop.text "No packages found."
                ]

            Bulma.section [
                Components.Pagination
                    {|
                        CurrentPage = currentPage
                        TotalHits = matchedPackages.Count
                        OnNavigate = setCurrentPage
                        ElementsPerPage = elementsPerPage
                    |}
            ]
        ]

type Pages with

    [<ReactComponent>]
    static member Search() =
        let currentSteps, setCurrentSteps = React.useState IndexingInProgress

        match currentSteps with
        | IndexingInProgress ->
            Components.SearchIndexing(fun packages ->
                setCurrentSteps (Ready(ResizeArray packages))
            )

        | Ready indexedPackages ->
            Components.ShowSearchFormAndResults indexedPackages
