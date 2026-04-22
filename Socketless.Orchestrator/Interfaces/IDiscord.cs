using Socketless.Orchestrator.Common;

namespace Socketless.Orchestrator.Interfaces;

public interface IDiscord
{
    Task<AppScalingInformation> GetAppScalingInformationAsync(AppToken token);
}

public record AppScalingInformation(
    int ApproximateGuildInstallCount,
    int ApproximateUserInstallCount,
    int RecommendedShardCount);
