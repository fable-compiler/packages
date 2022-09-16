module Fable.Packages.Components.PackageHome.PageHeader

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Fable.Packages.Components
open Fable.Packages.Components.NuGetPackageMedia
open Fable.Packages.Types
open Feliz.ReactMarkdown

let private remarkGfm: obj =
    import "default" "remark-gfm"

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
    static member private Description(description: string option) =
        let description =
            description |> Option.defaultValue ""

        Bulma.text.div [
            text.hasTextCentered
            spacing.mb4

            prop.children [
                ReactMarkdown.ReactMarkdown [
                    reactMarkdown.children description
                    reactMarkdown.remarkPlugins [|
                        remarkGfm
                    |]
                ]
            ]
        ]

    [<ReactComponent>]
    static member private Tags(tags : (string list) option) =
        match tags with
        | None -> null
        | Some tags ->
            Bulma.tags [
                helpers.isJustifyContentCenter

                prop.children [
                    for tag in tags do
                        Html.a [
                            prop.className "tag is-link is-primary is-light"
                            prop.onClick (fun _ ->
                                printfn "TODO: Implements search by tag"
                            )
                            prop.text tag
                        ]
                ]
            ]

    [<ReactComponent>]
    static member PageHeader(package: V3.SearchResponse.Package) =
        Bulma.text.div [
            spacing.mb5

            prop.children [
                Components.IconAndName package
                Components.Description package.Description
                Components.Tags package.Tags
            ]
        ]
