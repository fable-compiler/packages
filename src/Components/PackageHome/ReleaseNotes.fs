module Fable.Packages.Components.PackageHome.ReleaseNotes

open Fable.Core.JsInterop
open Feliz
open Feliz.UseDeferred
open Feliz.Bulma
open Fable.Packages.Components
open Fable.Packages.Types
open Feliz.ReactMarkdown

let private remarkGfm: obj =
    import "default" "remark-gfm"

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

[<ReactComponent(import="default", from="../MarkdownContent.jsx")>]
let MarkdownContent(content : string) =
    React.imported()

type Components with

    [<ReactComponent>]
    static member ReleaseNotes
        (content : string option)
        =

        match content with
        | Some content ->
            Bulma.content [
                MarkdownContent content
            ]

        | None ->
            Bulma.text.div [
                color.hasTextGrey
                prop.text "Package doesn't have a release notes"
            ]
