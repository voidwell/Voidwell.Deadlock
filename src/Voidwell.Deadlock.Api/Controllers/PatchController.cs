using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;

namespace Voidwell.Deadlock.Api.Controllers;

[Route("deadlock/patch")]
[OutputCache(PolicyName = CachePolicies.ReferenceData)]
public class PatchController : Controller
{
    private readonly IPatchStore _patchStore;

    public PatchController(IPatchStore patchStore)
    {
        _patchStore = patchStore;
    }

    /// <summary>
    /// Known patches, newest first. The date is what the analytics endpoints take as their patch parameter.
    /// </summary>
    /// <param name="majorOnly">Only the curated major patch days.</param>
    [HttpGet]
    public async Task<ActionResult> GetPatchesAsync([FromQuery] bool majorOnly = false)
    {
        var patches = (await _patchStore.GetAllPatches() ?? [])
            .Where(a => !majorOnly || a.IsBigDay)
            .OrderByDescending(a => a.PublishedAt)
            .Select(a => new
            {
                Date = DateOnly.FromDateTime(a.PublishedAt.ToUniversalTime()),
                a.PublishedAt,
                a.Title,
                a.Source,
                a.Link,
                a.IsBigDay
            });

        return Ok(patches);
    }
}
