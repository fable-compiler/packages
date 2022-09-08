module App

open Feliz
open Fable.Core.JsInterop
open Browser

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

[<ReactComponent>]
let App () =
    let uptime, setUptime = React.useStateWithUpdater 0

    React.useEffect(fun () ->
        let timerId =
            window.setInterval(fun () ->
                setUptime (fun uptime -> uptime + 1)
            , 1000)

        { new System.IDisposable with
            member __.Dispose() =
                window.clearInterval timerId
        }

    , [||])

    Html.div [
        Html.text $"Application is running since %i{uptime} seconds"
        Html.br [ ]
        Html.text "Change this text and check that the timer has not been reset"
    ]
