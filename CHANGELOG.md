# Changelog for the module

The format is based on and uses the types of changes according to [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.7.0] - 2026-02-25

## [2.6.2] - 2025-10-11

### Changed

- Bumped Azure.Identity to 1.17.0
- Bumped Azure.Identity.Broker to 1.3.0
- Bumped System.Security.Cryptography.ProtectedData to 9.0.9

## [2.6.1] - 2025-08-17

### Added

- The name of the token cache being removed is now shown in the warning message when using `Clear-AzTokenCache` with the `-Force` parameter

### Changed

- Bumped Azure.Identity to 1.15.0
- Bumped Azure.Identity.Broker to 1.2.1
- Bumped System.IdentityModel.Tokens.Jwt to 8.14.0
- Bumped System.Security.Cryptography.ProtectedData to 9.0.7
- Bumped Microsoft.Identity.Client.NativeInterop to 0.19.4

## [2.6.0] - 2025-07-04

### Added

- New command `Get-AzTokenCache`, closes #96 
- New parameters for `Clear-AzTokenCache`
  - The new `-Force` parameter will attempt to delete the entire token cache directory instead of only clearing it from tokens. _Use at your own risk_, since it deletes a directory and recursively deletes its files on disk.
  - The new `-RootPath` allows for specifying a custom root directory for token caches. This may show up in other commands in the future.
- All token cache parameters now support argument completers to suggest existing caches in the chosen root directory
- Alias `-Name` added for `TokenCache` parameter for Clear- & Get-commands.

## [2.5.0] - 2025-07-01

### Removed

- Removed Visual Studio Code credential option which has been deprecated

### Changed

- When getting Tokens non-interactively, Managed Identity is now last in the credential chain until #112 is fixed
- Bumped Azure.Identity from 1.12.1 to 1.14.1
- Bumped Microsoft.VisualStudio.Threading from 17.12.19 to 17.14.15
- Bumped Azure.Identity.Broker from 1.1.0 to 1.2.0
- Bumped System.IdentityModel.Tokens.Jwt from 8.3.0 to 8.12.1
- Bumped System.Security.Cryptography.ProtectedData from 9.0.0 to 9.0.6
- Bumped Microsoft.Identity.Client.NativeInterop from 0.17.2 to 0.19.3

## [2.4.1] - 2024-12-18

### Changed

- `-TenantId` parameter is now called `-Tenant`, but `-TenantId` alias is still supported #99

## [2.4.0] - 2024-12-12

### Added

- Added -CredentialPrecedence parameter to allow for setting the precedence of the credentials used for non-interactive authentication #13

### Changed

- Parameter -ClientId now explains usage in a warning message when used with non-interactive parameter set

### Fixed

- Downgraded Azure.Identity until issue <https://github.com/Azure/azure-sdk-for-net/issues/47057> is resolved #112

## [2.3.0] - 2024-08-21

### Fixed

- Upgraded gitversion config in repo to support version 6

### Added

- Adds related links links to blog posts for Get-AzToken and the parameters -WorkloadIdentity & -ExternalToken
- Added `-TimeoutSeconds` parameter for Managed Identity authentication and non-interactive authentication
- Added Managed Identity authentication as first option of non-interactive login

## [2.2.10] - 2024-05-22

### Changed

- Bumped Azure.Identity from 1.10.4 to 1.11.3.
- Bumped System.IdentityModule.Tokens.Jwt from 7.2.0 to 7.5.2.
- Bumped Microsoft.VisualStudio.Threading from 7.8.14 to 7.10.48.

## [2.2.9] - 2024-01-22

### Added

- Added support for ClientCertificate authentication [#16](https://github.com/PalmEmanuel/AzAuth/issues/16)

## [2.2.8] - 2024-01-11

### Added

- Implemented Sampler for the GitHub project [#45](https://github.com/PalmEmanuel/AzAuth/issues/45)
- Bumped System.IdentityModule.Tokens.Jwt from 7.0.3 to 7.2.0

### Changed

- Improved build and test workflow to run on multiple platforms
- Updated LICENSE year

[unreleased]: https://github.com/PalmEmanuel/AzAuth/compare/v2.7.0...HEAD
[2.7.0]: https://github.com/PalmEmanuel/AzAuth/compare/v2.6.2...v2.7.0
[2.6.2]: https://github.com/PalmEmanuel/AzAuth/compare/v2.6.1...v2.6.2
[2.6.1]: https://github.com/PalmEmanuel/AzAuth/compare/v2.6.0...v2.6.1
[2.6.0]: https://github.com/PalmEmanuel/AzAuth/compare/v2.5.0...v2.6.0
[2.5.0]: https://github.com/PalmEmanuel/AzAuth/compare/v2.4.1...v2.5.0
[2.4.1]: https://github.com/PalmEmanuel/AzAuth/compare/v2.4.0...v2.4.1
[2.4.0]: https://github.com/PalmEmanuel/AzAuth/compare/v2.3.0...v2.4.0
[2.3.0]: https://github.com/PalmEmanuel/AzAuth/compare/v2.2.10...v2.3.0
[2.2.10]: https://github.com/PalmEmanuel/AzAuth/compare/v2.2.9...v2.2.10
[2.2.9]: https://github.com/PalmEmanuel/AzAuth/compare/v2.2.8...v2.2.9
[2.2.8]: https://github.com/PalmEmanuel/AzAuth/compare/1371440a317d3b48245636c58caeabea85331e21...v2.2.8
