module Fable.Packages.Components.PackageHome.Metadata

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Fable.Packages
open Fable.Packages.Components
open Fable.Packages.Types
open Feliz.Lucide
open Fable.SimpleXml
open FsToolkit.ErrorHandling

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type Components with

    [<ReactComponent>]
    static member private MetadataSectionTitle
        (title: string)
        =
        Html.div [
            prop.className "metadata-section__title"
            prop.text title
        ]

    [<ReactComponent>]
    static member private MetadataSectionLink
        (iconElement: list<ISvgAttribute> -> ReactElement)
        (label: string)
        (url: string)
        =
        Html.div [
            prop.className "metadata-section__link"
            prop.children [
                Bulma.icon [
                    iconElement [
                        lucide.size 32
                    ]
                ]
                Html.a [
                    prop.className "metadata-item"
                    prop.text label
                    prop.href url
                ]
            ]
        ]

    [<ReactComponent>]
    static member private ProjectUrl(projectUrlOpt: string option) =
        match projectUrlOpt with
        | Some projectUrl ->
            Components.MetadataSectionLink
                Lucide.Globe
                "Project website"
                projectUrl

        | None -> null

    [<ReactComponent>]
    static member private Repository(nuspec : string) =
        let xmlDocument = SimpleXml.parseDocument nuspec

        let repositoryUrlOpt =
            option {
                let! metadataElement =
                    SimpleXml.tryFindElementByName "metadata" xmlDocument.Root

                let! repositoryElement =
                    SimpleXml.tryFindElementByName "repository" metadataElement

                return! Map.tryFind "url" repositoryElement.Attributes
            }

        match repositoryUrlOpt with
        | Some repositoryUrl ->
            Components.MetadataSectionLink
                Lucide.GitBranch
                "Repository"
                repositoryUrl

        | None -> null

    [<ReactComponent>]
    static member private Owners(owners : (string list) option) =
        match owners with
        | Some owners ->

            React.fragment [
                Components.MetadataSectionTitle "Owners"

                Html.ul [
                    for owner in owners do
                        let url =
                            {| Router.SearchParameters.initial with
                                Query = $"owner:%s{owner}"
                            |}
                            |> Some
                            |> Router.Page.Search
                            |> Router.toUrl

                        Html.li [
                            prop.className "metadata-item__author"
                            prop.children [
                                Bulma.image [
                                    image.is48x48
                                    prop.children [
                                        Html.img [
                                            prop.src $"https://www.nuget.org/profiles/{owner}/avatar?imageSize=48"
                                        ]
                                    ]
                                ]
                                Html.a [
                                    prop.href url
                                    prop.text owner
                                ]
                            ]
                        ]
                ]
            ]

        | None -> null


    [<ReactComponent>]
    static member Metadata
        (
            package: V3.SearchResponse.Package,
            displayedVersion: string,
            nuspec : string
        ) =
        Html.div [
            prop.className "metadata-section"
            prop.children [
                Html.div [
                    prop.className "metadata-section-about"
                    prop.children [
                        Components.MetadataSectionTitle "About"

                        Html.div [
                            prop.className "metadata-section-about-links"
                            prop.children [
                                Components.MetadataSectionLink
                                    Lucide.ExternalLink
                                    "See on NuGet"
                                    $"https://www.nuget.org/packages/%s{package.Id}/%s{displayedVersion}"
                                Components.ProjectUrl package.ProjectUrl
                                Components.Repository nuspec
                            ]
                        ]

                    ]
                ]

                Components.Owners package.Owners
            ]
        ]
