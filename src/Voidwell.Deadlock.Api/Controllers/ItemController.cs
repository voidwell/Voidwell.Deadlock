using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;

namespace Voidwell.Deadlock.Api.Controllers;

[Route("deadlock/item")]
[OutputCache(PolicyName = CachePolicies.ReferenceData)]
public class ItemController : Controller
{
    private readonly IItemStore _itemStore;

    public ItemController(IItemStore itemStore)
    {
        _itemStore = itemStore;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllItemsAsync()
    {
        var items = await _itemStore.GetAllItems();
        return Ok(items);
    }

    [HttpGet("{itemId:long}")]
    public async Task<ActionResult> GetItemAsync(long itemId)
    {
        var item = await _itemStore.GetItem(itemId);
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}
