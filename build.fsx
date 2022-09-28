#r "nuget: Fun.Build, 0.1.6"

open Fun.Build
open System
open System.IO

let args = Environment.GetCommandLineArgs()

let isHelp = Seq.contains "--help" args || Seq.contains "help" args

if isHelp then
    printfn
        """
Usage: dotnet fsi build.fsx [options]

Options:
    --watch         Start a local web server and watch for changes
    --publish       Build a production version of the site and publish it on GitHub Pages
    --help, help    Show this help
    """

    exit 0

module Directory =

    let deleteIfExists path =
        if Directory.Exists path then
            Directory.Delete(path, true)

module Path =

    let src = Path.Combine(__SOURCE_DIRECTORY__, "src")

pipeline "Fable.Packages" {
    workingDir __SOURCE_DIRECTORY__

    stage "Clean" {
        run (fun ctx ->
            Directory.deleteIfExists "src/bin"
            Directory.deleteIfExists "src/obj"
        )
    }

    stage "Restore tools and dependencies" {
        run "npm install"
        run "dotnet tool restore"
    }

    stage "Watch" {
        whenCmdArg "--watch"
        workingDir Path.src
        paralle
        stage "Vite" { run "npx vite dev" }
        stage "Fable" { run "dotnet fable --watch" }
    }

    stage "Publish" {
        whenCmdArg "--publish"
        workingDir Path.src
        run "dotnet fable"
        run "npx vite build"
        run "npx gh-pages -d dist"
    }

    runIfOnlySpecified false
}
