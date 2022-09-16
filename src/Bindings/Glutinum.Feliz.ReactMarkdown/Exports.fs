namespace Feliz.ReactMarkdown

open Feliz
open Fable.Core
open Fable.Core.JsInterop

[<Erase>]
type ReactMarkdown =

    static member inline ReactMarkdown (properties: #IReactMarkdownProperty seq) =
        Interop.reactApi.createElement(import "default" "react-markdown", createObj !!properties)
