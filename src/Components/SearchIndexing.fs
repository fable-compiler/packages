module Fable.Packages.Components.SearchIndexing

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Fable.Packages.Types
open Fable.Packages.Types.CompositeTypes
open Fable.SimpleHttp
open Thoth.Json
open Browser.Dom

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

[<RequireQualifiedAccess>]
type private Step =
    | IndexingPackageList
    | IndexingLastPackageVersion of packages: V3.SearchResponse.Package list

type private IndexingProgress = {
    CurrentPage: int
    TotalPages: int
}

[<RequireQualifiedAccess>]
type private FetchPageResult =
    | Success of V3.SearchResponse.Package list
    | DeserializationError of string
    | MaximumPackagesReached

type SearchIndexingProps = {| OnReady: V3.SearchResponse.Package list -> unit |}

type Components with

    static member private DisplayProgressSteps
        (indexingProgress: IndexingProgress)
        =
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

    [<ReactComponent>]
    static member private IndexingLastPackageVersion
        (packages: V3.SearchResponse.Package list)
        (onCompleted: IndexedNuGetPackage list -> unit)
        =
        // In the future, we could use a web worker to cache previous request result
        // In general, a specific package version is not going to be updated often
        // Using a stale while revalidate strategy could be a good idea, it will give
        // a good boost in performance between visits and we will have the latest version
        // available most of the time
        let chunkSize = 50

        let indexingProgress, setIndexingProgress =
            React.useStateWithUpdater
                {
                    CurrentPage = 0
                    TotalPages = packages.Length
                }

        let task = async {
            let start = System.DateTime.Now

            let batches =
                packages
                |> Seq.chunkBySize chunkSize
                |> Seq.map (fun packages ->
                    packages
                    |> Seq.map (fun package ->
                        let packageRegistrationUrl =
                            package.Versions
                            |> List.tryFind (fun version ->
                                version.Version = package.Version
                            )
                            |> Option.map (fun versionInfo -> versionInfo.Id)

                        match packageRegistrationUrl with
                        | Some packageRegistrationUrl -> async {
                            let! response =
                                Http.request packageRegistrationUrl
                                |> Http.method GET
                                |> Http.header (
                                    Headers.accept "application/json"
                                )
                                |> Http.send

                            let jsonResult =
                                Decode.fromString
                                    NuGetRegistration5Semver1.decoder
                                    response.responseText

                            let result =
                                match jsonResult with
                                | Ok semverInfo ->
                                    {
                                        Package = package
                                        LastVersionInfo = semverInfo
                                    }
                                    |> Ok
                                | Error errorMessage -> Error errorMessage

                            return result
                          }

                        | None -> failwith "Package registration url not found"
                    )
                    |> Async.Parallel
                )

            let indexedPackages = ResizeArray(packages.Length)

            for batch in batches do
                let! batchResult = batch

                // Update progress
                setIndexingProgress (fun progress ->
                    { progress with
                        CurrentPage = progress.CurrentPage + batchResult.Length
                    }
                )

                for package in batchResult do
                    match package with
                    | Ok package -> indexedPackages.Add package
                    | Error error ->
                        // TODO: Inform user of potential errors and so missing packages
                        console.error error

            let finish = System.DateTime.Now
            let elapsed = finish - start
            printfn "Elapsed: %A" elapsed.TotalSeconds
            onCompleted (Seq.toList indexedPackages)
        }

        let _ =
            React.useDeferredNoCancel (
                task,
                [|
                    box onCompleted
                |]
            )

        Components.DisplayProgress(
            indexingProgress,
            "Fable.Packages is retrieving the packages information"
        )

    static member private DisplayProgress
        (
            indexingProgress: IndexingProgress,
            infoText: string
        ) =

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

                                Components.DisplayProgressSteps indexingProgress

                                Bulma.text.p [

                                    text.isItalic
                                    prop.text infoText

                                ]
                            ]
                        ]
                    ]
                ]
            ]
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
    static member IndexingPackageList(onCompleted: V3.SearchResponse.Package list -> unit) =
        // At the time of writting there are around 350 pacakges
        // tagged with `fable` using 50 elements per page means
        // we will have to make 7 requests to get all the packages
        // Processing the 7 requests takes around 2.5 sec which is acceptable
        // We could reduce the number of requests by increasing the number of
        // elements per page but I think that this is a good enough compromise
        // to be able to adapt in the future.
        // If more packages are added to the nuget registry we can increase the
        // number of elements per page accordingly.

        // FOLLOWING SECTION REPLACE ABOVE SECTION WHILE WAITING A FIX FROM NUGET.ORG API

        // Ordering is not stable between different page calls
        // This means that if we make several calls to the nuget registry
        // some packages are duplicated and others are missing.
        // To workaround this issue, we use the maximum page size of 1000
        // If that bug is fixed in the future, I would like to reduce the number of elements
        // per pages to have a better progress report for the user.
        // Workaround mentionned on this issue: https://github.com/NuGet/NuGetGallery/issues/7494
        let elementsPerPage = 1000
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
                    // Include prerelease packages and set the semVerLevel to 2.0.0
                    // to match the way NuGet.org search works
                    // See: https://github.com/NuGet/NuGetGallery/issues/9235
                    "prerelease=true"
                    "semVerLevel=2.0.0"
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

            return Decode.fromString V3.SearchResponse.decoder response.responseText
        }

        let rec fetchAllPages (currentPage: int) (packages: V3.SearchResponse.Package list) = async {
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
                else if currentPage >= totalPage then
                    let packages =
                        packages
                        // Make sure we have no duplicates
                        // This is a preventive measure because of inconsistent
                        // ordering from NuGet API.
                        // When the bug is fixed on their side, we should be able to remove this.
                        |> List.distinctBy (fun package -> package.Id)

                    return FetchPageResult.Success packages
                else
                    return! fetchAllPages (currentPage + 1) newPackages

            | Error error -> return FetchPageResult.DeserializationError error
        }

        let fetchPackages = async {
            match! fetchAllPages 0 [] with
            // All good, notify the parent component that we are ready
            | FetchPageResult.Success packages -> onCompleted packages

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
                    box onCompleted
                |]
            )

        Components.DisplayProgress(
            indexingProgress,
            "Fable.Packages is retrieving all the Fable packages from NuGet.org"
        )

    [<ReactComponent>]
    static member SearchIndexing(onReady: IndexedNuGetPackage list -> unit) =
        let currentStep, setCurrentStep =
            React.useState Step.IndexingPackageList

        let onIndexingPackageListCompleted (packages: V3.SearchResponse.Package list) =
            setCurrentStep (Step.IndexingLastPackageVersion packages)

        let onIndexingLastPackageVersionCompleted
            (indexedPackages: IndexedNuGetPackage list)
            =
            onReady indexedPackages

        match currentStep with
        | Step.IndexingPackageList ->
            Components.IndexingPackageList onIndexingPackageListCompleted

        | Step.IndexingLastPackageVersion packages ->
            Components.IndexingLastPackageVersion
                packages
                onIndexingLastPackageVersionCompleted
