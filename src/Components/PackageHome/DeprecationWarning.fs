module Fable.Packages.Components.PackageHome.DeprecationWarning

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

type Components with

    [<ReactComponent>]
    static member LegacyWarning (deprecation : V3.CatalogRoot.CatalogPage.Package.PackageDetails.Deprecation) =
        Bulma.message [
            color.isWarning
            prop.children [
                Bulma.messageBody [
                    Html.p [
                        Html.text "This package has been deprecated as it is "
                        Html.strong "legacy"
                        Html.text " and is no longer maintained. "
                    ]

                    match deprecation.AlternatePackage with
                    | Some alternatePackage ->
                        React.fragment [
                            Html.br  []
                            Html.p [
                                Html.strong "Suggested alternative: "
                            ]
                            Html.a [
                                prop.href $"/packages/#/package/{alternatePackage.Id}"
                                prop.text alternatePackage.Id
                            ]
                        ]
                    | None ->
                        null
                ]
            ]
        ]

    [<ReactComponent>]
    static member UnkownWarningReason (reason : string) =
        Bulma.message [
            color.isWarning
            prop.children [
                Bulma.messageBody [
                    Html.p "This packages has been deprecated"
                    Html.br  []
                    Html.p $"However, Fable.Packages doesn't support yet the reason : {reason}"
                    Html.text $"Please open an issue about it on "
                    Html.a [
                        prop.href "https://github.com/fable-compiler/packages"
                        prop.text "Fable.Packages"
                    ]
                    Html.text " so we can add support for it."
                ]
            ]
        ]

    [<ReactComponent>]
    static member DeprecationWarning
        (nugetPackage: V3.SearchResponse.Package)
        (versions: V3.CatalogRoot.CatalogPage.Package list) =

        let packageInfo =
            versions
            |> List.tryFind (fun package ->
                package.CatalogEntry.Version = nugetPackage.Version
            )

        match packageInfo with
        | Some packageInfo ->
            match packageInfo.CatalogEntry.Deprecation with
            | Some deprecationInfo ->

                React.fragment [
                    for reason in deprecationInfo.Reasons do
                        match reason with
                        | "Legacy" ->
                            Components.LegacyWarning deprecationInfo

                        | unkonwnReason ->
                            Components.UnkownWarningReason unkonwnReason

                ]


            | None ->
                null

        | None ->
            Html.text
                "Could not find the requested version in the catalog entry"
