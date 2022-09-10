module Helpers

open Fable.Core.JsInterop

module JS =

    let formatNumberToLocalString (num: float) =
        emitJsExpr num "$0.toLocaleString()"
