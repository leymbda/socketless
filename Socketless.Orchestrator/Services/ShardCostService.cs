namespace Socketless.Orchestrator.Services;

public static class ShardCostService
{
    public static float CalculateCost(int applicationGuildCount, int shardCount) =>
        (float)applicationGuildCount / shardCount / 150 + 1;
}
