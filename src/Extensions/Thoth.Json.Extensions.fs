module Thoth.Json

open Thoth.Json

module Decode =

    let stringOrStringListDecoder: Decoder<string list> =

        Decode.oneOf [
            Decode.list Decode.string

            Decode.string |> Decode.map List.singleton
        ]

    let stringOrStringArrayDecoder: Decoder<string array> =

        Decode.oneOf [
            Decode.array Decode.string

            Decode.string |> Decode.map Array.singleton
        ]
