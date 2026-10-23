using BlazorApp.DTO;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Services;

public interface IItemService
{
    Task<ItemDto> CreateItemAsync(ItemDto itemDto, IEnumerable<IBrowserFile> files);
    Task<ItemDto> UpdateItemAsync(int id, ItemDto itemDto, IEnumerable<IBrowserFile> files);
    Task DeleteItemAsync(int id);
    Task<IEnumerable<ItemDto>> GetItemsAsync();
    Task<ItemDto> GetItemByIdAsync(int id);
}