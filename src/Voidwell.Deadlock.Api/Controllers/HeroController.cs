using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;

namespace Voidwell.Deadlock.Api.Controllers;

[Route("deadlock/hero")]
[OutputCache(PolicyName = CachePolicies.ReferenceData)]
public class HeroController : Controller
{
    private readonly IHeroStore _heroStore;

    public HeroController(IHeroStore heroStore)
    {
        _heroStore = heroStore;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllHeroesAsync()
    {
        var heroes = await _heroStore.GetAllHeroes();
        return Ok(heroes);
    }

    [HttpGet("{heroId:int}")]
    public async Task<ActionResult> GetHeroAsync(int heroId)
    {
        var hero = await _heroStore.GetHero(heroId);
        if (hero == null)
        {
            return NotFound();
        }

        return Ok(hero);
    }
}
