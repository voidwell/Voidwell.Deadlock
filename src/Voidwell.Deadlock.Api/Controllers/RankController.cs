using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;

namespace Voidwell.Deadlock.Api.Controllers;

[Route("deadlock/rank")]
[OutputCache(PolicyName = CachePolicies.ReferenceData)]
public class RankController : Controller
{
    private readonly IRankStore _rankStore;

    public RankController(IRankStore rankStore)
    {
        _rankStore = rankStore;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllRanksAsync()
    {
        var ranks = await _rankStore.GetAllRanks();
        return Ok(ranks);
    }
}
