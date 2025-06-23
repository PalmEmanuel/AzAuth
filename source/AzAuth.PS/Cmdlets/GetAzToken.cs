using System.Management.Automation;

namespace PipeHow.AzAuth.Cmdlets;

[Cmdlet(VerbsCommon.Get, "AzToken", DefaultParameterSetName = "NonInteractive")]
public class GetAzToken : AzAuthGetTokenCmdlet {}