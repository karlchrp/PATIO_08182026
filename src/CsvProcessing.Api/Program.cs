using CsvProcessing.Api.Authentication;
using CsvProcessing.Api.Middleware;
using CsvProcessing.Application.Csv;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<ApiKeyOptions>()
    .Bind(builder.Configuration.GetSection(ApiKeyOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => options.Keys.Any(key => key.Enabled && !string.IsNullOrWhiteSpace(key.Key)),
        "ApiKey:Keys must contain at least one enabled key with a non-empty value.")
    .Validate(
        options => options.Keys.All(key => string.IsNullOrWhiteSpace(key.Key) || key.Key.Length >= 16),
        "Every configured API key must be at least 16 characters long.")
    .ValidateOnStart();

builder.Services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddSingleton<ICsvFileProcessor, CsvFileProcessor>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.UseMiddleware<ApiKeyMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
