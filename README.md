# Fable.Packages

Fable.Packages is a tool used to search for Fable packages.

## Development

### How to run ?

You can run the project using the `build.cmd <Target>` or `build.sh <Target>`.

Description of the targets:

| Target | Description |
|--------|---|
| Watch | Start a local server on [http://localhost:3000](http://localhost:3000)   |
| Build | Build the website in production mode. Destination is `src/dist`  |

If preferred, you can can run the project on [Gitpod](https://gitpod.io/) by clicking on the button below.

[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/MangelMaxime/template-gitpod-fable-3)

### Conventions

#### Components

TODO

#### Path based routing

Fable.Packages is written using path based routing as it makes it easier to host
the website on GitHub Pages.

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
