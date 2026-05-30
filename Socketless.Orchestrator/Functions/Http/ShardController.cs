using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Functions.Http;

public static class ShardController
{
    [Function(nameof(ShardList))]
    public static async Task<IActionResult> ShardList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shards")] HttpRequest req,
        [FromQuery] WorkerId? workerId,
        [FromQuery] AppId? appId,
        [FromQuery] ClientId? clientId)
    {
        return new StatusCodeResult(501);
    }
}
