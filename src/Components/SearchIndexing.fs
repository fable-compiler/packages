module Fable.Packages.Components.SearchIndexing

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Fable.Packages.Types
open Fable.SimpleHttp
open Thoth.Json
open Browser.Dom

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type private IndexingProgress = {
    CurrentPage: int
    TotalPages: int
}

[<RequireQualifiedAccess>]
type FetchPageResult =
    | Success of NuGetPackage list
    | DeserializationError of string
    | MaximumPackagesReached

type SearchIndexingProps = {| OnReady: NuGetPackage list -> unit |}

type Components with

    static member private DisplayProgress(indexingProgress: IndexingProgress) =
        let text =
            // Indexing info not available yet, we display an info message
            // to avoid layout jump when info become available
            if indexingProgress.TotalPages = -1 then
                "Calculation of work to be done"
            else
                $"Progress: {indexingProgress.CurrentPage} / {indexingProgress.TotalPages}"

        Bulma.text.p [
            size.isSize4
            color.hasTextGrey
            prop.text text
        ]

    /// <summary>
    /// Component used to index all the packages marked with `fable` tag.
    ///
    /// Note: There is a hard limitaton of a total of 4000 packages retriable
    /// by searching only on `fable` tag.
    ///
    /// nuget.org limits the skip parameter to 3,000 and the take parameter to 1,000.
    ///
    /// If we happen to have more than 4000 packages, we will need to serch using
    /// several tags and rebuild the full list of packages.
    ///
    /// For example, we could search by using `fable-binding` and ``fable-library``
    /// tags, which would allow a total of 8000 packages to be retrieved.
    ///
    /// And we could, go further by using searching using the language tags.
    /// </summary>
    /// <param name="onReady"></param>
    /// <returns></returns>
    [<ReactComponent>]
    static member SearchIndexing(onReady: NuGetPackage list -> unit) =
        // At the time of writting there are around 350 pacakges
        // tagged with `fable` using 50 elements per page means
        // we will have to make 7 requests to get all the packages
        // Processing the 7 requests takes around 2.5 sec which is acceptable
        // We could reduce the number of requests by increasing the number of
        // elements per page but I think that this is a good enough compromise
        // to be able to adapt in the future.
        // If more packages are added to the nuget registry we can increase the
        // number of elements per page accordingly.
        let elementsPerPage = 50
        let maximumPackages = 4000

        let indexingProgress, setIndexingProgress =
            React.useState
                {
                    CurrentPage = 0
                    TotalPages = -1 // Will be updated when the first request is made
                }

        let fetchPage (pageRank: int) = async {
            let queryParams =
                [
                    "q=Tags:\"fable\"" // We want to only the Fable packages
                    $"skip=%i{pageRank * elementsPerPage}"
                    $"take=%i{elementsPerPage}"
                    "prerelease=true"
                ]
                |> List.map window.encodeURI
                |> String.concat "&"

            let requestUrl =
                $"https://azuresearch-usnc.nuget.org/query?%s{queryParams}"

            let! response =
                Http.request requestUrl
                |> Http.method GET
                |> Http.header (Headers.accept "application/json")
                |> Http.send

            return Decode.fromString NuGetResponse.decoder response.responseText
        }

        let rec fetchAllPages (currentPage: int) (packages: NuGetPackage list) = async {
            let! currentPageResult = fetchPage currentPage

            match currentPageResult with
            | Ok pageResult ->
                let totalPage =
                    Helpers.computateTotalPageCount
                        pageResult.TotalHits
                        elementsPerPage

                setIndexingProgress
                    {
                        CurrentPage = currentPage
                        TotalPages = totalPage
                    }

                let newPackages = packages @ pageResult.Data

                if newPackages.Length > maximumPackages then
                    return FetchPageResult.MaximumPackagesReached
                else if currentPage > totalPage then
                    return FetchPageResult.Success packages
                else
                    return! fetchAllPages (currentPage + 1) newPackages

            | Error error -> return FetchPageResult.DeserializationError error
        }

        let fetchPackages = async {
            match! fetchAllPages 0 [] with
            // All good, notify the parent component that we are ready
            | FetchPageResult.Success packages -> onReady packages

            // Something went wrong, report the error
            | FetchPageResult.DeserializationError error ->
                printfn "DeserializationError: %A" error

            // We reached the maximum number of packages, report the error
            | FetchPageResult.MaximumPackagesReached -> ()
        }

        let _ =
            React.useDeferredNoCancel (
                fetchPackages,
                [|
                    box onReady
                |]
            )

        Bulma.hero [
            prop.className "is-fullheight-with-spaced-navbar"

            prop.children [
                Bulma.heroBody [
                    Bulma.container [
                        text.hasTextCentered

                        prop.children [

                            Html.p [
                                prop.className "title is-3"
                                prop.text "Indexing packages"
                            ]

                            Bulma.progress [
                                color.isPrimary

                                prop.value indexingProgress.CurrentPage
                                prop.max indexingProgress.TotalPages
                            ]

                            Bulma.content [

                                Components.DisplayProgress indexingProgress

                                Bulma.text.p [

                                    text.isItalic
                                    prop.text
                                        "Fable.Packages needs to index all the packages to be able to search them."
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
