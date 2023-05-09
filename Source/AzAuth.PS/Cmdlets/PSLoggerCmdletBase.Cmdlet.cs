using Microsoft.VisualStudio.Threading;
using System.Management.Automation;

namespace PipeHow.AzAuth;

public abstract partial class PSLoggerCmdletBase : PSCmdlet
{
    private protected CancellationTokenSource stopProcessing = new();
    private protected JoinableTaskFactory joinableTaskFactory = new(new JoinableTaskContext());

    // Set logger in TokenManager
    protected override void BeginProcessing()
    {
        TokenManager.Logger = this;
    }

    // Cancel any operations if user presses CTRL + C
    protected override void StopProcessing() => stopProcessing.Cancel();
}
