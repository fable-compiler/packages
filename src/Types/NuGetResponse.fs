namespace Fable.Packages.Types

open System
open Thoth.Json

type NuGetPackageVersion =
    {
        Version : string
        Downloads: int
        Id : string
    }

module NuGetPackageVersion =

    let decoder : Decoder<NuGetPackageVersion> =
        Decode.object (fun get ->
            {
                Version = get.Required.Field "version" Decode.string
                Downloads = get.Required.Field "downloads" Decode.int
                Id = get.Required.Field "@id" Decode.string
            }
        )

type NuGetPackageType =
    | Template
    | Dependency
    | DotnetTool
    | Unkown of string

module NuGetPackageType =

    let decoder : Decoder<NuGetPackageType> =
        Decode.field "name" Decode.string
        |> Decode.andThen (
            function
            | "Template" -> Decode.succeed Template
            | "Dependency" -> Decode.succeed Dependency
            | "DotnetTool" -> Decode.succeed DotnetTool
            | unkown -> Decode.succeed (Unkown unkown)
        )

type NuGetPackage =
    {
        Id : string
        Version : string
        Description : string option
        IconUrl : string option
        LicenseUrl : string option
        ProjectUrl : string option
        Tags : (string list) option
        Authors : (string list) option
        Owners : (string list) option
        TotalDownloads : int64 option
        Versions : NuGetPackageVersion list
        PackageTypes : NuGetPackageType list
    }

module NuGetPackage =

    let decoder : Decoder<NuGetPackage> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Version = get.Required.Field "version" Decode.string
                Description = get.Optional.Field "description" Decode.string
                IconUrl = get.Optional.Field "iconUrl" Decode.string
                LicenseUrl = get.Optional.Field "licenseUrl" Decode.string
                ProjectUrl = get.Optional.Field "projectUrl" Decode.string
                Tags = get.Optional.Field "tags" (Decode.list Decode.string)
                Authors = get.Optional.Field "authors" (Decode.list Decode.string)
                Owners = get.Optional.Field "owners" (Decode.list Decode.string)
                TotalDownloads = get.Optional.Field "totalDownloads" Decode.int64
                Versions = get.Required.Field "versions" (Decode.list NuGetPackageVersion.decoder)
                PackageTypes = get.Required.Field "packageTypes" (Decode.list NuGetPackageType.decoder)
            }
        )

type NuGetResponse =
    {
        TotalHits: int
        Data : NuGetPackage list
    }

module NuGetResponse =

    let decoder : Decoder<NuGetResponse> =
        Decode.object (fun get ->
            {
                TotalHits = get.Required.Field "totalHits" Decode.int
                Data = get.Required.Field "data" (Decode.list NuGetPackage.decoder)
            }
        )

type NuGetRegistration5Semver1 =
    {
        Id : string
        Type : string list
        CatalogEntry : string
        Listed : bool
        PackageContent : string
        Published : DateTime
        Registration : string
    }

module NuGetRegistration5Semver1 =

    let decoder : Decoder<NuGetRegistration5Semver1> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "@id" Decode.string
                Type = get.Required.Field "@type" (Decode.list Decode.string)
                CatalogEntry = get.Required.Field "catalogEntry" Decode.string
                Listed = get.Required.Field "listed" Decode.bool
                PackageContent = get.Required.Field "packageContent" Decode.string
                Published = get.Required.Field "published" Decode.datetime
                Registration = get.Required.Field "registration" Decode.string
            }
        )

type IndexedNuGetPackage =
    {
        Package : NuGetPackage
        LastVersionInfo : NuGetRegistration5Semver1
    }
