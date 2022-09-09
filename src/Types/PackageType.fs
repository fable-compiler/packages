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
