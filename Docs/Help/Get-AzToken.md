---
external help file: AzAuth.PS.dll-Help.xml
Module Name: AzAuth
online version:
schema: 2.0.0
---

# Get-AzToken

## SYNOPSIS

Gets a new Azure access token.

## SYNTAX

### NonInteractive (Default)
```
Get-AzToken [-Resource <String>] [-Scope <String[]>] [-TenantId <String>] [-Claim <String>] [-Force]
 [<CommonParameters>]
```

### Interactive
```
Get-AzToken [-Resource <String>] [-Scope <String[]>] [-TenantId <String>] [-Claim <String>]
 [-ClientId <String>] [-Interactive] [-Force] [<CommonParameters>]
```

### ManagedIdentity
```
Get-AzToken [-Resource <String>] [-Scope <String[]>] [-TenantId <String>] [-Claim <String>]
 [-ClientId <String>] [-ManagedIdentity] [-Force] [<CommonParameters>]
```

## DESCRIPTION

Gets a new Azure access token.

If the command is used non-interactively, an attempt will be made to get a token using the following sources in order:

- Saved interactive credential if the command was used interactively in the same session (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.interactivebrowsercredential)
- Environment variables (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential)
- Azure PowerShell (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.azurepowershellcredential)
- Azure CLI (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.azureclicredential)
- Visual Studio Code (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocodecredential)
- Visual Studio (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocredential)

## EXAMPLES

### Example 1

```powershell
PS C:\> Get-AzToken
```

Gets a new Azure access token non-interactively for the `.default` scope of Microsoft Graph.

### Example 2

```powershell
PS C:\> Get-AzToken -Resource 'https://graph.microsoft.com/' -Scope 'User.Read','LearningContent.Read.All' -ClientId 'a4d5d049-a35c-49a1-ad6e-0a3a94138d32' -Interactive
```

Gets a new Azure access token interactively for Microsoft Graph with the scopes `User.Read` and `LearningContent.Read.All`, also specifying a client id.

### Example 3

```powershell
PS C:\> Get-AzToken -Scope 'Directory.Read.All' -ClientId '0b279d62-06f2-4175-b008-d9efd0e4f4d3' -ManagedIdentity
```

Gets a new Azure access token for a managed identity, valid for for Microsoft Graph with the scope `Directory.Read.All`, also specifying a client id.

## PARAMETERS

### -Claim

Additional claims to be included in the token.

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

### -ClientId

The client id of the application used to authenticate the user or identity. If not specified the user will be authenticated with an Azure development application.

```yaml
Type: String
Parameter Sets: Interactive, ManagedIdentity
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Force

Disregard any previous authentication made in this session.

This may be required when combining interactive and non-interactive authentication towards different tenants.

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

### -Interactive

Get a token using an interactive browser.

The authentication record will be saved during the session and used as the first option for a token if the command is used again but non-interactively.

```yaml
Type: SwitchParameter
Parameter Sets: Interactive
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ManagedIdentity

Get a token using a managed identity assigned to the environment, such as Azure VMs, App Service or Azure Functions applications.

```yaml
Type: SwitchParameter
Parameter Sets: ManagedIdentity
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Resource

The resource for the token, such as Microsoft Graph or Azure Key Vault. This can be provided either as a URI or as an id.

If not specified, the resource will be set to `https://graph.microsoft.com`.

```yaml
Type: String
Parameter Sets: (All)
Aliases: ResourceId, ResourceUrl

Required: False
Position: Named
Default value: https://graph.microsoft.com
Accept pipeline input: False
Accept wildcard characters: False
```

### -Scope

One or several scopes for the token, in the context of the provided resource.

If not specified, the scope will be set to `.default`.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: .default
Accept pipeline input: False
Accept wildcard characters: False
```

### -TenantId

The id of the tenant that the token should be valid for.

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
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
