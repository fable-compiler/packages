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
open Fable.Packages.Components.NuGetPackageMedia
open Fable.SimpleHttp
open Thoth.Json

importSideEffects "./index.scss"

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

[<ReactComponent>]
let App () =
    let activeSearchOptions, setActiveSearchOptions =
        React.useStateWithUpdater SearchOptions.initial

    let elementsPerPage = 10
    let currentPageRank, setCurrentPageRank = React.useState 0

    let onSearch (searchOptions: SearchOptions) =
        if activeSearchOptions <> searchOptions then
            setActiveSearchOptions (fun _ -> searchOptions)
            setCurrentPageRank 0

    let fetchPackages = async {
        printfn "fetching packages"

        let queryParams =
            [
                "q=Tags%3A%22fable%22"
                $"take=%i{elementsPerPage}"
                $"skip=%i{currentPageRank * elementsPerPage}"
            ]
            |> String.concat "&"

        let requestUrl =
            $"https://azuresearch-usnc.nuget.org/query?%s{queryParams}"

        let! response =
            Http.request requestUrl
            |> Http.method GET
            |> Http.header (Headers.accept "application/json")
            |> Http.send

        return Decode.fromString NuGetResponse.decoder response.responseText
    }

    let packages =
        React.useDeferredNoCancel (
            fetchPackages,
            [|
                box activeSearchOptions
                box currentPageRank
            |]
        )

    React.useEffect (
        (fun () -> printfn "Searching for packages use effect"),
        [|
            box activeSearchOptions
            box currentPageRank
        |]
    )

    Html.div [
        Navbar()

        Bulma.container [
            prop.className "is-max-desktop"

            prop.children [
                Bulma.section [
                    SearchForm
                        {|
                            OnSearch =
                                (fun newSearchOptions ->
                                    setActiveSearchOptions (fun _ ->
                                        newSearchOptions
                                    )
                                )
                        |}
                ]

                match packages with
                | Deferred.Failed error -> Html.text "Failed to fetch packages"
                | Deferred.HasNotStartedYet -> null
                | Deferred.InProgress -> Html.text "Loading..."
                | Deferred.Resolved (Ok packages) ->
                    Html.div [
                        prop.className "packages-list"
                        packages.Data
                        |> List.map NuGetPackageMedia
                        |> prop.children

                    ]

                    Bulma.section [
                        Pagination
                            {|
                                CurrentPage = currentPageRank
                                TotalHits = packages.TotalHits
                                OnNavigate = setCurrentPageRank
                                ElementsPerPage = elementsPerPage
                            |}
                    ]
                | Deferred.Resolved (Error error) -> Html.text error
            ]
        ]
    ]
