# Changelog for the module

The format is based on and uses the types of changes according to [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Adds related links links to blog posts for Get-AzToken and the parameters -WorkloadIdentity & -ExternalToken
- Added `-TimeoutSeconds` parameter for Managed Identity authentication and non-interactive authentication
- Added Managed Identity authentication as first option of non-interactive login

## [2.2.10] - 2024-05-22

### Changed

-   Bumped Azure.Identity from 1.10.4 to 1.11.3.
-   Bumped System.IdentityModule.Tokens.Jwt from 7.2.0 to 7.5.2.
-   Bumped Microsoft.VisualStudio.Threading from 7.8.14 to 7.10.48.

## [2.2.9] - 2024-01-22

### Added

-   Added support for ClientCertificate authentication [#16](https://github.com/PalmEmanuel/AzAuth/issues/16)

## [2.2.8] - 2024-01-11

### Added

-   Implemented Sampler for the GitHub project [#45](https://github.com/PalmEmanuel/AzAuth/issues/45)
-   Bumped System.IdentityModule.Tokens.Jwt from 7.0.3 to 7.2.0

### Changed

-   Improved build and test workflow to run on multiple platforms
-   Updated LICENSE year

[Unreleased]: https://github.com/PalmEmanuel/AzAuth/compare/v2.2.10...HEAD

[2.2.10]: https://github.com/PalmEmanuel/AzAuth/compare/v2.2.9...v2.2.10

[2.2.9]: https://github.com/PalmEmanuel/AzAuth/compare/v2.2.8...v2.2.9

[2.2.8]: https://github.com/PalmEmanuel/AzAuth/compare/1371440a317d3b48245636c58caeabea85331e21...v2.2.8
