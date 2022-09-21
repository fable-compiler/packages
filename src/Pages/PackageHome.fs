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
open Fable.Packages.Components.PackageHome.ReleaseNotes
open Fable.SimpleHttp
open Thoth.Json
open Fable.Core.JsInterop
open Browser.Dom
open Feliz.Lucide
open Fable.ZipJs
open Fable.SimpleXml
open FsToolkit.ErrorHandling
open FSharp.Control

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
    Versions: V3.CatalogRoot.CatalogPage.Package list
    Readme: string option
    ReleaseNotes: string option
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
    | Versions
    | ReleaseNotes
    | License

type Components with

    [<ReactComponent>]
    static member private Loading() =
        Bulma.hero [
            prop.className "is-fullheight-with-spaced-navbar"

            prop.children [
                Bulma.heroBody [
                    Bulma.container [
                        text.hasTextCentered
                        prop.style [
                            style.maxWidth (length.percent 50)
                        ]

                        prop.children [
                            Bulma.progress [
                                color.isPrimary
                            ]
                            Bulma.text.div [
                                prop.text
                                    "Loading package version information..."
                            ]
                        ]
                    ]
                ]
            ]
        ]

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
            | Tab.Versions -> Lucide.History, "Versions"
            | Tab.ReleaseNotes -> Lucide.BookOpen, "Release notes"
            | Tab.License -> Lucide.Copyright, "License"

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
                            Components.Tab(Tab.License, activeTab, setActiveTab)
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
                        info.Versions
                        info.Package.Version
                | Tab.Versions -> Components.Versions info.Package info.Versions
                | Tab.ReleaseNotes -> Components.ReleaseNotes info.ReleaseNotes
                | Tab.License -> Html.div "Components.License info.Package"
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

let private tryExtractReleaseNotes (entries: IEntry array) = promise {
    let nuspecEntry =
        entries
        |> Array.tryFind (fun entry ->
            entry.filename.ToLowerInvariant().EndsWith(".nuspec")
        )

    match nuspecEntry with
    | Some nuspecEntry ->
        let! content = nuspecEntry.getDataString (zip.createStringWriter ())
        // Wait for a fix from Fable.SimpleXml
        // See: https://github.com/Zaid-Ajaj/Fable.SimpleXml/issues/39
        let xmlDocument = SimpleXml.parseDocument content

        let releaseNotes =
            xmlDocument.Root
            |> SimpleXml.tryFindElementByName "metadata"
            |> Option.map (SimpleXml.tryFindElementByName "releaseNotes")
            |> Option.flatten
            |> Option.map (fun xmlElement -> xmlElement.Content)

        return releaseNotes
    | None -> return None
}

/// <summary>
/// Retrieve the README.md file content if present in the package
/// </summary>
/// <param name="entries"></param>
/// <typeparam name="'a"></typeparam>
/// <returns></returns>
let private tryExtractReadme (entries: IEntry array) = promise {
    // Naive way to retrieve the readme file
    // In the future, we should parse the nuspec file to retrieve the readme file
    let readmeEntry =
        entries
        |> Array.tryFind (fun entry ->
            entry.filename.ToLowerInvariant() = "readme.md"
        )

    match readmeEntry with
    | Some entry ->
        let! content = entry.getDataString (zip.createStringWriter ())
        return Some content

    | None -> return None
}

let private fetchPackageContent
    (packageId: string)
    (packageVersion: string)
    (versions: V3.CatalogRoot.CatalogPage.Package list)
    =
    async {
        let packageContent =
            versions
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

                let! entries = zipReader.getEntries () |> Async.AwaitPromise

                return Ok entries

            | _ -> return failwith "Unexpected response content type"
        | None ->
            return
                Error
                    $"Could not find package content URL for %s{packageId}@%s{packageVersion}"
    }

let private fetchVersions (url: string) = asyncResult {
    let! response =
        Http.request url
        |> Http.method GET
        |> Http.header (Headers.accept "application/json")
        |> Http.send

    let! catalogPage =
        Decode.fromString
            V3.CatalogRoot.CatalogPage.decoder
            response.responseText
        |> Result.mapError (fun errorMessage ->
            [
                "Error while decoding the search response JSON"
                ""
                errorMessage
            ]
            |> String.concat "\n"
        )

    let versions =
        catalogPage.Items
        |> Option.defaultValue []

    return versions
}

let private fetchAllVersions (pages: V3.CatalogRoot.CatalogPage list) = asyncSeq {
    for page in pages do
        let! versions = fetchVersions page.Id

        yield versions
}

let private fetchCatalogPage (catalogRootUrl: string) = asyncResult {
    let! response =
        Http.request catalogRootUrl
        |> Http.method GET
        |> Http.header (Headers.accept "application/json")
        |> Http.send

    let! catalogRoot =
        Decode.fromString V3.CatalogRoot.decoder response.responseText
        |> Result.mapError (fun errorMessage ->
            [
                "Error while decoding the search response JSON"
                ""
                errorMessage
            ]
            |> String.concat "\n"
        )

    // Should not happen because there are not elements it probably means
    // that the package doesn't exist
    if catalogRoot.Count = 0 then
        return! Error "No catalog page found"
    // If there is only one element, we can return it directly
    else if catalogRoot.Count = 1 then
        return catalogRoot.Items.Head.Items |> Option.defaultValue []
    else
        // If there are more than one element, we need to fetch each page
        // individually and merge them

        // Each page contains up to 64 elements
        let allVersions = ResizeArray(64 * catalogRoot.Items.Length)

        for batch in fetchAllVersions catalogRoot.Items do
            allVersions.AddRange batch

        return Seq.toList allVersions
}

let private fetchPackageInfo
    (packageId: string)
    (packageVersion: string option)
    : Async<Result<PackageFoundInfo, string>> =
    asyncResult {
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

        let! searchResponse =
            Decode.fromString V3.SearchResponse.decoder response.responseText
            |> Result.mapError (fun errorMessage ->
                [
                    "Error while decoding the search response JSON"
                    ""
                    errorMessage
                ]
                |> String.concat "\n"
            )

        let! registrationUrl =
            if searchResponse.TotalHits = 0 then
                Error "Package not found"
            else if searchResponse.TotalHits > 1 then
                Error "Multiple packages found"
            else
                match searchResponse.Data.Head.Registration with
                | Some registrationUrl -> Ok registrationUrl
                | None -> Error "Missing registration URL"

        let! allPackageVersions = fetchCatalogPage registrationUrl

        let requestedVersion =
            match packageVersion with
            | Some packageVersion -> packageVersion
            | None -> searchResponse.Data.Head.Version

        let! packageContent =
            fetchPackageContent packageId requestedVersion allPackageVersions

        let! readmeOpt = tryExtractReadme packageContent |> Async.AwaitPromise

        let! releaseNoteOpt =
            tryExtractReleaseNotes packageContent |> Async.AwaitPromise

        return {
            Package = searchResponse.Data.Head
            Versions = allPackageVersions
            Readme = readmeOpt
            ReleaseNotes = releaseNoteOpt
        }
    }

type PackageHomeProps =
    {| PackageId: string
       PackageVersion: string option |}

type Pages with

    [<ReactComponent>]
    static member PackageHome(props: PackageHomeProps) =
        let activeTab, setActiveTab = React.useState Tab.Readme

        let package =
            React.useDeferredNoCancel (
                fetchPackageInfo props.PackageId props.PackageVersion,
                [|
                    props
                |]
            )

        // When navigating to another version of the same package
        // react doesn't seems to reset the component completely
        // using useEffect is a workaround to reset the active tab
        React.useEffect (
            (fun () -> setActiveTab Tab.Readme),
            [|
                box props
            |]
        )

        match package with
        | Deferred.HasNotStartedYet -> Html.none
        | Deferred.InProgress -> Components.Loading()
        | Deferred.Failed reason -> Html.text reason.Message

        // | Deferred.Resolved DataStatus.PackageNotFound ->
        //     Components.PackageNotFound props.PackageId

        // | Deferred.Resolved DataStatus.MultiplePackagesFound ->
        //     Components.MultiplePackagesFound props.PackageId

        | Deferred.Resolved (Error error) -> Components.Errored error

        | Deferred.Resolved (Ok info) ->
            Html.div [
                Bulma.section [
                    Components.PageHeader info.Package props.PackageVersion
                    Components.TabsHeader(activeTab, setActiveTab)
                    Components.TabBody(activeTab, info)
                ]
            ]
