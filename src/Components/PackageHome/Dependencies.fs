module Fable.Packages.Components.PackageHome.Dependencies

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Fable.Packages.Components
open Fable.Packages.Types
open Fable.DateFunctions

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type PackageDependencyGroup =
    V3.CatalogRoot.CatalogPage.Package.PackageDetails.PackageDependencyGroup

type PackageDependency =
    V3.CatalogRoot.CatalogPage.Package.PackageDetails.PackageDependencyGroup.PackageDependency

type Components with

    [<ReactComponent>]
    static member PackageDependency(packageDependency: PackageDependency) =
        let range =
            // See https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource#package-dependency
            packageDependency.Range |> Option.defaultValue "(, )"

        Html.div [
            prop.key packageDependency.Id
            prop.children [
                Html.a [
                    prop.text packageDependency.Id
                ]
                Bulma.text.span [
                    spacing.ml2
                    prop.text range
                ]
            ]
        ]

    [<ReactComponent>]
    static member DependencyGroup(dependencyGroup: PackageDependencyGroup) =
        let framework =
            dependencyGroup.TargetFramework
            |> Option.defaultValue "All supported frameworks"

        Html.div [
            Html.div [
                prop.className "title is-5 is-underlined"
                prop.text framework
            ]

            match dependencyGroup.Dependencies with
            | Some dependencies ->
                Html.ul [
                    for dependency in dependencies do
                        Components.PackageDependency dependency
                ]

            | None ->
                Bulma.text.div [
                    color.hasTextGrey
                    prop.text "No dependencies"
                ]
        ]

    [<ReactComponent>]
    static member Dependencies
        (nugetPackage: V3.SearchResponse.Package)
        (catalogPage: V3.CatalogRoot.CatalogPage)
        (version: string)
        =
        match catalogPage.Items with
        | Some packages ->
            let packageInfo =
                packages
                |> List.tryFind (fun package ->
                    package.CatalogEntry.Version = version
                )

            match packageInfo with
            | Some packageInfo ->
                match packageInfo.CatalogEntry.DependencyGroups with
                | Some dependencyGroups ->
                    Html.div [
                        for dependencyGroup in dependencyGroups do
                            Components.DependencyGroup dependencyGroup
                    ]

                | None ->
                    Bulma.text.div [
                        color.hasTextGrey
                        prop.text "No dependencies"
                    ]

            | None ->
                Html.text
                    "Could not find the requested version in the catalog entry"

        | None -> Html.text "Missing packages information in the catalog page"
