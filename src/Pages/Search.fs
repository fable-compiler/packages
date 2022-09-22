module Fable.Packages.Pages.Search

open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Fable.Packages.Types
open Fable.Packages
open Fable.Packages.Types.CompositeTypes
open Fable.Packages.Components
open Fable.Packages.Components.SearchForm
open Fable.Packages.Components.Pagination
open Fable.Packages.Components.NuGetPackageMedia
open Fable.Packages.Components.SearchIndexing
open Fable.Core
open Fable.Core.JsInterop
open Glutinum.Fuse
open System
open System.Text.RegularExpressions

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
                Array.contains (PackageType.toTag searchedPackageType) tags
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
                Array.contains (Target.toTag searchedTarget) tags
            )
        // The package doesn't have any tags, so we apply the search on the package type
        // Discard that package
        | None -> false

let private filterByTags
    (searchedTags: string list)
    (package: IndexedNuGetPackage)
    =
    // User didn't request a specific package type so no filter is applied
    // Always return true
    if searchedTags.IsEmpty then
        true
    else
        match package.Package.Tags with
        // The packages has some tags, so we can check if it contains
        // one of the searched package type
        | Some tags ->
            searchedTags
            |> Seq.exists (fun searchedTag -> Array.contains searchedTag tags)
        // The package doesn't have any tags, so we apply the search on the package type
        // Discard that package
        | None -> false

let private filterByTerms
    (searchedText: string)
    (indexedPackages: ResizeArray<IndexedNuGetPackage>)
    =
    let fuse =
        fuse.Create(
            indexedPackages,
            Fuse.IFuseOptions<_>(
                keys = [|
                    U3.Case1(Fuse.FuseOptionKeyObject("Package.Id", 10))
                    U3.Case1(Fuse.FuseOptionKeyObject("Package.Description", 2))
                    U3.Case1(Fuse.FuseOptionKeyObject("Package.Tags", 1))
                |],
                distance = 50,
                includeScore = true,
                threshold = 0.4
            // A threshold value of 0.4 seems to be the right spot to not
            // retrieve Fable.Core when searching for "json" term
            // I think Fable.Core can match "json" term because of "js" tags
            )
        )

    fuse.search searchedText

type Components with

    [<ReactComponent>]
    static member private ShowSearchFormAndResults
        (indexedPackages: ResizeArray<IndexedNuGetPackage>)
        (urlParameters: Router.SearchParameters option)
        =
        let activeSearchOptions, setActiveSearchOptions =
            React.useState SearchOptions.initial

        let elementsPerPage = 10
        // Store the current page displayed
        let currentPage, setCurrentPage = React.useState 0
        // Store the current packages list to display
        let matchedPackages, setMatchedPackages = React.useState (ResizeArray())

        let onSearch =
            React.useCallbackRef (fun (newSearchOptions: SearchOptions) ->
                if newSearchOptions <> activeSearchOptions then
                    // Reset the current page
                    setCurrentPage 0
                    // Update the search options
                    setActiveSearchOptions newSearchOptions
            )

        // // This effect serve to handle urlParameters changes
        // React.useEffect (
        //     fun () ->
        //         let newSearchOptions =
        //             match urlParameters with
        //             | Some urlParameters ->
        //                 {
        //                     TextField = urlParameters.Query
        //                     Targets =
        //                         Set.ofList [
        //                             if urlParameters.TargetDotnet then
        //                                 Target.Dotnet
        //                             if urlParameters.TargetJavaScript then
        //                                 Target.JavaScript
        //                             if urlParameters.TargetRust then
        //                                 Target.Rust
        //                             if urlParameters.TargetPython then
        //                                 Target.Python
        //                             if urlParameters.TargetDart then
        //                                 Target.Dart
        //                             if urlParameters.TargetAll then
        //                                 Target.All
        //                         ]
        //                     PackageTypes =
        //                         Set.ofList [
        //                             if urlParameters.SearchForBindings then
        //                                 PackageType.Binding
        //                             if urlParameters.SearchForLibraries then
        //                                 PackageType.Library
        //                         ]
        //                     SortBy = urlParameters.SortBy
        //                     Options =
        //                         Set.ofList [
        //                             if urlParameters.IncludePrerelease then
        //                                 NuGetOption.IncludePreRelease
        //                         ]
        //                 }
        //             | None ->
        //                 SearchOptions.initial

        //         onSearch newSearchOptions
        //     , [|
        //         box urlParameters
        //     |]
        // )

        // This effect control when the search is triggered
        React.useEffect (
            fun () ->
                let tagPattern = "tag:(?<tag>[^ ]+)"

                // Get the text to search by without the tags special search
                let searchedText =
                    Regex.Replace(activeSearchOptions.TextField, tagPattern, "")

                // Get the list of tags to search by
                let tags =
                    Regex.Matches(activeSearchOptions.TextField, tagPattern)
                    |> Seq.map (fun m -> m.Groups.["tag"].Value)
                    |> Seq.toList

                let intermediateResult =
                    indexedPackages
                    // Filter by package type
                    |> Seq.filter (
                        filterByPackageTypes activeSearchOptions.PackageTypes
                    )
                    // Filter by target
                    |> Seq.filter (filterByTargets activeSearchOptions.Targets)
                    // Filter by tags
                    |> Seq.filter (filterByTags tags)
                    |> ResizeArray

                let result =
                    if String.IsNullOrEmpty searchedText then
                        intermediateResult
                        |> Seq.sortByDescending (fun package ->
                            match activeSearchOptions.SortBy with
                            | SortBy.Relevance -> package.Package.TotalDownloads
                            | SortBy.Downloads -> package.Package.TotalDownloads
                            | SortBy.RecentlyUpdated ->
                                Some package.LastVersionInfo.Published.Ticks
                        )
                    else
                        filterByTerms searchedText intermediateResult
                        |> Seq.sortWith (fun packageA packageB ->
                            match activeSearchOptions.SortBy with
                            | SortBy.Relevance ->
                                // Try to combine fuse score and total downloads to get the best result
                                // A good test for this formula is to search for "json"
                                // And we should report "Thoth.Json", "Fable.SimpleJson" way before "Toth.Json"
                                // "Toth.Json" is a deprecated package with almost no download. It was changed to
                                // "Thoth.Json" because "toth" is a vulgar abbreviation in English, even if Toth
                                // the right name of the god of same name in french

                                let scoreA =
                                    packageA.score
                                    // Make 1 aim for perfect match
                                    |> Option.map (fun score -> score * -1.0)
                                    // We should always have a score because we configured Fuse to return it
                                    |> Option.get

                                let scoreB =
                                    packageB.score
                                    // Make 1 aim for perfect match
                                    |> Option.map (fun score -> score * -1.0)
                                    // We should always have a score because we configured Fuse to return it
                                    |> Option.get

                                let totalDownloadsA =
                                    packageA.item.Package.TotalDownloads
                                    |> Option.map float
                                    |> Option.get

                                let totalDownloadsB =
                                    packageB.item.Package.TotalDownloads
                                    |> Option.map float
                                    |> Option.get

                                // The formula has been made in an empirical way
                                // We need to multiply the score value in order to give it more weight
                                // And we need to divide the total downloads by a reference value in order to give it less weight

                                let sortReferenceA =
                                    scoreA * 10000.0 + totalDownloadsA / 100.0

                                let sortReferenceB =
                                    scoreB * 10000.0 + totalDownloadsB / 100.0

                                compare sortReferenceA sortReferenceB * -1
                            | SortBy.Downloads ->
                                compare
                                    packageA.item.Package.TotalDownloads
                                    packageB.item.Package.TotalDownloads
                                * -1
                            | SortBy.RecentlyUpdated ->
                                compare
                                    packageA.item.LastVersionInfo.Published
                                    packageB.item.LastVersionInfo.Published
                                * -1
                        )
                        |> Seq.map (fun fuseResult -> fuseResult.item)

                setMatchedPackages (ResizeArray result)
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
                        UrlParameters = urlParameters
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

            // Only show the pagination if there is more than one page
            if matchedPackages.Count > elementsPerPage then
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
    static member Search(parameters: Router.SearchParameters option) =
        let currentSteps, setCurrentSteps = React.useState IndexingInProgress

        match currentSteps with
        | IndexingInProgress ->
            Components.SearchIndexing(fun packages ->
                setCurrentSteps (Ready(ResizeArray packages))
            )

        | Ready indexedPackages ->
            Components.ShowSearchFormAndResults indexedPackages parameters
