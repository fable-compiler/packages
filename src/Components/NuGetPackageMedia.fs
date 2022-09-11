module Fable.Packages.Components.NuGetPackageMedia

open Fable.Core.JS
open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Fable.Packages.Types
open Fable.SimpleHttp
open Thoth.Json
open Fable.DateFunctions

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

[<ReactComponent>]
let NuGetPackageMediaTags (tags: (string list) option) =
    match tags with
    | Some tags ->
        Bulma.tags [
            spacing.mt3
            prop.children [
                for tag in tags do
                    Html.a [
                        prop.className "tag is-link is-primary is-light"
                        prop.onClick (fun _ ->
                            printfn "TODO: Implements search by tag"
                        )
                        prop.text tag
                    ]
            ]
        ]

    | None -> null

[<ReactComponent>]
let NuGetPackageIcon (iconUrl: string option) =
    let handledIconImgError, setHandledIconImgError = React.useState false

    let iconUrl =
        iconUrl
        |> Option.defaultValue
            "https://www.nuget.org/Content/gallery/img/default-package-icon.svg"

    Bulma.image [
        image.is64x64
        prop.children [
            Html.img [
                prop.src iconUrl
                // If the icon is not found, use the default one
                // Do it only once, to avoid infinite loop
                if not handledIconImgError then
                    prop.onError (fun (ev: Browser.Types.Event) ->
                        let element =
                            ev.target :?> Browser.Types.HTMLImageElement

                        element.src <-
                            "https://www.nuget.org/Content/gallery/img/default-package-icon.svg"

                        setHandledIconImgError true
                    )
            ]
        ]
    ]

[<ReactComponent>]
let NuGetPackageSummary (package: NuGetPackage) =
    // Compute the NuGet package URL
    // There doesn't seems to be a field containing this information
    let nugetPackageUrl = $"https://www.nuget.org/packages/%s{package.Id}"

    Html.div [
        prop.className "nuget-package-summary"

        prop.children [
            Html.p [
                prop.className "title is-4"
                prop.children [
                    Html.a [
                        prop.href nugetPackageUrl
                        prop.text package.Id
                    ]
                    Html.span [
                        spacing.ml2
                        prop.text $"v%s{package.Version}"
                    ]
                ]
            ]

            match package.Owners with
            | Some owners ->
                Html.p [
                    // title.is6
                    prop.className "subtitle is-6"
                    prop.children [
                        Html.span [
                            prop.text "By: "
                        ]
                        for owner in owners do
                            Html.a [
                                prop.href nugetPackageUrl
                                prop.text owner
                                prop.onClick (fun _ ->
                                    printfn "TODO: Implements search by owner"
                                )
                            ]

                            Html.span " "
                    ]
                ]
            | _ -> null

            match package.Description with
            | Some "Package Description"
            | None ->
                Bulma.text.div [
                    color.hasTextGrey
                    text.isItalic
                    prop.text "Package has no description"
                ]

            | Some description ->
                Html.p [
                    prop.text description
                ]
        ]
    ]

[<ReactComponent>]
let StatisticsRow (icon: string) (text: string) =
    Html.span [
        prop.className "icon-text"
        prop.children [
            Bulma.icon [
                color.hasTextGrey
                prop.children [
                    Html.i [
                        prop.className icon
                    ]
                ]
            ]
            Bulma.text.span [
                color.hasTextGrey
                prop.text text
            ]
        ]
    ]

[<ReactComponent>]
let NuGetPackageStatistics (package: NuGetPackage) =

    let fetchRegistrationInfo = async {
        let packageRegistrationUrl =
            package.Versions
            |> List.tryFind (fun version -> version.Version = package.Version)
            |> Option.map (fun versionInfo -> versionInfo.Id)

        match packageRegistrationUrl with
        | Some packageRegistrationUrl ->

            let! response =
                Http.request packageRegistrationUrl
                |> Http.method GET
                |> Http.header (Headers.accept "application/json")
                |> Http.send

            match
                Decode.fromString
                    NuGetRegistration5Semver1.decoder
                    response.responseText
            with
            | Ok value -> return Ok value
            | Error error ->
                console.error error
                return Error error

        | None ->
            let msg = "Could not find package registration URL"
            console.error msg
            return Error msg
    }

    let registrationInfo = React.useDeferred (fetchRegistrationInfo, [||])

    Html.div [
        prop.className "nuget-package-statistics"

        prop.children [

            match package.TotalDownloads with
            | Some totalDownloads ->
                StatisticsRow
                    "fas fa-download"
                    (Helpers.JS.formatNumberToLocalString totalDownloads)
            | None -> null

            match registrationInfo with
            | Deferred.Resolved (Ok registrationInfo) ->
                let timePastSinceLastUpdate =
                    registrationInfo.Published.FormatDistance(
                        System.DateTime.UtcNow
                    )

                StatisticsRow "fas fa-history" timePastSinceLastUpdate

            | Deferred.Failed _
            | Deferred.HasNotStartedYet
            | Deferred.InProgress
            | Deferred.Resolved (Error _) -> null
        ]
    ]

[<ReactComponent>]
let NuGetPackageMedia (package: NuGetPackage) =

    Bulma.media [
        Bulma.mediaLeft [
            NuGetPackageIcon package.IconUrl
        ]

        Bulma.mediaContent [
            Html.div [
                prop.className "nuget-package-body"
                prop.children [
                    NuGetPackageSummary package
                    NuGetPackageStatistics package
                ]
            ]
            NuGetPackageMediaTags package.Tags
        ]
    ]
