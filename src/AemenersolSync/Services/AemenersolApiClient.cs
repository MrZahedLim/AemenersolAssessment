using System.Net.Http.Json;
using System.Text.Json;
using AemenersolSync.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AemenersolSync.Services;

public class AemenersolApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<AemenersolApiClient> _logger;
    private readonly ApiSettings _settings;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AemenersolApiClient(
        HttpClient http,
        IOptions<ApiSettings> settings,
        ILogger<AemenersolApiClient> logger)
    {
        _http = http;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<string> LoginAsync(CancellationToken ct = default)
    {
        var request = new LoginRequest
        {
            Username = _settings.Username,
            Password = _settings.Password
        };

        _logger.LogInformation("Logging in as {Username}...", _settings.Username);

        using var response = await _http.PostAsJsonAsync("api/Account/Login", request, ct);
        response.EnsureSuccessStatusCode();

        // Token dipulangkan sebagai string JSON ber-quote, cth "eyJ..."
        var raw = await response.Content.ReadAsStringAsync(ct);
        var token = raw.Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Login succeeded but no token was returned.");

        _logger.LogInformation("Login successful, token acquired.");
        return token;
    }

    public Task<List<PlatformDto>> GetPlatformWellActualAsync(string token, CancellationToken ct = default)
        => GetPlatformWellAsync("api/PlatformWell/GetPlatformWellActual", token, ct);

    public Task<List<PlatformDto>> GetPlatformWellDummyAsync(string token, CancellationToken ct = default)
        => GetPlatformWellAsync("api/PlatformWell/GetPlatformWellDummy", token, ct);

    private async Task<List<PlatformDto>> GetPlatformWellAsync(string path, string token, CancellationToken ct)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, path);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        _logger.LogInformation("Calling {Path}...", path);

        using var response = await _http.SendAsync(message, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<List<PlatformDto>>(json, JsonOptions);
        var result = data ?? new List<PlatformDto>();

        _logger.LogInformation("Received {Count} platform(s) from {Path}.", result.Count, path);
        return result;
    }
}
