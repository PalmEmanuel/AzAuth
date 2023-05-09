using Microsoft.Extensions.Logging;
using System.Management.Automation;

namespace PipeHow.AzAuth;

public abstract partial class PSLoggerCmdletBase : PSCmdlet, ILogger
{
    // Set logger in TokenManager
    protected override void BeginProcessing()
    {
        TokenManager.Logger = this;
    }
}
