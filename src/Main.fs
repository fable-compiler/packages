module Fable.Packages.Main

open Feliz
open Browser.Dom
open Browser

ReactDOM.render (
    App.App(),
    document.querySelector ("#root") :?> Types.HTMLElement
)
