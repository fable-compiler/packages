namespace rec Fable.Packages.Types

open System
open Thoth.Json

#nowarn "40"

type NuGetRegistration5Semver1 = {
    Id: string
    Type: string list
    CatalogEntry: string
    Listed: bool
    PackageContent: string
    Published: DateTime
    Registration: string
}

module NuGetRegistration5Semver1 =

    let decoder: Decoder<NuGetRegistration5Semver1> =
        Decode.object (fun get -> {
            Id = get.Required.Field "@id" Decode.string
            Type = get.Required.Field "@type" (Decode.list Decode.string)
            CatalogEntry = get.Required.Field "catalogEntry" Decode.string
            Listed = get.Required.Field "listed" Decode.bool
            PackageContent = get.Required.Field "packageContent" Decode.string
            Published = get.Required.Field "published" Decode.datetime
            Registration = get.Required.Field "registration" Decode.string
        }
        )

module V3 =

    type SearchResponse =
        {
            TotalHits : int
            Data : SearchResponse.Package list
        }

    module SearchResponse =

        let decoder : Decoder<SearchResponse> =
            Decode.object (fun get -> {
                TotalHits = get.Required.Field "totalHits" Decode.int
                Data = get.Required.Field "data" (Decode.list Package.decoder)
            }
            )

        type Package = {
            /// <summary>
            /// The ID of the matched package
            /// </summary>
            Id : string
            /// <summary>
            /// The full SemVer 2.0.0 version string of the package (could contain build metadata)
            /// </summary>
            Version : string
            Description : string option
            /// <summary>
            /// All of the versions of the package matching the prerelease parameter
            /// </summary>
            Versions : Package.Version list
            Authors : (string list) option
            IconUrl : string option
            LicenseUrl : string option
            Owners : (string list) option
            ProjectUrl : string option
            /// <summary>
            /// The absolute URL to the associated <a href="https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource#registration-index">registration index</a>
            /// </summary>
            Registration : string option
            Summary : string option
            Tags : (string list) option
            Title : string option
            /// <summary>
            /// This value can be inferred by the sum of downloads in the <c>versions</c> array
            /// </summary>
            TotalDownloads : int64 option
            /// <summary>
            /// A JSON boolean indicating whether the package is <a href="https://docs.microsoft.com/en-us/nuget/nuget-org/id-prefix-reservation">verified</a>
            /// </summary>
            Verified : bool option
            /// <summary>
            /// The package types defined by the package author (added in SearchQueryService/3.5.0)
            /// </summary>
            PackageTypes : Package.PackageType list
        }

        module Package =

            let decoder : Decoder<Package> =
                Decode.object (fun get -> {
                    Id = get.Required.Field "id" Decode.string
                    Version = get.Required.Field "version" Decode.string
                    Description = get.Optional.Field "description" Decode.string
                    Versions = get.Required.Field "versions" (Decode.list Version.decoder)
                    Authors = get.Optional.Field "authors" Decode.stringOrStringListDecoder
                    IconUrl = get.Optional.Field "iconUrl" Decode.string
                    LicenseUrl = get.Optional.Field "licenseUrl" Decode.string
                    Owners = get.Optional.Field "owners" Decode.stringOrStringListDecoder
                    ProjectUrl = get.Optional.Field "projectUrl" Decode.string
                    Registration = get.Optional.Field "registration" Decode.string
                    Summary = get.Optional.Field "summary" Decode.string
                    Tags = get.Optional.Field "tags" Decode.stringOrStringListDecoder
                    Title = get.Optional.Field "title" Decode.string
                    TotalDownloads = get.Optional.Field "totalDownloads" Decode.int64
                    Verified = get.Optional.Field "verified" Decode.bool
                    PackageTypes = get.Required.Field "packageTypes" (Decode.list PackageType.decoder)
                }
                )

            type Version = {
                /// <summary>
                /// The absolute URL to the associated <a href="https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource#registration-leaf">registration leaf</a>
                /// </summary>
                Id : string
                /// <summary>
                /// The full SemVer 2.0.0 version string of the package (could contain build metadata)
                /// </summary>
                Version : string
                /// <summary>
                /// The number of downloads for this specific package version
                /// </summary>
                Downloads : int
            }

            module Version =

                let decoder : Decoder<Version> =
                    Decode.object (fun get -> {
                        Id = get.Required.Field "@id" Decode.string
                        Version = get.Required.Field "version" Decode.string
                        Downloads = get.Required.Field "downloads" Decode.int
                    }
                    )

            type PackageType =
                | Template
                | Dependency
                | DotnetTool
                | Unkown of string

            module PackageType =

                let decoder: Decoder<PackageType> =
                    Decode.field "name" Decode.string
                    |> Decode.andThen (
                        function
                        | "Template" -> Decode.succeed Template
                        | "Dependency" -> Decode.succeed Dependency
                        | "DotnetTool" -> Decode.succeed DotnetTool
                        | unkown -> Decode.succeed (Unkown unkown)
                    )


    type CatalogRoot = {
        Count: int
        Items: CatalogRoot.CatalogPage list
    }

    module CatalogRoot =

        let decoder: Decoder<CatalogRoot> =
            Decode.object (fun get -> {
                Count = get.Required.Field "count" Decode.int
                Items =
                    get.Required.Field "items" (Decode.list CatalogPage.decoder)
            }
            )

        type CatalogPage = {
            Count: int
            Items: (CatalogPage.Package list) option
            Parent: string option
            Lower: string
            Upper: string
        }

        module CatalogPage =

            let decoder: Decoder<CatalogPage> =
                Decode.object (fun get -> {
                    Count = get.Required.Field "count" Decode.int
                    Items =
                        get.Optional.Field "items" (Decode.list Package.decoder)
                    Parent = get.Optional.Field "parent" Decode.string
                    Lower = get.Required.Field "lower" Decode.string
                    Upper = get.Required.Field "upper" Decode.string
                }
                )

            type Package = {
                CatalogEntry: Package.PackageDetails
                PackageContent: string
            }

            module Package =

                let decoder: Decoder<Package> =
                    Decode.object (fun get -> {
                        CatalogEntry =
                            get.Required.Field
                                "catalogEntry"
                                PackageDetails.decoder
                        PackageContent =
                            get.Required.Field "packageContent" Decode.string
                    }
                    )

                type PackageDetails = {
                    Authors: (string list) option
                    DependencyGroups: (PackageDetails.PackageDependencyGroup list) option
                    Description: string option
                    IconUrl: string option
                    Id: string
                    LicenseUrl: string option
                    LicenseExpression: string option
                    Listed: bool option
                    MinClientVersion: string option
                    ProjectUrl: string option
                    Published: DateTime option
                    RequireLicenseAcceptance: bool option
                    Summary: string option
                    Tags: (string list) option
                    Title: string option
                    Version: string
                // Vulnerabilities : obj array
                }

                module PackageDetails =

                    let decoder: Decoder<PackageDetails> =
                        Decode.object (fun get -> {
                            Authors =
                                get.Optional.Field
                                    "authors"
                                    Decode.stringOrStringListDecoder
                            DependencyGroups =
                                get.Optional.Field
                                    "dependencyGroups"
                                    (Decode.list PackageDependencyGroup.decoder)
                            Description =
                                get.Optional.Field "description" Decode.string
                            IconUrl =
                                get.Optional.Field
                                    "icodzdzdzdnUrl"
                                    Decode.string
                            Id = get.Required.Field "id" Decode.string
                            LicenseUrl =
                                get.Optional.Field "licenseUrl" Decode.string
                            LicenseExpression =
                                get.Optional.Field
                                    "licenseExpression"
                                    Decode.string
                            Listed = get.Optional.Field "listed" Decode.bool
                            MinClientVersion =
                                get.Optional.Field
                                    "minClientVersion"
                                    Decode.string
                            ProjectUrl =
                                get.Optional.Field "projectUrl" Decode.string
                            Published =
                                get.Optional.Field "published" Decode.datetime
                            RequireLicenseAcceptance =
                                get.Optional.Field
                                    "requireLicenseAcceptance"
                                    Decode.bool
                            Summary = get.Optional.Field "summary" Decode.string
                            Tags =
                                get.Optional.Field
                                    "tags"
                                    Decode.stringOrStringListDecoder
                            Title = get.Optional.Field "title" Decode.string
                            Version = get.Required.Field "version" Decode.string
                        }
                        )

                    type PackageDependencyGroup = {
                        Dependencies: (PackageDependencyGroup.PackageDependency list) option
                        TargetFramework: string option
                    }

                    module PackageDependencyGroup =

                        let decoder: Decoder<PackageDependencyGroup> =
                            Decode.object (fun get -> {
                                Dependencies =
                                    get.Optional.Field
                                        "dependencies"
                                        (Decode.list PackageDependency.decoder)
                                TargetFramework =
                                    get.Optional.Field
                                        "targetFramework"
                                        Decode.string
                            }
                            )

                        type PackageDependency = {
                            Id: string
                            Range: string option
                            Registration: string option
                        }

                        module PackageDependency =

                            let decoder: Decoder<PackageDependency> =
                                Decode.object (fun get -> {
                                    Id = get.Required.Field "id" Decode.string
                                    Range =
                                        get.Optional.Field "range" Decode.string
                                    Registration =
                                        get.Optional.Field
                                            "registration"
                                            Decode.string
                                }
                                )
