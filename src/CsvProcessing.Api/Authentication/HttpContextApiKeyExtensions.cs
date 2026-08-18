namespace CsvProcessing.Api.Authentication;

public static class HttpContextApiKeyExtensions
{
    private const string ApiKeyOwnerItemKey = "ApiKeyOwner";

    public static void SetApiKeyOwner(this HttpContext context, string owner)
        => context.Items[ApiKeyOwnerItemKey] = owner;

    public static string? GetApiKeyOwner(this HttpContext context)
        => context.Items.TryGetValue(ApiKeyOwnerItemKey, out var value) ? value as string : null;
}
