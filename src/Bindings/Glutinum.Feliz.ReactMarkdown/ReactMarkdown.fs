namespace Feliz.ReactMarkdown

open Feliz
open Fable.Core

[<Erase>]
type reactMarkdown =

    static member inline children (value : string) =
        Interop.mkReactMarkdownProperty "children" value

    static member inline remarkPlugins (value : obj array) =
        Interop.mkReactMarkdownProperty "remarkPlugins" value

    static member inline rehypePlugins (value : obj array) =
        Interop.mkReactMarkdownProperty "rehypePlugins" value

    static member remarkRehypeOptions (value : obj) =
        Interop.mkReactMarkdownProperty "remarkRehypeOptions" value

    static member className (value : obj) =
        Interop.mkReactMarkdownProperty "className" value
