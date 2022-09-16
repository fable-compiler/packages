module Fable.Packages.Components.PackageHome.Readme

open Fable.Core.JsInterop
open Feliz
open Feliz.UseDeferred
open Feliz.Bulma
open Fable.Packages.Components
open Fable.Packages.Types
open Fable.DateFunctions
open Fable.SimpleHttp
open Fable.ZipJs
open Fable.Core

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

let private zip: Zip = importAll "@zip.js/zip.js/lib/zip-no-worker.js"
zip.configure (jsOptions<IConfiguration> (fun x -> x.useWebWorkers <- false))

// type FetchPackageResult

let private fetchPackageContent
    (nugetPackage: V3.SearchResponse.Package)
    (catalogPage: V3.CatalogRoot.CatalogPage)
    =
    async {

        match catalogPage.Items with
        | Some packages ->
            let currentPage =
                packages
                |> List.tryFind (fun package ->
                    package.CatalogEntry.Version = nugetPackage.Version
                )

            match currentPage with
            | Some package ->
                let! response =
                    Http.request package.PackageContent
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

                    // Dummy way to detect if the package has a readme file
                    // TODO: Parse the .nuspec file to get the readme file
                    let readmeEntry =
                        entries
                        |> Seq.tryFind (fun entry ->
                            entry.filename.ToLowerInvariant() = "readme.md"
                        )

                    match readmeEntry with
                    | Some readmeEntry ->
                        let! readmeContent =
                            readmeEntry.getDataString (zip.createStringWriter())
                            |> Async.AwaitPromise

                        return Ok None

                    | None ->
                        return Ok None

                | _ ->
                    return failwith "Unexpected response"

            | None ->
                return Error "Could not find detail about the requested version of the package"

        | None ->
            return Error "Missing catalog page items"
    }

type Components with

    [<ReactComponent>]
    static member Readme
        (nugetPackage: V3.SearchResponse.Package)
        (catalogPage: V3.CatalogRoot.CatalogPage)
        =

        let packageContent =
            React.useDeferredNoCancel (
                fetchPackageContent nugetPackage catalogPage,
                [|
                    box nugetPackage
                    box catalogPage
                |]
            )

        Html.div "readme dzhidzd"
