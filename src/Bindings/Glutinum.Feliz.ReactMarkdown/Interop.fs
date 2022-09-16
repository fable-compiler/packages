namespace Feliz.ReactMarkdown

module Interop =

    let mkReactMarkdownProperty (key: string) (value: obj) : IReactMarkdownProperty = unbox (key, value)

