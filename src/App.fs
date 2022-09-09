module App

open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Fable.Packages.Types
open Fable.Packages.Components.Navbar
open Fable.Packages.Components.SearchForm
open Fable.Packages.Components.Pagination
open Fable.SimpleHttp
open Thoth.Json

importSideEffects "./index.scss"

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

[<ReactComponent>]
let App () =
    let activeSearchOptions, setActiveSearchOptions =
        React.useState SearchOptions.initial

    let currentPageRank, setCurrentPageRank = React.useState 0

    let fetchPackages = async {
        let! response =
            Http.request
                "https://azuresearch-usnc.nuget.org/query?q=Tags%3A%22fable%22"
            |> Http.method GET
            |> Http.header (Headers.accept "application/json")
            |> Http.send

        return Decode.fromString NuGetResponse.decoder response.responseText
    }

    let packages =
        React.useDeferred (
            fetchPackages,
            [|
                box activeSearchOptions
                box currentPageRank
            |]
        )

    Html.div [
        Navbar()

        Bulma.container [
            Bulma.section [
                SearchForm
                    {|
                        OnSearch = setActiveSearchOptions
                    |}
            ]

            match packages with
            | Deferred.Failed error -> Html.text "Failed to fetch packages"
            | Deferred.HasNotStartedYet -> null
            | Deferred.InProgress -> Html.text "Loading..."
            | Deferred.Resolved (Ok packages) ->
                Pagination
                    {|
                        CurrentPage = currentPageRank
                        TotalHits = packages.TotalHits
                        OnNavigate = setCurrentPageRank
                    |}
            | Deferred.Resolved (Error error) -> Html.text error
        ]
    ]
