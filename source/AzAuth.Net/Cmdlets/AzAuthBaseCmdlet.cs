using System.Management.Automation;

namespace PipeHow.AzAuth.Cmdlets;

public abstract class AzAuthBaseCmdlet : PSCmdlet
{
    private protected CancellationTokenSource stopProcessing = new();

    // Cancel any operations if user presses CTRL + C
    protected override void StopProcessing() => stopProcessing.Cancel();
}