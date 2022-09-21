// This namespace contains types which doesn't belongs to a specific category
// or doesn't enforce domain modeling.
namespace Fable.Packages.Types

[<RequireQualifiedAccess>]
type LicenseType =
    | File of string
    | Expression of string
