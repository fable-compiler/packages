module Fable.Packages.App

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Feliz.Router
open Fable.Packages.Components
open Fable.Packages.Components.Navbar
open Fable.Packages.Pages
open Fable.Packages.Pages.Search
open Fable.Packages.Pages.NotFound
open Fable.Packages.Pages.PackageHome
open Fable.Packages.Contexts.NuGetRegistration5Semver1Cache

importSideEffects "./index.scss"

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

[<ReactComponent>]
let App () =
    let (page, setPage) =
        React.useState (Router.parseUrl (Router.currentUrl ()))

    let pageElement =
        match page with
        | Router.Page.Search parameters ->
            Pages.Search parameters

        | Router.Page.Package parameters ->
            Pages.PackageHome
                {|
                    PackageId = parameters.PackageId
                    PackageVersion = parameters.Version
                |}

        | Router.Page.NotFound -> Pages.NotFound()

    NuGetRegistration5Semver1CacheProvider (
        React.fragment [
            Components.Navbar()

            Bulma.container [
                prop.className "is-max-desktop main-body"

                prop.children [
                    React.router [
                        router.onUrlChanged (Router.parseUrl >> setPage)
                        router.children pageElement
                    ]
                ]
            ]

            Bulma.footer [
                Bulma.text.div [
                    size.isSize5
                    prop.className "has-text-centered content"
                    prop.children [
                        Html.p "Fable.Packages is in prototype phase"
                        Html.p "Future versions will aim to improve performances"
                        Html.text "Please report any issues on "
                        Html.a [
                            prop.className "has-text-white is-underlined"
                            prop.href "https://github.com/fable-compiler/packages/issues"
                            prop.text "GitHub"
                        ]
                    ]
                ]
            ]
        ]
    )
