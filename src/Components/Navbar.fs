module Fable.Packages.Components.Navbar

open Feliz

// The Navbar is generated by copying the HTML from the main website
// and using https://thisfunctionaltom.github.io/Html2Feliz/ to convert the HTML to Feliz.
// Note: Some adaptations needs to be made for the SVG elements.

[<ReactComponent>]
let Navbar () =
    Html.nav [
        prop.classes [
            "navbar"
            "is-fixed-top"
            "is-spaced"
        ]
        prop.children [
            Html.div [
                prop.className "container"
                prop.children [
                    Html.div [
                        prop.className "navbar-brand"
                        prop.children [
                            Html.a [
                                prop.classes [
                                    "navbar-item"
                                    "title"
                                    "is-4"
                                ]
                                prop.href "https://fable.io/"
                                prop.text "Fable"
                            ]
                        ]
                    ]
                    Html.div [
                        prop.className "navbar-menu"
                        prop.children [
                            Html.div [
                                prop.className "navbar-start"
                                prop.children [
                                    Html.a [
                                        prop.className "navbar-item"
                                        prop.href "/docs"
                                        prop.text "Documentation"
                                    ]
                                    Html.a [
                                        prop.classes [
                                            "navbar-item"
                                            "is-hidden-mobile"
                                        ]
                                        prop.href "/repl/"
                                        prop.text "Try"
                                    ]
                                    Html.a [
                                        prop.className "navbar-item"
                                        prop.href "/blog/index.html"
                                        prop.text "Blog"
                                    ]
                                    Html.a [
                                        prop.classes [
                                            "navbar-item"
                                            "is-hidden-mobile"
                                        ]
                                        prop.href "/community.html"
                                        prop.text "Community"
                                    ]
                                    Html.a [
                                        prop.classes [
                                            "navbar-item"
                                            "is-hidden-mobile"
                                        ]
                                        prop.href "/resources.html"
                                        prop.text "Resources"
                                    ]
                                    Html.div [
                                        prop.classes [
                                            "navbar-item"
                                            "navbar-burger-dots"
                                            "is-hidden-tablet"
                                        ]
                                        prop.children [
                                            Svg.svg [
                                                svg.height 4
                                                svg.stroke "none"
                                                svg.viewBox (0, 0, 22, 4)
                                                svg.width 22
                                                svg.children [
                                                    Svg.circle [
                                                        svg.cx 2
                                                        svg.cy 2
                                                        svg.r 2
                                                    ]
                                                    Svg.circle [
                                                        svg.cx 2
                                                        svg.cy 2
                                                        svg.r 2
                                                        svg.transform.translate (
                                                            9,
                                                            0
                                                        )
                                                    ]
                                                    Svg.circle [
                                                        svg.cx 2
                                                        svg.cy 2
                                                        svg.r 2
                                                        svg.transform.translate (
                                                            18,
                                                            0
                                                        )
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                            Html.div [
                                prop.classes [
                                    "navbar-end"
                                    "is-hidden-mobile"
                                ]
                                prop.children [
                                    Html.a [
                                        prop.className "navbar-item"
                                        prop.href
                                            "https://github.com/fable-compiler/fable"
                                        prop.children [
                                            Html.span [
                                                prop.className "icon"
                                                prop.children [
                                                    Html.i [
                                                        prop.classes [
                                                            ""
                                                            "fab"
                                                            "fa-github"
                                                            "fa-lg"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                    Html.a [
                                        prop.className "navbar-item"
                                        prop.href
                                            "https://twitter.com/FableCompiler"
                                        prop.children [
                                            Html.span [
                                                prop.className "icon"
                                                prop.children [
                                                    Html.i [
                                                        prop.classes [
                                                            ""
                                                            "fab"
                                                            "fa-twitter"
                                                            "fa-lg"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                    Html.a [
                                        prop.className "navbar-item"
                                        prop.href
                                            "https://gitter.im/fable-compiler/Fable"
                                        prop.children [
                                            Html.span [
                                                prop.className "icon"
                                                prop.children [
                                                    Html.i [
                                                        prop.classes [
                                                            ""
                                                            "fab"
                                                            "fa-gitter"
                                                            "fa-lg"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                    Html.a [
                                        prop.className "navbar-item"
                                        prop.href
                                            "https://www.youtube.com/channel/UC6m70Jyr65ogDySbK7aMmzg/videos"
                                        prop.children [
                                            Html.span [
                                                prop.className "icon"
                                                prop.children [
                                                    Html.i [
                                                        prop.classes [
                                                            ""
                                                            "fab"
                                                            "fa-youtube"
                                                            "fa-lg"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Html.div [
                        prop.className "nacara-navbar-menu"
                        prop.children [
                            Html.a [
                                prop.className "nacara-navbar-menu-item"
                                prop.href "/repl/"
                                prop.text "Try"
                            ]
                            Html.a [
                                prop.className "nacara-navbar-menu-item"
                                prop.href "/community.html"
                                prop.text "Community"
                            ]
                            Html.a [
                                prop.className "nacara-navbar-menu-item"
                                prop.href "/resources.html"
                                prop.text "Resources"
                            ]
                            Html.a [
                                prop.className "nacara-navbar-menu-item"
                                prop.href
                                    "https://github.com/fable-compiler/fable"
                                prop.text "Github"
                            ]
                            Html.a [
                                prop.className "nacara-navbar-menu-item"
                                prop.href "https://twitter.com/FableCompiler"
                                prop.text "Twitter"
                            ]
                            Html.a [
                                prop.className "nacara-navbar-menu-item"
                                prop.href
                                    "https://gitter.im/fable-compiler/Fable"
                                prop.text "Gitter"
                            ]
                            Html.a [
                                prop.className "nacara-navbar-menu-item"
                                prop.href
                                    "https://www.youtube.com/channel/UC6m70Jyr65ogDySbK7aMmzg/videos"
                                prop.text "Youtube"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]