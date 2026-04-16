using System.Net.Http.Headers;
using System.Net.Http.Json;
using PhysOn.Api.IntegrationTests.Infrastructure;
using PhysOn.Contracts.Auth;
using PhysOn.Contracts.Common;
using PhysOn.Contracts.Conversations;

namespace PhysOn.Api.IntegrationTests;

public sealed class VerticalSliceTests : IClassFixture<PhysOnApiFactory>
{
    private readonly PhysOnApiFactory _factory;

    public VerticalSliceTests(PhysOnApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_bootstrap_and_message_flow_work()
    {
        using var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/v1/auth/register/alpha-quick",
            new RegisterAlphaQuickRequest(
                "이안",
                "ALPHA-OPEN-2026",
                new DeviceRegistrationDto(Guid.NewGuid().ToString(), "windows", "Windows PC", "0.1.0")));

        registerResponse.EnsureSuccessStatusCode();

        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<RegisterAlphaQuickResponse>>();
        Assert.NotNull(registerPayload);
        Assert.Equal("이안", registerPayload!.Data.Account.DisplayName);
        Assert.NotEmpty(registerPayload.Data.Bootstrap.Conversations.Items);

        var accessToken = registerPayload.Data.Tokens.AccessToken;
        var selfConversationId = registerPayload.Data.Bootstrap.Conversations.Items
            .Single(x => x.Type == "self")
            .ConversationId;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var bootstrapPayload = await client.GetFromJsonAsync<ApiEnvelope<BootstrapResponse>>("/v1/bootstrap");
        Assert.NotNull(bootstrapPayload);
        Assert.Equal(registerPayload.Data.Account.UserId, bootstrapPayload!.Data.Me.UserId);

        var conversationsPayload = await client.GetFromJsonAsync<ApiEnvelope<ListEnvelope<ConversationSummaryDto>>>("/v1/conversations");
        Assert.NotNull(conversationsPayload);
        Assert.NotEmpty(conversationsPayload!.Data.Items);

        var postMessageResponse = await client.PostAsJsonAsync(
            $"/v1/conversations/{selfConversationId}/messages",
            new PostTextMessageRequest(Guid.NewGuid(), "첫 메시지"));

        postMessageResponse.EnsureSuccessStatusCode();
        var messagePayload = await postMessageResponse.Content.ReadFromJsonAsync<ApiEnvelope<MessageItemDto>>();
        Assert.NotNull(messagePayload);
        Assert.Equal("첫 메시지", messagePayload!.Data.Text);

        var messagesPayload = await client.GetFromJsonAsync<ApiEnvelope<ListEnvelope<MessageItemDto>>>(
            $"/v1/conversations/{selfConversationId}/messages");

        Assert.NotNull(messagesPayload);
        Assert.Contains(messagesPayload!.Data.Items, x => x.Text == "첫 메시지");

        var readCursorResponse = await client.PostAsJsonAsync(
            $"/v1/conversations/{selfConversationId}/read-cursor",
            new UpdateReadCursorRequest(messagePayload.Data.ServerSequence));

        readCursorResponse.EnsureSuccessStatusCode();
        var readCursorPayload = await readCursorResponse.Content.ReadFromJsonAsync<ApiEnvelope<ReadCursorUpdatedDto>>();
        Assert.NotNull(readCursorPayload);
        Assert.Equal(messagePayload.Data.ServerSequence, readCursorPayload!.Data.LastReadSequence);
    }

    [Fact]
    public async Task Posting_same_client_request_id_is_idempotent()
    {
        using var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/v1/auth/register/alpha-quick",
            new RegisterAlphaQuickRequest(
                "테스터",
                "ALPHA-OPEN-2026",
                new DeviceRegistrationDto(Guid.NewGuid().ToString(), "windows", "Windows PC", "0.1.0")));

        registerResponse.EnsureSuccessStatusCode();
        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<RegisterAlphaQuickResponse>>();
        Assert.NotNull(registerPayload);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerPayload!.Data.Tokens.AccessToken);
        var conversationId = registerPayload.Data.Bootstrap.Conversations.Items.Single(x => x.Type == "self").ConversationId;
        var request = new PostTextMessageRequest(Guid.NewGuid(), "중복 방지");

        var first = await client.PostAsJsonAsync($"/v1/conversations/{conversationId}/messages", request);
        var second = await client.PostAsJsonAsync($"/v1/conversations/{conversationId}/messages", request);

        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();

        var firstPayload = await first.Content.ReadFromJsonAsync<ApiEnvelope<MessageItemDto>>();
        var secondPayload = await second.Content.ReadFromJsonAsync<ApiEnvelope<MessageItemDto>>();

        Assert.NotNull(firstPayload);
        Assert.NotNull(secondPayload);
        Assert.Equal(firstPayload!.Data.MessageId, secondPayload!.Data.MessageId);
    }

    [Fact]
    public async Task Auth_and_bootstrap_responses_are_marked_no_store()
    {
        using var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/v1/auth/register/alpha-quick",
            new RegisterAlphaQuickRequest(
                "보안테스터",
                "ALPHA-OPEN-2026",
                new DeviceRegistrationDto(Guid.NewGuid().ToString(), "windows", "Windows PC", "0.1.0")));

        registerResponse.EnsureSuccessStatusCode();
        Assert.True(registerResponse.Headers.CacheControl?.NoStore ?? false);

        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<RegisterAlphaQuickResponse>>();
        Assert.NotNull(registerPayload);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerPayload!.Data.Tokens.AccessToken);

        var bootstrapResponse = await client.GetAsync("/v1/bootstrap");
        bootstrapResponse.EnsureSuccessStatusCode();
        Assert.True(bootstrapResponse.Headers.CacheControl?.NoStore ?? false);
    }

    [Fact]
    public async Task Protected_reads_are_marked_no_store()
    {
        using var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/v1/auth/register/alpha-quick",
            new RegisterAlphaQuickRequest(
                "캐시테스터",
                "ALPHA-OPEN-2026",
                new DeviceRegistrationDto(Guid.NewGuid().ToString(), "windows", "Windows PC", "0.1.0")));

        registerResponse.EnsureSuccessStatusCode();
        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<RegisterAlphaQuickResponse>>();
        Assert.NotNull(registerPayload);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerPayload!.Data.Tokens.AccessToken);
        var conversationId = registerPayload.Data.Bootstrap.Conversations.Items.Single(x => x.Type == "self").ConversationId;

        var meResponse = await client.GetAsync("/v1/me");
        meResponse.EnsureSuccessStatusCode();
        Assert.True(meResponse.Headers.CacheControl?.NoStore ?? false);

        var conversationsResponse = await client.GetAsync("/v1/conversations");
        conversationsResponse.EnsureSuccessStatusCode();
        Assert.True(conversationsResponse.Headers.CacheControl?.NoStore ?? false);

        var messagesResponse = await client.GetAsync($"/v1/conversations/{conversationId}/messages");
        messagesResponse.EnsureSuccessStatusCode();
        Assert.True(messagesResponse.Headers.CacheControl?.NoStore ?? false);
    }

    [Fact]
    public async Task Refresh_without_token_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/auth/token/refresh", new RefreshTokenRequest(string.Empty));

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
