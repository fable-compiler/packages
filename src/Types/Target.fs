namespace Fable.Packages.Types

[<RequireQualifiedAccess>]
type Target =
    | Dotnet
    | JavaScript
    | Rust
    | Python
    | Dart
    | All

module Target =

    let toLabel (target: Target) =
        match target with
        | Target.Dotnet -> "Dotnet"
        | Target.JavaScript -> "JavaScript"
        | Target.Rust -> "Rust"
        | Target.Python -> "Python"
        | Target.Dart -> "Dart"
        | Target.All -> "All"

    let toTag (target: Target) =
        match target with
        | Target.Dotnet -> "fable-dotnet"
        | Target.JavaScript -> "fable-javascript"
        | Target.Rust -> "fable-rust"
        | Target.Python -> "fable-python"
        | Target.Dart -> "fable-dart"
        | Target.All -> "fable"
