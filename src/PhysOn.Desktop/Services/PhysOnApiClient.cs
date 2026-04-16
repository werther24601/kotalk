using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PhysOn.Contracts.Auth;
using PhysOn.Contracts.Common;
using PhysOn.Contracts.Conversations;

namespace PhysOn.Desktop.Services;

public sealed class PhysOnApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<RegisterAlphaQuickResponse> RegisterAlphaQuickAsync(
        string apiBaseUrl,
        RegisterAlphaQuickRequest request,
        CancellationToken cancellationToken) =>
        SendAsync<RegisterAlphaQuickResponse>(
            apiBaseUrl,
            HttpMethod.Post,
            "/v1/auth/register/alpha-quick",
            null,
            request,
            cancellationToken);

    public Task<BootstrapResponse> GetBootstrapAsync(
        string apiBaseUrl,
        string accessToken,
        CancellationToken cancellationToken) =>
        SendAsync<BootstrapResponse>(
            apiBaseUrl,
            HttpMethod.Get,
            "/v1/bootstrap",
            accessToken,
            null,
            cancellationToken);

    public Task<ListEnvelope<MessageItemDto>> GetMessagesAsync(
        string apiBaseUrl,
        string accessToken,
        string conversationId,
        CancellationToken cancellationToken) =>
        SendAsync<ListEnvelope<MessageItemDto>>(
            apiBaseUrl,
            HttpMethod.Get,
            $"/v1/conversations/{conversationId}/messages",
            accessToken,
            null,
            cancellationToken);

    public Task<MessageItemDto> SendTextMessageAsync(
        string apiBaseUrl,
        string accessToken,
        string conversationId,
        PostTextMessageRequest request,
        CancellationToken cancellationToken) =>
        SendAsync<MessageItemDto>(
            apiBaseUrl,
            HttpMethod.Post,
            $"/v1/conversations/{conversationId}/messages",
            accessToken,
            request,
            cancellationToken);

    public Task<ReadCursorUpdatedDto> UpdateReadCursorAsync(
        string apiBaseUrl,
        string accessToken,
        string conversationId,
        UpdateReadCursorRequest request,
        CancellationToken cancellationToken) =>
        SendAsync<ReadCursorUpdatedDto>(
            apiBaseUrl,
            HttpMethod.Post,
            $"/v1/conversations/{conversationId}/read-cursor",
            accessToken,
            request,
            cancellationToken);

    private static async Task<T> SendAsync<T>(
        string apiBaseUrl,
        HttpMethod method,
        string path,
        string? accessToken,
        object? body,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(EnsureTrailingSlash(apiBaseUrl)),
            Timeout = TimeSpan.FromSeconds(20)
        };

        using var request = new HttpRequestMessage(method, path.TrimStart('/'));
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await client.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = JsonSerializer.Deserialize<ApiErrorEnvelope>(payload, JsonOptions);
            throw new InvalidOperationException(error?.Error.Message ?? $"요청이 실패했습니다. ({response.StatusCode})");
        }

        var envelope = JsonSerializer.Deserialize<ApiEnvelope<T>>(payload, JsonOptions);
        if (envelope is null)
        {
            throw new InvalidOperationException("서버 응답을 읽지 못했습니다.");
        }

        return envelope.Data;
    }

    private static string EnsureTrailingSlash(string apiBaseUrl) =>
        apiBaseUrl.EndsWith("/", StringComparison.Ordinal) ? apiBaseUrl : $"{apiBaseUrl}/";
}
