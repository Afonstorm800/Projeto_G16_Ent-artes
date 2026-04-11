using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EntArtes.Core.DTOs;
using EntArtes.Core.Interfaces;

namespace EntArtes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventory;

    public InventoryController(IInventoryService inventory)
    {
        _inventory = inventory;
    }

    [HttpPost("items")]
    public async Task<IActionResult> SubmitItem(ItemSubmissionDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var item = await _inventory.SubmitItemAsync(userId, dto);
        return Ok(item);
    }

    [HttpGet("items/available")]
    public async Task<IActionResult> GetAvailableItems()
    {
        var items = await _inventory.GetAvailableItemsAsync();
        return Ok(items);
    }

    [HttpGet("items/pending")]
    [Authorize(Roles = "Direcao")]
    public async Task<IActionResult> GetPendingItems()
    {
        var items = await _inventory.GetPendingItemsAsync();
        return Ok(items);
    }

    [HttpPost("items/{id}/approve")]
    [Authorize(Roles = "Direcao")]
    public async Task<IActionResult> ApproveItem(int id)
    {
        await _inventory.ApproveItemAsync(id);
        return Ok();
    }

    [HttpPost("items/{id}/reject")]
    [Authorize(Roles = "Direcao")]
    public async Task<IActionResult> RejectItem(int id)
    {
        await _inventory.RejectItemAsync(id);
        return Ok();
    }
}