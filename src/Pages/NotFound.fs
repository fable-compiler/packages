module Fable.Packages.Pages.NotFound

open Fable.Packages
open Feliz
open Feliz.Bulma

type Pages with

    [<ReactComponent>]
    static member NotFound () =
        Bulma.hero [
            prop.className "is-fullheight-with-spaced-navbar"

            prop.children [
                Bulma.heroBody [
                    Bulma.container [
                        text.hasTextCentered

                        prop.children [
                            Bulma.title "404 Page Not Found"
                            Bulma.subtitle "The page you are looking for does not exist."
                            Bulma.buttons [
                                buttons.isCentered

                                prop.children [
                                    Bulma.button.a [
                                        color.isPrimary
                                        color.isLight
                                        prop.text "Return to Home"
                                        prop.href (Router.toUrl Router.Page.Search)
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
