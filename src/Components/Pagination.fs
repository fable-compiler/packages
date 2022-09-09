module Fable.Packages.Components.Pagination

open Feliz
open Feliz.Bulma

[<ReactComponent>]
let private PaginationLink (pageRank: int) (isCurrent: bool) (onNavigate: int -> unit)=
    Bulma.paginationLink.a [
        if isCurrent then
            paginationLink.isCurrent

        prop.onClick (fun _ ->
            onNavigate pageRank
        )

        prop.text (pageRank + 1) // Human friendly page start from 1
    ]

type PaginationProps =
    {| CurrentPage: int
       TotalHits: int
       OnNavigate: int -> unit |}

[<ReactComponent>]
let Pagination (props: PaginationProps) =
    let elementsPerPage = 10
    let totalPage = props.TotalHits / elementsPerPage
    let deltaAroundCurrentPage = 1
    let minPageRank = props.CurrentPage - deltaAroundCurrentPage
    let maxPageRank = props.CurrentPage + deltaAroundCurrentPage

    Bulma.pagination [
        pagination.isCentered
        prop.style [
            style.maxWidth (length.px 600)
            style.marginLeft length.auto
            style.marginRight length.auto
        ]

        prop.children [
            Bulma.paginationPrevious.a [
                prop.text "Previous"
            ]

            Bulma.paginationNext.a [
                prop.text "Next"
            ]

            Bulma.paginationList [
                helpers.isHiddenMobile

                prop.children [
                    for pageRank = 0 to totalPage do
                        let isCurrent = pageRank = props.CurrentPage

                        // Always render the first page
                        if pageRank = 0 then
                            PaginationLink pageRank isCurrent props.OnNavigate

                        // If this is the page directly before the minPageRank render an ellipsis
                        else if pageRank = minPageRank - 1 then
                            Bulma.paginationEllipsis []

                        // If the page rank is between minPageRank and maxPageRank render a link
                        else if
                            pageRank >= minPageRank && pageRank <= maxPageRank
                        then
                            PaginationLink pageRank isCurrent props.OnNavigate

                        // If this is the page directly after the maxPageRank render an ellipsis
                        else if pageRank = maxPageRank + 1 then
                            Bulma.paginationEllipsis []

                        // Always render the last page
                        else if pageRank = totalPage then
                            PaginationLink pageRank isCurrent props.OnNavigate
                ]
            ]
        ]
    ]
