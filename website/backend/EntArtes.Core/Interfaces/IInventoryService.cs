using EntArtes.Core.DTOs;
using EntArtes.Core.Entities;

namespace EntArtes.Core.Interfaces;

public interface IInventoryService
{
	Task<Item> SubmitItemAsync(int contribuidorId, ItemSubmissionDto dto);
	Task<Item?> GetItemByIdAsync(int id);
	Task ApproveItemAsync(int itemId);
	Task RejectItemAsync(int itemId);
	Task<IEnumerable<Item>> GetPendingItemsAsync();
	Task<IEnumerable<Item>> GetAvailableItemsAsync();
}