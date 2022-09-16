module Fable.Packages.Components.PackageHome.PageHeader

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Fable.Packages.Components
open Fable.Packages.Components.NuGetPackageMedia
open Fable.Packages.Types

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type Components with

    [<ReactComponent>]
    static member IconAndName(package: V3.SearchResponse.Package) =
        Bulma.text.div [
            prop.className "title is-4"
            helpers.isFlex
            helpers.isAlignItemsCenter
            helpers.isJustifyContentCenter
            helpers.isFlexWrapWrap
            helpers.isFlexDirectionRow

            prop.children [
                Components.NuGetPackageIcon package.IconUrl

                Bulma.text.span [
                    spacing.ml4
                    text.hasTextWeightMedium
                    prop.text package.Id
                ]

                Bulma.text.span [
                    color.hasTextGrey
                    text.hasTextWeightLight
                    spacing.ml4
                    prop.text $"v%s{package.Version}"
                ]
            ]
        ]

    [<ReactComponent>]
    static member PageHeader(package: V3.SearchResponse.Package) =

        Bulma.text.div [
            spacing.mb5

            prop.children [
                Components.IconAndName package

                Html.div [
                    package.Description
                    |> Option.defaultValue ""
                    |> prop.text
                ]

            ]
        ]
