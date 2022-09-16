module Fable.Packages.Components.SearchForm

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma
open Feliz.UseElmish
open Elmish
open Fable.Packages.Types

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type Model = SearchOptions

type private Msg =
    | ToggleTarget of Target
    | TogglePackageType of PackageType
    | ChooseSortBy of SortBy
    | ToggleOption of NuGetOption
    | ChangeTextField of string
    | Search
    | Reset

let private init () = SearchOptions.initial, Cmd.none

let private update (onSearch: SearchOptions -> unit) (msg: Msg) (model: Model) =
    match msg with
    | ToggleTarget target ->
        { model with
            Targets = Set.toggle target model.Targets
        },
        Cmd.none

    | TogglePackageType packageType ->
        { model with
            PackageTypes = Set.toggle packageType model.PackageTypes
        },
        Cmd.none

    | ChooseSortBy sortBy ->
        { model with
            SortBy = sortBy
        },
        Cmd.none

    | ToggleOption option ->
        { model with
            Options = Set.toggle option model.Options
        },
        Cmd.none

    | ChangeTextField textFieldValue ->
        { model with
            TextField = textFieldValue
        },
        Cmd.none

    | Reset ->
        let newModel = SearchOptions.initial

        newModel, Cmd.OfFunc.exec onSearch newModel

    | Search -> model, Cmd.OfFunc.exec onSearch model

type SearchFormProps = {| OnSearch: SearchOptions -> unit |}

type Components with

    [<ReactComponent>]
    static member private CheckboxField
        (
            text: string,
            isChecked: bool,
            onChange: unit -> unit
        ) =
        Bulma.field.div [
            Bulma.control.div [
                Html.label [
                    prop.className "checkbox"
                    prop.children [
                        Bulma.input.checkbox [
                            prop.isChecked isChecked
                            prop.onChange (fun (_: bool) -> onChange ())
                        ]
                        Html.text text
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member private CheckradioField
        (
            text: string,
            name: string,
            isChecked: bool,
            onChange: unit -> unit
        ) =
        Bulma.field.div [
            Bulma.control.div [
                Html.label [
                    prop.className "radio"
                    prop.children [
                        Bulma.input.radio [
                            prop.isChecked isChecked
                            prop.name name
                            prop.onChange (fun (_: bool) -> onChange ())
                        ]
                        Html.text text
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member private CheckboxTargetField
        (
            target: Target,
            selectedTargets: Set<Target>,
            dispatch: Dispatch<Msg>
        ) =
        let isChecked = Set.contains target selectedTargets

        Components.CheckboxField(
            Target.toLabel target,
            isChecked,
            (fun () -> dispatch (ToggleTarget target))
        )

    [<ReactComponent>]
    static member private CheckboxPackageTypeField
        (
            packageType: PackageType,
            selectedPackageTypes: Set<PackageType>,
            dispatch: Dispatch<Msg>
        ) =
        let isChecked = Set.contains packageType selectedPackageTypes

        Components.CheckboxField(
            PackageType.toLabel packageType,
            isChecked,
            fun () -> dispatch (TogglePackageType packageType)
        )

    [<ReactComponent>]
    static member private CheckradioSortByField
        (
            sortBy: SortBy,
            selectedSortBy: SortBy,
            dispatch: Dispatch<Msg>
        ) =
        let isChecked = sortBy = selectedSortBy

        Components.CheckradioField(
            SortBy.toLabel sortBy,
            "sort-by",
            isChecked,
            fun () -> dispatch (ChooseSortBy sortBy)
        )

    [<ReactComponent>]
    static member private CheckboxOptionField
        (
            option: NuGetOption,
            selectedOptions: Set<NuGetOption>,
            dispatch: Dispatch<Msg>
        ) =
        let isChecked = Set.contains option selectedOptions

        Components.CheckboxField(
            NuGetOption.toLabel option,
            isChecked,
            (fun () -> dispatch (ToggleOption option))
        )

    [<ReactComponent>]
    static member private FormColumn
        (
            label: string,
            children: ReactElement list
        ) =

        Bulma.column [
            column.is3

            prop.children [
                Bulma.field.div [
                    Bulma.label [
                        prop.text label
                    ]
                ]

                yield! children
            ]
        ]

    [<ReactComponent>]
    static member private Filters(model, dispatch) =
        let isFiltersVisible, setFiltersVisible = React.useState false

        Html.div [
            Html.span [
                prop.className
                    "icon-text mb-3 advanced-search-with-filters-button"
                prop.onClick (fun _ -> setFiltersVisible (not isFiltersVisible))
                prop.children [
                    Html.span [
                        prop.text "Advanced search with filters"
                    ]
                    Bulma.icon [
                        Html.i [
                            if isFiltersVisible then
                                prop.className "fas fa-angle-down"
                            else
                                prop.className "fas fa-angle-right"
                        ]
                    ]
                ]
            ]

            if isFiltersVisible then

                Bulma.box [

                    Bulma.columns [
                        Components.FormColumn(
                            "Target",
                            [
                                Components.CheckboxTargetField(
                                    Target.Dotnet,
                                    model.Targets,
                                    dispatch
                                )
                                Components.CheckboxTargetField(
                                    Target.JavaScript,
                                    model.Targets,
                                    dispatch
                                )
                                Components.CheckboxTargetField(
                                    Target.Rust,
                                    model.Targets,
                                    dispatch
                                )
                                Components.CheckboxTargetField(
                                    Target.Python,
                                    model.Targets,
                                    dispatch
                                )
                                Components.CheckboxTargetField(
                                    Target.Dart,
                                    model.Targets,
                                    dispatch
                                )
                                Components.CheckboxTargetField(
                                    Target.All,
                                    model.Targets,
                                    dispatch
                                )
                            ]
                        )

                        Components.FormColumn(
                            "Package type",
                            [
                                Components.CheckboxPackageTypeField(
                                    PackageType.Binding,
                                    model.PackageTypes,
                                    dispatch
                                )
                                Components.CheckboxPackageTypeField(
                                    PackageType.Library,
                                    model.PackageTypes,
                                    dispatch
                                )
                            ]
                        )

                        Components.FormColumn(
                            "Sort by",
                            [
                                Components.CheckradioSortByField(
                                    SortBy.Relevance,
                                    model.SortBy,
                                    dispatch
                                )
                                Components.CheckradioSortByField(
                                    SortBy.Downloads,
                                    model.SortBy,
                                    dispatch
                                )
                                Components.CheckradioSortByField(
                                    SortBy.RecentlyUpdated,
                                    model.SortBy,
                                    dispatch
                                )
                            ]
                        )

                        Components.FormColumn(
                            "Options",
                            [
                                Components.CheckboxOptionField(
                                    NuGetOption.IncludePreRelease,
                                    model.Options,
                                    dispatch
                                )
                            ]
                        )

                    ]

                    Bulma.field.div [
                        field.isGrouped

                        prop.children [
                            Bulma.control.div [
                                Bulma.button.button [
                                    color.isPrimary
                                    prop.text "Apply"
                                    prop.onClick (fun _ -> dispatch Search)
                                ]
                            ]
                            Bulma.control.div [
                                Bulma.button.button [
                                    color.isPrimary
                                    color.isLight
                                    prop.text "Reset"
                                    prop.onClick (fun _ -> dispatch Reset)
                                ]
                            ]
                        ]
                    ]
                ]
        ]

    [<ReactComponent>]
    static member private SearchInputField model dispatch =
        Bulma.field.div [
            field.isGrouped

            prop.children [
                Bulma.control.div [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.placeholder "Search for packages..."
                            prop.value model.TextField
                            prop.onChange (fun newText ->
                                dispatch (ChangeTextField newText)
                            )
                            prop.onKeyDown (fun ev ->
                                if ev.key = "Enter" then
                                    dispatch Search
                            )
                        ]
                    ]
                ]

                Bulma.control.div [
                    Bulma.button.button [
                        color.isPrimary
                        prop.text "Search"
                        prop.onClick (fun _ -> dispatch Search)
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member SearchForm(props: SearchFormProps) =
        let model, dispatch =
            React.useElmish (init, update props.OnSearch, [||])

        Html.div [
            Components.SearchInputField model dispatch
            Components.Filters(model, dispatch)
        ]
