namespace Feliz.UseDeferred

open Feliz

[<AutoOpen>]
module ReactHookExtensions =
    type React with

        [<Hook>]
        static member useDeferredNoCancel
            (
                operation: Async<'T>,
                dependencies: obj array
            ) =
            let (deferred, setDeferred) =
                React.useState (Deferred.HasNotStartedYet)

            let executeOperation () = async {
                try
                    do setDeferred (Deferred<'T>.InProgress)
                    let! output = operation
                    do setDeferred (Deferred<'T>.Resolved output)
                with error ->
#if DEBUG
                    Browser.Dom.console.log (error)
#endif
                    do setDeferred (Deferred<'T>.Failed error)
            }

            React.useEffect (
                (fun () -> Async.StartImmediate(executeOperation ())),
                dependencies
            )

            deferred
