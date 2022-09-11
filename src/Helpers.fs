module Helpers

open Fable.Core.JsInterop

module JS =

    let formatNumberToLocalString (num: float) : string =
        emitJsExpr num "$0.toLocaleString()"
