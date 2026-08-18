using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CsvProcessing.Api.Authentication;

public class ApiKeyValidator : IApiKeyValidator
{
    private readonly IOptionsMonitor<ApiKeyOptions> _options;

    public ApiKeyValidator(IOptionsMonitor<ApiKeyOptions> options) => _options = options;

    public bool TryValidate(string? presentedKey, [NotNullWhen(true)] out ApiKeyEntry? matchedKey)
    {
        matchedKey = null;

        if (string.IsNullOrWhiteSpace(presentedKey))
        {
            return false;
        }

        var presentedBytes = Encoding.UTF8.GetBytes(presentedKey);

        foreach (var candidate in _options.CurrentValue.Keys)
        {
            if (!candidate.Enabled || string.IsNullOrWhiteSpace(candidate.Key))
            {
                continue;
            }

            if (CryptographicOperations.FixedTimeEquals(presentedBytes, Encoding.UTF8.GetBytes(candidate.Key)))
            {
                matchedKey = candidate;
                return true;
            }
        }

        return false;
    }
}
