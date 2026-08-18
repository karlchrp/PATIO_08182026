using System.ComponentModel.DataAnnotations;

namespace CsvProcessing.Api.Authentication;

public class ApiKeyOptions
{
    public const string SectionName = "ApiKey";

    [Required(AllowEmptyStrings = false)]
    public string HeaderName { get; set; } = "X-Api-Key";

    [MinLength(1, ErrorMessage = "At least one API key must be configured.")]
    public IList<ApiKeyEntry> Keys { get; set; } = new List<ApiKeyEntry>();
}

public sealed class ApiKeyEntry
{
    [Required(AllowEmptyStrings = false)]
    public string Key { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Owner { get; set; } = "unknown";

    public bool Enabled { get; set; } = true;
}
