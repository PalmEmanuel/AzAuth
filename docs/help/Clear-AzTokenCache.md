---
external help file: AzAuth.PS.dll-Help.xml
Module Name: AzAuth
online version:
schema: 2.0.0
---

# Clear-AzTokenCache

## SYNOPSIS

Clear all tokens from a specified token cache.

## SYNTAX

```
Clear-AzTokenCache -TokenCache <String> [-Force] [-RootPath <String>]
 [<CommonParameters>]
```

## DESCRIPTION

Clear all tokens from a specified token cache. By default, this command uses MSAL's safe token removal methods to clear account data while preserving the cache file structure.

When the -Force parameter is specified, the command will delete the entire cache directory and all its files. This is more thorough but may cause issues with other applications that are using the same cache.

## EXAMPLES

### Example 1

```powershell
PS C:\> Clear-AzTokenCache -TokenCache 'MyCache'
```

Clears all tokens from the cache named "MyCache" using MSAL's safe removal methods. The cache directory and structure remain intact.

### Example 2

```powershell
PS C:\> Clear-AzTokenCache -TokenCache 'MyCache' -Force
```

Deletes the entire cache directory and all files for the cache named "MyCache", instead of only clearing tokens within. Use with caution as this may affect other applications.

## PARAMETERS

### -Force

Deletes the entire cache directory and all its files instead of just clearing tokens. This is more thorough but may cause issues with other applications using the same cache.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
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

### -TokenCache

The name of the token cache to clear.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Name

Required: True
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

### None

## NOTES

## RELATED LINKS
