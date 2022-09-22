module Fable.Packages.Components.AnErrorOccured

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Fable.Packages

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type Components with

    [<ReactComponent>]
    static member AnErrorOccured
        (
            title: string,
            subtitle: string,
            details: string
        ) =
        Bulma.section [

            prop.children [
                Bulma.heroBody [
                    Bulma.container [
                        text.hasTextCentered

                        prop.children [
                            Bulma.title title
                            Bulma.subtitle subtitle

                            Bulma.text.p [
                                prop.text details
                            ]

                            Bulma.buttons [
                                buttons.isCentered

                                prop.children [
                                    Bulma.button.a [
                                        color.isPrimary
                                        color.isLight
                                        prop.text "Return to Home"
                                        prop.href (
                                            Router.toUrl (Router.Page.Search None)
                                        )
                                    ]
                                    Bulma.button.a [
                                        color.isDanger
                                        color.isLight
                                        prop.text "Report a problem"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member AnErrorOccured(title: string, subtitle: string) =
        Bulma.hero [
            prop.className "is-fullheight-with-spaced-navbar"

            prop.children [
                Bulma.heroBody [
                    Bulma.container [
                        text.hasTextCentered

                        prop.children [
                            Bulma.title title
                            Bulma.subtitle subtitle
                            Bulma.buttons [
                                buttons.isCentered

                                prop.children [
                                    Bulma.button.a [
                                        color.isPrimary
                                        color.isLight
                                        prop.text "Return to Home"
                                        prop.href (
                                            Router.toUrl (Router.Page.Search None)
                                        )
                                    ]
                                    Bulma.button.a [
                                        color.isDanger
                                        color.isLight
                                        prop.text "Report a problem"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
