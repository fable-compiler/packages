module Fable.Packages.Pages.PackageHome

open Fable.Core
open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Fable.Packages
open Fable.Packages.Types
open Fable.Packages.Components
open Fable.Packages.Components.AnErrorOccured
open Fable.Packages.Components.PackageHome.PageHeader
open Fable.Packages.Components.PackageHome.Versions
open Fable.Packages.Components.PackageHome.Dependencies
open Fable.Packages.Components.PackageHome.Readme
open Fable.SimpleHttp
open Thoth.Json
open Fable.Core.JsInterop
open Browser.Dom
open Feliz.Lucide
open Fable.ZipJs

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

let private zip: Zip = importAll "@zip.js/zip.js"

type private DetailedError = {
    Error: string
    Details: string
}

type private PackageFoundInfo = {
    Package: V3.SearchResponse.Package
    CatalogPage: V3.CatalogRoot.CatalogPage
    Readme : string option
}

[<RequireQualifiedAccess>]
type private DataStatus =
    | PackageNotFound
    | MultiplePackagesFound
    | Errored of string
    | PackageFound of PackageFoundInfo

[<RequireQualifiedAccess>]
type private Tab =
    | Readme
    | Dependencies
    | UsedBy
    | Versions
    | ReleaseNotes

type Components with

    [<ReactComponent>]
    static member private Tab
        (
            tabToRender: Tab,
            activeTab: Tab,
            onClick: Tab -> unit
        ) =

        let (iconElement, text) =
            match tabToRender with
            | Tab.Readme -> Lucide.Book, "Readme"
            | Tab.Dependencies -> Lucide.Boxes, "Dependencies"
            | Tab.UsedBy -> Lucide.GitFork, "Used by"
            | Tab.Versions -> Lucide.History, "Versions"
            | Tab.ReleaseNotes -> Lucide.BookOpen, "Release notes"

        Bulma.tab [
            if tabToRender = activeTab then
                tab.isActive

            prop.children [
                Html.a [
                    prop.onClick (fun _ -> onClick tabToRender)
                    prop.children [
                        Bulma.icon [
                            iconElement [
                                lucide.size 24
                            ]
                        ]
                        Html.span text
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member private TabsHeader
        (
            activeTab: Tab,
            setActiveTab: Tab -> unit
        ) =
        Html.div [
            prop.className "package-home-tabs"
            prop.children [
                Bulma.tabs [
                    tabs.isCentered
                    tabs.isToggle
                    tabs.isFullWidth

                    prop.children [
                        Html.ul [
                            Components.Tab(Tab.Readme, activeTab, setActiveTab)
                            Components.Tab(
                                Tab.Dependencies,
                                activeTab,
                                setActiveTab
                            )
                            Components.Tab(Tab.UsedBy, activeTab, setActiveTab)
                            Components.Tab(
                                Tab.Versions,
                                activeTab,
                                setActiveTab
                            )
                            Components.Tab(
                                Tab.ReleaseNotes,
                                activeTab,
                                setActiveTab
                            )
                        ]
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member private TabBody(activeTab: Tab, info: PackageFoundInfo) =
        Html.div [
            prop.className "tab-body box"
            prop.children [
                match activeTab with
                | Tab.Readme -> Components.Readme info.Readme
                | Tab.Dependencies ->
                    Components.Dependencies
                        info.Package
                        info.CatalogPage
                        info.Package.Version
                | Tab.UsedBy ->
                    // Components.UsedBy info.Package
                    Html.div "Components.UsedBy info.Package"
                | Tab.Versions ->
                    Components.Versions info.Package info.CatalogPage
                | Tab.ReleaseNotes ->
                    // Components.ReleaseNotes info.Package
                    Html.div "Components.ReleaseNotes info.Package"
            ]
        ]

    [<ReactComponent>]
    static member private PackageNotFound(packageId: string) =
        Components.AnErrorOccured(
            "Package not found",
            $"No package found for id '%s{packageId}'"
        )

    [<ReactComponent>]
    static member private MultiplePackagesFound(packageId: string) =
        Components.AnErrorOccured(
            "Multiple package found",
            $"Multiple packages found for id '%s{packageId}', this should not happen please report it"
        )

    [<ReactComponent>]
    static member private Errored(message: string) =
        Components.AnErrorOccured("An unexpected error occured", message)

/// <summary>
/// Retrieve the README.md file content if present in the package
/// </summary>
/// <param name="entries"></param>
/// <typeparam name="'a"></typeparam>
/// <returns></returns>
let private tryExtractReadme (entries : IEntry array) =
    promise {
        // Naive way to retrieve the readme file
        // In the future, we should parse the nuspec file to retrieve the readme file
        let readmeEntry =
            entries
            |> Array.tryFind (fun entry -> entry.filename.ToLowerInvariant() = "readme.md")

        match readmeEntry with
        | Some entry ->
            let! content = entry.getDataString (zip.createStringWriter())
            return Some content

        | None -> return None
    }

let private fetchPackageContent
    (packageId : string)
    (packageVersion : string)
    (catalogPage: V3.CatalogRoot.CatalogPage)
    =
    async {
        match catalogPage.Items with
        | Some packages ->
            let packageContent =
                packages
                |> List.tryFind (fun package ->
                    package.CatalogEntry.Version = packageVersion
                )
                |> Option.map (fun package -> package.PackageContent)

            match packageContent with
            | Some packageContent ->
                let! response =
                    Http.request packageContent
                    |> Http.method GET
                    |> Http.header (Headers.accept "application/json")
                    |> Http.overrideResponseType ResponseTypes.Blob
                    |> Http.send

                match response.content with
                | ResponseContent.Blob blob ->
                    let zipReader = zip.createZipReader blob

                    let! entries =
                        zipReader.getEntries()
                        |> Async.AwaitPromise

                    let! readmeOpt = tryExtractReadme entries |> Async.AwaitPromise

                    return Ok readmeOpt

                | _ ->
                    return failwith "Unexpected response content type"
            | None ->
                return Error $"Could not find package content URL for %s{packageId}@%s{packageVersion}"

        | None ->
            return Error "Missing catalog page items"
    }

let private fetchCatalogPage (catalogRootUrl: string) = async {
    let! response =
        Http.request catalogRootUrl
        |> Http.method GET
        |> Http.header (Headers.accept "application/json")
        |> Http.send

    let jsonResponse =
        Decode.fromString V3.CatalogRoot.decoder response.responseText

    match jsonResponse with
    | Ok catalogRoot ->
        if catalogRoot.Count = 1 then
            return Ok catalogRoot.Items.Head
        else
            return failwith "Catalog root should have only one item"

    | Error errorMessage -> return Error errorMessage
}

let private fetchPackageInfo (packageId: string) (packageVersion : string option) : Async<DataStatus> = async {
    let queryParams =
        [
            $"q=PackageId:\"%s{packageId}\"" // We want to only the Fable packages
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

    let jsonResponse =
        Decode.fromString V3.SearchResponse.decoder response.responseText

    match jsonResponse with
    | Ok searchResponse ->
        if searchResponse.TotalHits = 0 then
            return DataStatus.PackageNotFound
        else if searchResponse.TotalHits > 1 then
            return DataStatus.MultiplePackagesFound
        else
            match searchResponse.Data.Head.Registration with
            | Some registrationUrl ->
                match! fetchCatalogPage registrationUrl with
                | Ok catalogPage ->
                    let requestedVersion =
                        match packageVersion with
                        | Some packageVersion -> packageVersion
                        | None -> searchResponse.Data.Head.Version
                    let! readmeResult = fetchPackageContent packageId requestedVersion catalogPage

                    match readmeResult with
                    | Ok readmeOpt ->
                        let result = {
                            Package = searchResponse.Data.Head
                            CatalogPage = catalogPage
                            Readme = readmeOpt
                        }

                        return DataStatus.PackageFound result
                    | Error errorMessage ->
                        return DataStatus.Errored errorMessage

                | Error errorMessage -> return DataStatus.Errored errorMessage
            | None ->

                return
                    [
                        "Missing registration URL"
                        ""
                        "Cannot fetch the package details"
                    ]
                    |> String.concat "\n"
                    |> DataStatus.Errored

    | Error errorMessage ->
        return
            [
                "Error while decoding the search response JSON"
                ""
                errorMessage
            ]
            |> String.concat "\n"
            |> DataStatus.Errored
}

type Pages with

    [<ReactComponent>]
    static member PackageHome(parameters: Router.PackageParameters) =
        let activeTab, setActiveTab =
            React.useState Tab.Readme

        let package =
            React.useDeferredNoCancel (
                fetchPackageInfo parameters.PackageId parameters.Version,
                [|
                    parameters
                |]
            )

        match package with
        | Deferred.HasNotStartedYet -> Html.none
        | Deferred.InProgress -> Html.none
        | Deferred.Failed _ -> Html.none
        | Deferred.Resolved DataStatus.PackageNotFound ->
            Components.PackageNotFound parameters.PackageId
        | Deferred.Resolved DataStatus.MultiplePackagesFound ->
            Components.MultiplePackagesFound parameters.PackageId
        | Deferred.Resolved (DataStatus.Errored error) ->
            Components.Errored error
        | Deferred.Resolved (DataStatus.PackageFound info) ->
            Html.div [
                Bulma.section [
                    Components.PageHeader info.Package
                    Components.TabsHeader(activeTab, setActiveTab)
                    Components.TabBody(activeTab, info)
                ]
            ]
