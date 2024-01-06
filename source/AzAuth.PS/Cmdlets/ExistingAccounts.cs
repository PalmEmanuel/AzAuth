using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PipeHow.AzAuth;

public class ExistingAccounts : IArgumentCompleter
{
    public IEnumerable<CompletionResult> CompleteArgument(
        string commandName,
        string parameterName,
        string wordToComplete,
        CommandAst commandAst,
        IDictionary fakeBoundParameters)
    {
        IEnumerable<CompletionResult> results = Enumerable.Empty<CompletionResult>();

        try
        {
            return CacheManager.GetAccounts(fakeBoundParameters["TokenCache"]?.ToString()).Select(a => new CompletionResult(a));
        }
        catch (Exception) { /* It's fine if we cannot get accounts here */ }

        return results;
    }
}
