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
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] [-TenantId <String>] [-Claim <String>] [-Force]
 [<CommonParameters>]
```

### Cache
```
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] [-TenantId <String>] [-Claim <String>]
 [-ClientId <String>] -TokenCache <String> -Username <String>
 [<CommonParameters>]
```

### Interactive
```
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] [-TenantId <String>] [-Claim <String>]
 [-ClientId <String>] [-TokenCache <String>] [-TimeoutSeconds <Int32>] [-Interactive] [-Force]
 [<CommonParameters>]
```

### DeviceCode
```
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] [-TenantId <String>] [-Claim <String>]
 [-ClientId <String>] [-TokenCache <String>] [-TimeoutSeconds <Int32>] [-DeviceCode] [-Force]
 [<CommonParameters>]
```

### ManagedIdentity
```
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] [-TenantId <String>] [-Claim <String>]
 [-ClientId <String>] [-ManagedIdentity] [-Force] [<CommonParameters>]
```

### WorkloadIdentity
```
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] -TenantId <String> [-Claim <String>]
 -ClientId <String> [-WorkloadIdentity] -ExternalToken <String> [-Force]
 [<CommonParameters>]
```

### ClientSecret
```
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] -TenantId <String> [-Claim <String>]
 -ClientId <String> -ClientSecret <String> [-Force] [<CommonParameters>]
```

### ClientCertificate
```
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] -TenantId <String> [-Claim <String>]
 -ClientId <String> -ClientCertificate <X509Certificate2> [-Force]
 [<CommonParameters>]
```

### ClientCertificatePath
```
Get-AzToken [[-Resource] <String>] [[-Scope] <String[]>] -TenantId <String> [-Claim <String>]
 -ClientId <String> -ClientCertificatePath <String> [-Force]
 [<CommonParameters>]
```

## DESCRIPTION

Gets a new Azure access token.

The token can be retrieved from an existing named cache, interactively from a browser, or non-interactively with specific token sources. If the command is used non-interactively, an attempt will be made to get a token using the following sources in order:

- Saved interactive credential if the command was used interactively in the same session (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.interactivebrowsercredential)
- Environment variables (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential)
- Azure PowerShell (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azurepowershellcredential)
- Azure CLI (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azureclicredential)
- Visual Studio Code (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocodecredential)
- Visual Studio (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocredential)
- Shared token cache (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.sharedtokencachecredential)

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
PS C:\> Get-AzToken -Interactive -TokenCache 'AzAuthCache'
```

Gets a new Azure access token interactively and stores the token in a new (or existing) token cache named "AzAuthCache".

### Example 4

```powershell
PS C:\> Get-AzToken -TokenCache 'AzAuthCache'
```

Gets a new Azure access token interactively from an existing token cache named "AzAuthCache".

### Example 5

```powershell
PS C:\> Get-AzToken -Scope 'Directory.Read.All' -ClientId $ClientId -ManagedIdentity
```

Gets a new Azure access token for a managed identity, valid for Microsoft Graph with the scope `Directory.Read.All`, also specifying a client id.

### Example 6

```powershell
PS C:\> Get-AzToken -ClientId $ClientId -ClientSecret $ClientSecret -TenantId $TenantId
```

Gets a new Azure access token for a client using the client credentials flow by specifying a client secret, valid for the default Microsoft Graph scope, also specifying the tenant as a mandatory parameter.

### Example 7

```powershell
PS C:\> Get-AzToken -ClientCertificate (Get-Item "Cert:\CurrentUser\My\$Thumbprint") -ClientId $ClientId -TenantId $TenantId
```

Gets a new Azure access token for a client using the client certificate flow by getting and providing an installed certificate from the user certificate store.

### Example 8

```powershell
PS C:\> Get-AzToken -ClientCertificatePath ".\certAndPrivateKey.pem" -ClientId $ClientId -TenantId $TenantId
```

Gets a new Azure access token for a client using the client certificate flow by specifying a path to a file containing both the certificate and the private key.

### Example 9

```powershell
PS C:\> Get-AzToken -WorkloadIdentity -ExternalToken $OidcToken -ClientId $ClientId -TenantId $TenantId
```

Gets a new Azure access token for a client using the workload identity federation pattern by specifying a valid id token. For more details, see blog post in related links of this command.

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

### -ClientCertificate

The certificate to be used for getting a token with the client certificate flow.

```yaml
Type: X509Certificate2
Parameter Sets: ClientCertificate
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientCertificatePath

The path to a file containing both the certificate and private key, used for getting a token with the client certificate flow.

```yaml
Type: String
Parameter Sets: ClientCertificatePath
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientId

The client id of the application used to authenticate the user or identity. If not specified the user will be authenticated with an Azure development application.

```yaml
Type: String
Parameter Sets: Cache, Interactive, DeviceCode, ManagedIdentity
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: WorkloadIdentity, ClientSecret, ClientCertificate, ClientCertificatePath
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientSecret

The client secret to use for getting a token with the client credentials flow.

```yaml
Type: String
Parameter Sets: ClientSecret
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DeviceCode

Get a token using a device code login interactively, for example on a different device.

```yaml
Type: SwitchParameter
Parameter Sets: DeviceCode
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExternalToken

The external token used for the federated credential of the workload identity, used together with parameter -WorkloadIdentity for the client assertion flow. For more details, see blog post in related links of this command.

```yaml
Type: String
Parameter Sets: WorkloadIdentity
Aliases:

Required: True
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
Parameter Sets: NonInteractive, Interactive, DeviceCode, ManagedIdentity, WorkloadIdentity, ClientSecret, ClientCertificate, ClientCertificatePath
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
Position: 0
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
Position: 1
Default value: .default
Accept pipeline input: False
Accept wildcard characters: False
```

### -TenantId

The id of the tenant that the token should be valid for.

```yaml
Type: String
Parameter Sets: NonInteractive, Cache, Interactive, DeviceCode, ManagedIdentity
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: WorkloadIdentity, ClientSecret, ClientCertificate, ClientCertificatePath
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeoutSeconds

The number of seconds to wait until the login times out.

```yaml
Type: Int32
Parameter Sets: Interactive, DeviceCode
Aliases:

Required: False
Position: Named
Default value: 120
Accept pipeline input: False
Accept wildcard characters: False
```

### -TokenCache

The name of the token cache to get the token from, or to store the interactively retrieved token in.

```yaml
Type: String
Parameter Sets: Cache
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Interactive, DeviceCode
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username

The username to get the token for in the named cache.

```yaml
Type: String
Parameter Sets: Cache
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WorkloadIdentity

Get a token using a federated credential, or "workload identity federation". For an example of how to use this in a pipeline, see related links of this command.

```yaml
Type: SwitchParameter
Parameter Sets: WorkloadIdentity
Aliases:

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

### System.Object

## NOTES

## RELATED LINKS

[Blog Post "OAuth 2.0 Fundamentals for Azure APIs"](https://pipe.how/connect-azure/)

[Blog Post "Azure Workload Identity Federation"](https://pipe.how/get-oidctoken/)
