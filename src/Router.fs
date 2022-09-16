namespace Fable.Packages

open Feliz.Router

[<RequireQualifiedAccess>]
module Router =

    type PackageParameters = {
        PackageId: string
        Version: string option
    }

    [<RequireQualifiedAccess>]
    type Page =
        | Search
        | Package of PackageParameters
        | NotFound

    let parseUrl =
        function
        | []
        | [ "search" ] -> Page.Search
        | [ "package"; packageId ] ->
            Page.Package
                {
                    PackageId = packageId
                    Version = None
                }
        | [ "package"; packageId; version ] ->
            Page.Package
                {
                    PackageId = packageId
                    Version = Some version
                }
        | _ -> Page.NotFound

    let toUrl (page: Page) =
        match page with
        | Page.Search -> Router.format ("search")
        | Page.Package parameters ->
            match parameters.Version with
            | Some version ->
                Router.format ("package", parameters.PackageId, version)
            | None -> Router.format ("package", parameters.PackageId)
        | Page.NotFound -> Router.format ("not-found")
