module Fable.Packages.Components.PackageHome.License

open Fable.Core.JsInterop
open Feliz
open Feliz.UseDeferred
open Feliz.Bulma
open Fable.Packages.Components
open Fable.Packages.Types
open Feliz.ReactMarkdown
open Browser.Dom

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type Components with

    [<ReactComponent>]
    static member License(licenseOpt: LicenseType option) =

        match licenseOpt with
        | Some (LicenseType.File content) ->
            Bulma.content [
                prop.style [
                    style.whitespace.preline
                ]
                prop.text content
            ]

        | Some (LicenseType.Expression expression) ->
            // We cannot display the license content because Microsoft doesn't support external requests on this domain

            let url = $"https://licenses.nuget.org/%s{expression}"
            let encodedUrl = window.encodeURI url

            Bulma.content [
                Html.p [
                    Html.span "Package license is defined by the following expression: "
                    Bulma.text.span [
                        text.hasTextWeightSemibold
                        prop.text expression
                    ]
                ]

                Html.p [
                    Html.a [
                        prop.href encodedUrl
                        prop.text "Visit this link to learn more about the license"
                    ]
                ]
            ]

        | None ->
            Bulma.text.div [
                color.hasTextGrey
                prop.text "Package doesn't a license"
            ]
