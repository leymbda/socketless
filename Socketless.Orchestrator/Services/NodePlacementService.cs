namespace Socketless.Orchestrator.Services;

public static class NodePlacementService
{
    public const int MaximumShards = 300;
    public const float InitialBalance = 1000;

    public static bool CanAccomodate(int currentShardCount, int incomingShardCount, float currentCost, float incomingCost) =>
        (currentShardCount + incomingShardCount) < MaximumShards && (currentCost + incomingCost) <= InitialBalance;
}
