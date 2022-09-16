module Fable.Packages.Contexts.NuGetRegistration5Semver1Cache

open Feliz
open Fable.Core.JsInterop
open Fable.Packages.Types

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type NuGetRegistration5Semver1CacheContextType = {
    TryFindById: string -> NuGetRegistration5Semver1 option
    AddOrUpdate: NuGetRegistration5Semver1 -> unit
    AddOrUpdateMany: NuGetRegistration5Semver1 list -> unit
}

let NuGetRegistration5Semver1CacheContext =
    React.createContext<NuGetRegistration5Semver1CacheContextType> (
        "NuGetRegistration5Semver1CacheContextType",
        Unchecked.defaultof<_>
    )

[<Hook>]
let useNuGetRegistration5Semver1Cache () =
    React.useContext NuGetRegistration5Semver1CacheContext

[<ReactComponent>]
let NuGetRegistration5Semver1CacheProvider (children: ReactElement) =
    let cache, setCache =
        React.useStateWithUpdater (Map.empty<string, NuGetRegistration5Semver1>)

    let service = {
        TryFindById = fun id -> Map.tryFind id cache
        AddOrUpdate =
            fun data -> setCache (fun oldCache -> Map.add data.Id data oldCache)
        AddOrUpdateMany =
            fun data ->
                setCache (fun oldCache ->
                    data
                    |> List.fold
                        (fun acc data -> Map.add data.Id data acc)
                        oldCache
                )
    }

    React.contextProvider (
        NuGetRegistration5Semver1CacheContext,
        service,
        children
    )
