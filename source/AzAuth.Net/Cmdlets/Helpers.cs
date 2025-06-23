using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;

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

public class ValidateCertificatePathAttribute : ValidateArgumentsAttribute
{
    protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
    {
        var path = arguments as string;

        if (!Regex.Match(path!, "\\.(pfx|pem)$").Success)
        {
            throw new ArgumentException("Only .pfx and .pem files are supported!");
        }

        if (!File.Exists(path))
        {
            throw new ArgumentException($"File '{path}' does not exist.");
        }
    }
}