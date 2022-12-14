module Fable.Packages.Components.PackageHome.Versions

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Fable.Packages
open Fable.Packages.Components
open Fable.Packages.Types
open Fable.DateFunctions
open Feliz.Lucide

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

let private deprecationWarningToText (version : string) (reason : string) =
    match reason with
    | "Legacy" ->
        $"{version} is deprecated as it is legacy and is no longer maintained."

    | unkonwnReason ->
        $"{version} is deprecated, for the reason : {unkonwnReason}"


type Components with

    [<ReactComponent>]
    static member Versions
        (nugetPackage: V3.SearchResponse.Package)
        (versions: V3.CatalogRoot.CatalogPage.Package list)
        =
        let packages =
            versions
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
                            Html.th ""
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
                                    Html.td [
                                        if package.CatalogEntry.Deprecation.IsSome then
                                            React.fragment [
                                                for reason in package.CatalogEntry.Deprecation.Value.Reasons do
                                                    Html.div [
                                                        prop.custom("data-tooltip", deprecationWarningToText package.CatalogEntry.Version reason)
                                                        prop.className "has-tooltip-multiline has-tooltip-text-centered has-tooltip-left"
                                                        prop.children [
                                                            Lucide.AlertTriangle [ ]
                                                        ]
                                                    ]
                                            ]
                                    ]
                                ]

                            else
                                null

                    ]
                ]
            ]
        ]
