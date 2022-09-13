module Helpers

open Fable.Core.JsInterop

module JS =

    let formatNumberToLocalString (num: float) : string =
        emitJsExpr num "$0.toLocaleString()"

let computateTotalPageCount (totalHits: int) (elementsPerPage: int) : int =
    float totalHits / float elementsPerPage
    |> System.Math.Ceiling
    |> int
