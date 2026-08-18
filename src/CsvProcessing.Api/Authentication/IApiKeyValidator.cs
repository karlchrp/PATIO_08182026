using System.Diagnostics.CodeAnalysis;

namespace CsvProcessing.Api.Authentication;

public interface IApiKeyValidator
{
    bool TryValidate(string? presentedKey, [NotNullWhen(true)] out ApiKeyEntry? matchedKey);
}
