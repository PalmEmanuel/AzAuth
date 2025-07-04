---
external help file: AzAuth.PS.dll-Help.xml
Module Name: AzAuth
online version:
schema: 2.0.0
---

# Get-AzTokenCache

## SYNOPSIS

Lists all available token caches in the default cache directory.

## SYNTAX

```
Get-AzTokenCache [-IncludeAccounts] [-RootPath <String>]
 [<CommonParameters>]
```

## DESCRIPTION

The Get-AzTokenCache command discovers and lists all token caches stored in a cache directory. This includes caches created by interactive authentication, device code flows, and other cache types managed by the AzAuth module.

The command scans the cache directory (by default `~/.IdentityService` on Unix-like systems or `%USERPROFILE%\.IdentityService` on Windows) and provides information about each cache:

- Cache name and path to the cache directory
- Creation and modification dates
- Number of accounts found including usernames (if -IncludeAccounts is specified)

Using the parameter -IncludeAccounts may prompt for access permissions to read account information from secure storage.

## EXAMPLES

### Example 1

```powershell
PS C:\> Get-AzTokenCache
```

Lists all available token caches with complete details including file information, but without account data.

### Example 2

```powershell
PS C:\> Get-AzTokenCache -IncludeAccounts
```

Lists all available token caches including account information. May prompt for access credentials to secure storage depending on the platform.

### Example 3

```powershell
PS C:\> Get-AzTokenCache -RootPath "C:\CustomRootCachePath"
```

Lists all available token caches in a custom directory, overriding the default cache directory.

## PARAMETERS

### -IncludeAccounts

When specified, includes account information such as account count and usernames stored in each cache. This may prompt for access credentials depending on the platform.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RootPath

Optional parameter to override the default cache directory path (at your own risk). By default, it uses the standard MSAL cache directory (`~/.IdentityService` on Linux and MacOS systems, or `%USERPROFILE%\.IdentityService` on Windows).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable, -ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### TokenCacheInfo[]

Returns an array of TokenCacheInfo objects containing information about each discovered cache.

## NOTES

- The command only discovers caches in the default MSAL cache directory
- Some cache information may not be available if the cache files are corrupted or inaccessible
- Account information is retrieved by attempting to initialize each cache, which may not succeed for all cache types

## RELATED LINKS

[Get-AzToken](Get-AzToken.md)
[Clear-AzTokenCache](Clear-AzTokenCache.md)
