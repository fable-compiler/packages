module Thoth.Json

open Thoth.Json

module Decode =

    let stringOrStringListDecoder: Decoder<string list> =

        Decode.oneOf [
            Decode.list Decode.string

            Decode.string |> Decode.map List.singleton
        ]
