namespace Fable.Packages.Types

[<RequireQualifiedAccess>]
type NuGetOption = | IncludePreRelease

module NuGetOption =

    let toLabel (option: NuGetOption) =
        match option with
        | NuGetOption.IncludePreRelease -> "Include prerelease"
