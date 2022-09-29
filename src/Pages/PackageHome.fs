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
open Fable.Packages.Components.PackageHome.License
open Fable.Packages.Components.PackageHome.InstallationInstructions
open Fable.Packages.Components.PackageHome.Metadata
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
    License: LicenseType option
    Nuspec: string
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

[<RequireQualifiedAccess>]
type private LicenseInfo =
    | File of string
    | Expression of string
    | Unkown of string

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
    static member private TabBody
        (
            activeTab: Tab,
            info: PackageFoundInfo,
            displayedVersion: string
        ) =
        Html.div [
            prop.className "tab-body box"
            prop.children [
                // Left section - Dynamic content
                match activeTab with
                | Tab.Readme -> Components.Readme info.Readme
                | Tab.Dependencies ->
                    Components.Dependencies
                        info.Package
                        info.Versions
                        info.Package.Version
                | Tab.Versions -> Components.Versions info.Package info.Versions
                | Tab.ReleaseNotes -> Components.ReleaseNotes info.ReleaseNotes
                | Tab.License -> Components.License info.License

                // Right section - Static content (metadata)
                Components.Metadata(info.Package, displayedVersion, info.Nuspec)
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

let private tryExtractReleaseNotes (nuspecContent: string) = promise {
    let xmlDocument = SimpleXml.parseDocument nuspecContent

    let releaseNotes =
        xmlDocument.Root
        |> SimpleXml.tryFindElementByName "metadata"
        |> Option.map (SimpleXml.tryFindElementByName "releaseNotes")
        |> Option.flatten
        |> Option.map (fun xmlElement -> xmlElement.Content)

    return releaseNotes
}

let private retrieveLicenseFile (entries: IEntry array) (licensePath: string) = asyncResult {
    let entry =
        entries
        |> Array.tryFind (fun entry ->
            entry.filename.ToLowerInvariant() = licensePath.ToLowerInvariant()
        )

    match entry with
    | None ->
        return!
            Error
                $"Could not find the license file in the package. License path: '%s{licensePath}'"

    | Some entry ->
        let! content =
            entry.getDataString (zip.createStringWriter ())
            |> Async.AwaitPromise

        return content
}

let private extractNuspecContent (entries: IEntry array) = asyncResult {
    let nuspecEntry =
        entries
        |> Array.tryFind (fun entry ->
            entry.filename.ToLowerInvariant().EndsWith(".nuspec")
        )

    match nuspecEntry with
    | Some nuspecEntry ->
        let! content =
            nuspecEntry.getDataString (zip.createStringWriter ())
            |> Async.AwaitPromise

        return content

    | None -> return! Error "Could not find a nuspec file in the package"
}

let private tryExtractLicense (nuspecContent: string) (entries: IEntry array) = asyncResult {

    let xmlDocument = SimpleXml.parseDocument nuspecContent

    let licenseInfo = option {
        let! licenseElement =
            xmlDocument.Root
            |> SimpleXml.tryFindElementByName "metadata"
            |> Option.map (SimpleXml.tryFindElementByName "license")
            |> Option.flatten

        let! licenseTypeAttr = Map.tryFind "type" licenseElement.Attributes

        if licenseTypeAttr = "file" then
            return LicenseInfo.File licenseElement.Content
        else if licenseTypeAttr = "expression" then
            return LicenseInfo.Expression licenseElement.Content
        else
            return LicenseInfo.Unkown licenseTypeAttr
    }

    match licenseInfo with
    | Some (LicenseInfo.Expression expression) ->
        return Some(LicenseType.Expression expression)

    | Some (LicenseInfo.File file) ->
        let! licenseText = retrieveLicenseFile entries file

        return Some(LicenseType.File licenseText)

    | Some (LicenseInfo.Unkown unkownType) ->
        return! Error $"Unkown license type '%s{unkownType}'"
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
        Decode.fromString V3.CatalogRoot.CatalogPage.decoder response.responseText
            |> Result.mapError (fun errorMessage ->
                [
                    "Error while decoding the search response JSON"
                    "Context: Fetching package versions - catalog page"
                    $"Url: %s{url}"
                    ""
                    errorMessage
                ]
                |> String.concat "\n"
            )

    match catalogPage.Items with
    | Some items ->
        return items
    | None ->
        return!
            Error
                $"Could not find any package version for the package. Url: '%s{url}'"
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
                "Context: Fetching catalog page"
                $"Url: %s{catalogRootUrl}"
                ""
                errorMessage
            ]
            |> String.concat "\n"
        )

    // Each page contains up to 64 elements
    let acc = ResizeArray(64 * catalogRoot.Items.Length)

    for item in catalogRoot.Items do
        match item.Items with
        | Some packages ->
            acc.AddRange packages
        // The response doesn't contains the package information
        // Try to retrieve it via another call
        | None ->
            let! version = fetchVersions item.Id
            acc.AddRange version

    return Seq.toList acc
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
                    "Context: Fetching package info"
                    $"Url: %s{requestUrl}"
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

        let! nuspecContent = extractNuspecContent packageContent

        let! licenseOpt = tryExtractLicense nuspecContent packageContent

        let! releaseNoteOpt =
            tryExtractReleaseNotes nuspecContent |> Async.AwaitPromise

        return {
            Package = searchResponse.Data.Head
            Versions = allPackageVersions
            Readme = readmeOpt
            ReleaseNotes = releaseNoteOpt
            License = licenseOpt
            Nuspec = nuspecContent
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

        | Deferred.Resolved (Error error) -> Components.Errored error

        | Deferred.Resolved (Ok info) ->
            // If the user didn't request a specific version, we show the latest version
            // Otherwise, we show the requested version
            let displayedVersion =
                props.PackageVersion |> Option.defaultValue info.Package.Version

            Html.div [
                Bulma.section [
                    Components.PageHeader info.Package displayedVersion
                    Components.InstallationInstructions(
                        info.Package.Id,
                        displayedVersion
                    )
                    Html.hr []
                    Components.TabsHeader(activeTab, setActiveTab)
                    Components.TabBody(activeTab, info, displayedVersion)
                ]
            ]
