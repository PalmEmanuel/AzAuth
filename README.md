# AzAuth

[![AzAuth]][AzAuthGallery] [![AzAuthDownloads]][AzAuthGallery]

AzAuth is a lightweight PowerShell module to handle Azure authentication, using the Azure.Identity MSAL library.

## Installation

AzAuth is published to [PowerShell Gallery](https://www.powershellgallery.com/packages/AzAuth/) and can be installed with a simple command.

```powershell
Install-Module -Name AzAuth
```

## Using AzAuth

AzAuth supports multiple ways of getting an access token for a user or identity.

The simplest way is to just run the command, which will look for available tokens among shared tools or sources on the machine.

```PowerShell
# Find a token from already authenticated sources like Azure PowerShell or CLI
# Unless otherwise specified, the command uses ".default" as scope, and "https://graph.microsoft.com" as the resource
Get-AzToken
```

AzAuth implements MSAL and also allows for interactive browser logins, and even persistent credential caches!

```PowerShell
Get-AzToken -Interactive -TokenCache 'AzAuthCache'
```

AzAuth also caches authentications made for the duration of the session even without specifying a cache, so once you've logged in you can get new access tokens without logging in again. No passwords or credentials are stored, only the refresh token as part of the [authenticated credential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.interactivebrowsercredential) object.

```PowerShell
Get-AzToken -Resource 'https://management.azure.com'
```

For more information, see the help documentation!

```PowerShell
Get-Help Get-AzToken -Full
```

## Bug report and feature requests

If you find a bug or have an idea for a new feature, please create an issue in the repo! Before submitting, have a look and see if there are any similar issues already open, in which case you can add to the discussion.

## Contribution

If you like AzAuth and want to contribute, you are very welcome to do so! Please read the [Contribution Guide](CONTRIBUTING.md) to get started!

---

<!-- References -->
[AzAuthDownloads]: https://img.shields.io/powershellgallery/dt/AzAuth
[AzAuthGallery]: https://www.powershellgallery.com/packages/AzAuth/
[AzAuth]: https://img.shields.io/powershellgallery/v/AzAuth?label=AzAuth
