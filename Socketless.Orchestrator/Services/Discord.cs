using Socketless.Orchestrator.Common;
using Socketless.Orchestrator.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Socketless.Orchestrator.Services;

public class Discord(IHttpClientFactory httpClientFactory) : IDiscord
{
    public async Task<AppScalingInformation> GetAppScalingInformationAsync(AppToken token)
    {
        using var client = httpClientFactory.CreateClient(); // TODO: Should this also use dedicated IP?

        client.BaseAddress = new Uri("https://discord.com");
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DiscordBot (https://github.com/leymbda/socketless, 0.0.1)"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", token.ToString());

        var applicationResponse = await client.GetAsync("/api/v10/applications/@me");
        var gatewayResponse = await client.GetAsync("/api/v10/gateway/bot");

        if (!applicationResponse.IsSuccessStatusCode || !gatewayResponse.IsSuccessStatusCode)
            throw new InvalidOperationException("Failed to retrieve application information from Discord API.");

        var applicationContent = await applicationResponse.Content.ReadAsStringAsync();
        var application = JsonDocument.Parse(applicationContent).RootElement;

        var gatewayContent = await gatewayResponse.Content.ReadAsStringAsync();
        var gateway = JsonDocument.Parse(gatewayContent).RootElement;


        var approximateGuildInstallCount = application.GetProperty("approximate_guild_count").GetInt32();
        var approximateUserInstallCount = application.GetProperty("approximate_user_install_count").GetInt32();
        var recommendedShardCount = gateway.GetProperty("shards").GetInt32();

        return new(approximateGuildInstallCount, approximateUserInstallCount, recommendedShardCount);
    }
}
