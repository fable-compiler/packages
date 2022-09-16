module Fable.Packages.Pages.NotFound

open Fable.Packages
open Fable.Packages.Components
open Fable.Packages.Components.AnErrorOccured
open Feliz

type Pages with

    [<ReactComponent>]
    static member NotFound() =
        Components.AnErrorOccured(
            "404 Page Not Found",
            "The page you are looking for does not exist."
        )
