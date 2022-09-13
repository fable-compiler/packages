namespace Fable.Packages.Types

[<RequireQualifiedAccess>]
type PackageType =
    | Binding
    | Library

module PackageType =

    let toLabel (packageType: PackageType) =
        match packageType with
        | PackageType.Binding -> "Binding"
        | PackageType.Library -> "Library"

    let toTag (packageType: PackageType) =
        match packageType with
        | PackageType.Binding -> "fable-binding"
        | PackageType.Library -> "fable-library"
