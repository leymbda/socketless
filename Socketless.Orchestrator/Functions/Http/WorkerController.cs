using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Functions.Http;

public static class WorkerController
{
    [Function(nameof(WorkerList))]
    public static async Task<IActionResult> WorkerList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workers")] HttpRequest req)
    {
        return new StatusCodeResult(501);
    }

    [Function(nameof(WorkerGet))]
    public static async Task<IActionResult> WorkerGet(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workers/{workerId}")] HttpRequest req,
        [FromRoute] WorkerId workerId)
    {
        return new StatusCodeResult(501);
    }
}
