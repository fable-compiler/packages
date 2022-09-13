namespace Fable.Packages

open Feliz.Router

[<RequireQualifiedAccess>]
module Router =

    [<RequireQualifiedAccess>]
    type Page =
        | Search
        | Package of string
        | NotFound

    let parseUrl =
        function
        | []
        | [ "search" ] -> Page.Search
        | [ "package"; packageName ] -> Page.Package packageName
        | _ -> Page.NotFound

    let toUrl (page: Page) =
        match page with
        | Page.Search -> Router.format ("search")
        | Page.Package packageName -> Router.format ("package", packageName)
        | Page.NotFound -> Router.format ("not-found")
