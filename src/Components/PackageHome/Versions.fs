module Fable.Packages.Components.PackageHome.Versions

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Fable.Packages
open Fable.Packages.Components
open Fable.Packages.Types
open Fable.DateFunctions

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type Components with

    [<ReactComponent>]
    static member Versions
        (nugetPackage: V3.SearchResponse.Package)
        (catalogPage: V3.CatalogRoot.CatalogPage)
        =
        match catalogPage.Items with
        | Some packages ->
            let packages =
                packages
                |> List.sortByDescending (fun package ->
                    package.CatalogEntry.Published
                )

            Bulma.tableContainer [
                Bulma.table [
                    table.isFullWidth

                    prop.children [
                        Html.thead [
                            Html.tr [
                                Html.th "Version"
                                Html.th "Downloads"
                                Html.th "Last updated"
                            ]
                        ]

                        Html.tbody [
                            for package in packages do
                                let isListed =
                                    match package.CatalogEntry.Listed with
                                    | Some true
                                    | None -> true
                                    | Some false -> false

                                if isListed then

                                    let publishedText =
                                        package.CatalogEntry.Published
                                        |> Option.map (fun date ->
                                            date.FormatDistance(
                                                System.DateTime.UtcNow
                                            )
                                            |> Html.text
                                        )
                                        |> Option.defaultValue null

                                    let downloadCount =
                                        nugetPackage.Versions
                                        |> List.tryFind (fun version ->
                                            version.Version = package.CatalogEntry.Version
                                        )
                                        |> Option.map (fun version ->
                                            Html.text version.Downloads
                                        )
                                        |> Option.defaultValue null

                                    let packageUrl =
                                        ({
                                            PackageId = nugetPackage.Id
                                            Version = Some package.CatalogEntry.Version
                                        } : Router.PackageParameters)
                                        |> Router.Page.Package
                                        |> Router.toUrl

                                    Html.tr [
                                        Html.td [
                                            Html.a [
                                                prop.href packageUrl
                                                prop.children [
                                                    Html.text
                                                        package.CatalogEntry.Version
                                                ]
                                            ]
                                        ]
                                        Html.td [
                                            downloadCount
                                        ]
                                        Html.td [
                                            publishedText
                                        ]
                                    ]

                                else
                                    null

                        ]
                    ]
                ]
            ]

        | None -> Html.text "Missing packages information in the catalog page"
