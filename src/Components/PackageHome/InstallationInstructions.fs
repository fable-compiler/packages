module Fable.Packages.Components.PackageHome.InstallationInstructions

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Fable.Packages
open Fable.Packages.Components
open Fable.Packages.Types
open Fable.DateFunctions
open Feliz.Lucide

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""


[<ReactComponent(import="default", from="./CopyInstruction.jsx")>]
let private CopyButton(value : string) =
    React.imported()


type private InstallationMethod =
    | PackageManager
    | DotnetCli
    | PackageReference
    | PaketCli

type Components with

    [<ReactComponent>]
    static member private Tab
        (
            tabToRender: InstallationMethod,
            activeTab: InstallationMethod,
            onClick: InstallationMethod -> unit
        ) =

        let text =
            match tabToRender with
            | PackageManager -> "Package Manager"
            | DotnetCli -> ".NET CLI"
            | PackageReference -> "Package Reference"
            | PaketCli -> "Paket CLI"

        Bulma.tab [
            if tabToRender = activeTab then
                tab.isActive

            prop.children [
                Html.a [
                    prop.onClick (fun _ -> onClick tabToRender)
                    prop.children [
                        Html.span text
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member private TabsHeader
        (
            activeTab: InstallationMethod,
            setActiveTab: InstallationMethod -> unit
        ) =
        Html.div [
            prop.className "package-home-installation-methods-tabs"
            prop.children [
                Bulma.tabs [
                    tabs.isCentered
                    tabs.isToggle
                    tabs.isFullWidth

                    prop.children [
                        Html.ul [
                            Components.Tab(PaketCli, activeTab, setActiveTab)
                            Components.Tab(DotnetCli, activeTab, setActiveTab)
                            Components.Tab(
                                PackageReference,
                                activeTab,
                                setActiveTab
                            )
                            Components.Tab(
                                PackageManager,
                                activeTab,
                                setActiveTab
                            )
                        ]
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member private TabBody
        (
            activeTab: InstallationMethod,
            packageId: string,
            version: string
        ) =
        let instruction =
            match activeTab with
            | PackageManager ->
                $"Install-Package %s{packageId} -Version %s{version}"
            | DotnetCli ->
                $"dotnet add package %s{packageId} --version %s{version}"
            | PackageReference ->
                $"<PackageReference Include=\"%s{packageId}\" Version=\"%s{version}\" />"
            | PaketCli ->
                $"paket add %s{packageId} --version %s{version}"

        Html.div [
            prop.className "package-home-installation-methods-tab-body"
            prop.children [
                Html.div [
                    prop.className "instruction-preview"
                    prop.children [
                        Html.pre [
                            Html.code [
                                prop.text instruction
                            ]
                        ]
                    ]
                ]
                // Bulma.button.button [
                //     prop.className "copy-instruction"
                //     color.isPrimary
                //     prop.children [
                //         Lucide.Copy [
                //             lucide.size 24
                //         ]
                //     ]
                // ]
                CopyButton instruction
            ]
        ]

    [<ReactComponent>]
    static member InstallationInstructions(pacakgeId: string, version: string) =
        let currentMethodDisplayed, setCurrentMethodDisplayed =
            React.useState InstallationMethod.PaketCli

        Html.div [
            prop.className "package-home-installation-methods"
            prop.children [
                Components.TabsHeader(
                    currentMethodDisplayed,
                    setCurrentMethodDisplayed
                )
                Components.TabBody(currentMethodDisplayed, pacakgeId, version)
            ]
        ]
