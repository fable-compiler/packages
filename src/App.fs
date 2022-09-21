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
        | Router.Page.Search -> Pages.Search()

        | Router.Page.Package parameters ->
            Pages.PackageHome
                {|
                    PackageId = parameters.PackageId
                    PackageVersion = parameters.Version
                |}

        | Router.Page.NotFound -> Pages.NotFound()

    Html.div [
        Components.Navbar()

        Bulma.container [
            prop.className "is-max-desktop"

            prop.children [
                React.router [
                    router.onUrlChanged (Router.parseUrl >> setPage)
                    router.children pageElement
                ]
            ]
        ]
    ]
