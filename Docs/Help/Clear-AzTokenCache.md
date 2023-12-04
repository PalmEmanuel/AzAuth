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
Clear-AzTokenCache -TokenCache <String> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

Clear all tokens from a specified token cache. The file may remain on disk, but without any tokens.

## EXAMPLES

### Example 1

```powershell
PS C:\> Clear-AzTokenCache -TokenCache 'MyCache'
```

Clears all tokens from the cache named "MyCache".

## PARAMETERS

### -TokenCache

The name of the token cache to clear.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### None

## NOTES

## RELATED LINKS
