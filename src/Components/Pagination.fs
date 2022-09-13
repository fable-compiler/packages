module Fable.Packages.Components.Pagination

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type PaginationProps =
    {| CurrentPage: int
       TotalHits: int
       OnNavigate: int -> unit
       ElementsPerPage: int |}

type Components with

    [<ReactComponent>]
    static member private PaginationLink
        (pageRank: int)
        (isCurrent: bool)
        (onNavigate: int -> unit)
        =
        Bulma.paginationLink.a [
            if isCurrent then
                paginationLink.isCurrent

            prop.onClick (fun _ -> onNavigate pageRank)

            prop.text (pageRank + 1) // Human friendly page start from 1
        ]

    [<ReactComponent>]
    static member Pagination(props: PaginationProps) =
        let totalPage =
            let computedTotalPage =
                Helpers.computateTotalPageCount
                    props.TotalHits
                    props.ElementsPerPage

            // I don't know why but for the pagination to be correct,
            // we need to remove 1 to the total page count.
            // Note: When fetching the packages from NuGet, we don't need to do that
            computedTotalPage - 1

        let deltaAroundCurrentPage = 1
        let minPageRank = props.CurrentPage - deltaAroundCurrentPage
        let maxPageRank = props.CurrentPage + deltaAroundCurrentPage

        Bulma.pagination [
            pagination.isCentered
            prop.className "packages-pagination"

            prop.children [

                Bulma.paginationPrevious.a [
                    if props.CurrentPage <= 0 then
                        prop.className "is-disabled"
                    else
                        prop.onClick (fun _ ->
                            props.OnNavigate(props.CurrentPage - 1)
                        )

                    prop.text "Previous"
                ]

                Bulma.paginationNext.a [
                    if props.CurrentPage >= totalPage then
                        prop.className "is-disabled"
                    else
                        prop.onClick (fun _ ->
                            props.OnNavigate(props.CurrentPage + 1)
                        )

                    prop.text "Next"
                ]

                Bulma.paginationList [
                    helpers.isHiddenMobile

                    prop.children [
                        for pageRank = 0 to totalPage do
                            let isCurrent = pageRank = props.CurrentPage

                            // Always render the first page
                            if pageRank = 0 then
                                Components.PaginationLink
                                    pageRank
                                    isCurrent
                                    props.OnNavigate

                            // If this is the page directly before the minPageRank render an ellipsis
                            else if pageRank = minPageRank - 1 then
                                Bulma.paginationEllipsis []

                            // If the page rank is between minPageRank and maxPageRank render a link
                            else if
                                pageRank >= minPageRank
                                && pageRank <= maxPageRank
                            then
                                Components.PaginationLink
                                    pageRank
                                    isCurrent
                                    props.OnNavigate

                            // Render the last page
                            // We handle try to render the last page before the "right ellipsis"
                            // Because we always want to render the last page
                            else if pageRank = totalPage then
                                Components.PaginationLink
                                    pageRank
                                    isCurrent
                                    props.OnNavigate

                            // If this is the page directly after the maxPageRank render an ellipsis
                            else if pageRank = maxPageRank + 1 then
                                Bulma.paginationEllipsis []
                    ]
                ]
            ]
        ]
