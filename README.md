# Fable.Packages

Fable.Packages is a tool used to search for Fable packages.

## Development

### How to run ?

You can run the project using the `dotnet fsi build.fsx <Target>`.

Description of the targets:

| Target | Description |
|--------|---|
| Watch | Start a local web server and watch for changes  |
| Build | Build a production version of the site and publish it on GitHub Pages  |

### Conventions

#### Components

Components are written using method extension on `Components` classes located in `src\Components\Components.fs`.

Example:

```fs
module Fable.Packages.Components.Pagination

open Fable.Core.JsInterop
open Feliz
open Feliz.Bulma

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type Components with

    [<ReactComponent>]
    static member private Pagination () =
        Html.div "Paginiation components"
```

To consume them you need to open the module to make it accessible on `Components` class.

```fs
open Fable.Packages.Pages.Search

open Fable.pacakges.Components
open Fable.pacakges.Components.Pagination

type Pages with

    [<ReactComponent>]
    static member Search() =
        Components.Pagination() // You can consume the component here

```

### Pages

Pages follow the same convention as components but they extends the `Pages` located in `src\Pages\Pages.fs`.

#### Path based routing

Fable.Packages is written using path based routing as it makes it easier to host the website on GitHub Pages.

This also make it easier to use anchor elements as we can rely on the browser native behavior.

Example:

```fs
Bulma.button.a [
    color.isPrimary
    color.isLight
    prop.text "Return to Home"
    prop.href (Router.toUrl Router.Page.Search)
]
```
