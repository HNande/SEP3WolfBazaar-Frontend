using System.Net.Http.Json;
using BlazorApp.DTO;
using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp;

namespace BlazorApp.Services;

public class HttpItemService : IItemService
{
    public readonly HttpClient HttpClient;
    
    public HttpItemService(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public async Task<ItemDto> CreateItemAsync(ItemDto itemDto, IEnumerable<IBrowserFile> files)
    {
        itemDto.Album = await BuildAlbumAsync(itemDto.Name, files);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/item", itemDto);
        
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create item: {error}");
        }

        return await response.Content.ReadFromJsonAsync<ItemDto>() ?? itemDto;
    }

    public async Task<ItemDto> UpdateItemAsync(int id, ItemDto itemDto, IEnumerable<IBrowserFile> files)
    {
        itemDto.Album = await BuildAlbumAsync(itemDto.Name, files);

        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/item/{id}", itemDto);
        
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to update item {id}: {error}");
        }

        return await response.Content.ReadFromJsonAsync<ItemDto>() 
               ?? throw new InvalidOperationException("Failed to deserialize updated item response.");
    }

    public async Task DeleteItemAsync(int id)
    {
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/item/{id}");
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to delete item {id}: {error}");
        }
    }

    public async Task<IEnumerable<ItemDto>> GetItemsAsync()
    {
        return await HttpClient.GetFromJsonAsync<IEnumerable<ItemDto>>("api/item") 
               ?? Enumerable.Empty<ItemDto>();
    }

    public async Task<ItemDto> GetItemByIdAsync(int id)
    {
        return await HttpClient.GetFromJsonAsync<ItemDto>($"api/item/{id}") 
               ?? throw new KeyNotFoundException($"Item with ID {id} was not found.");
    }

    private async Task<AlbumDto> BuildAlbumAsync(string itemName, IEnumerable<IBrowserFile> files)
    {
        var fileList = files?.ToList() ?? new List<IBrowserFile>();

        if (fileList.Count > 4)
        {
            throw new ArgumentException("You can upload a maximum of 4 pictures.");
        }

        var album = new AlbumDto
        {
            Title = $"{itemName}album",
            Photos = new List<PhotoDto>()
        };

        foreach (var file in fileList)
        {
            var contentType = file.ContentType?.ToLowerInvariant();
            if (contentType != "image/png" && contentType != "image/jpeg" && contentType != "image/jpg")
            {
                throw new ArgumentException($"File {file.Name} must be in PNG or JPEG format.");
            }

            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            
            var fileBytes = memoryStream.ToArray();

            memoryStream.Position = 0;
            using var image = Image.Load(memoryStream);

            int width = image.Width;
            int height = image.Height;

            if (width <= 0 || height <= 0 || width > 2048 || height > 2048)
            {
                throw new ArgumentException($"Picture {file.Name} resolution must be up to 2048x2048 pixels. Current: {width}x{height}");
            }

            album.Photos.Add(new PhotoDto
            {
                Width = width,
                Height = height,
                Filetype = file.ContentType ?? "image/jpeg",
                Data = fileBytes
            });
        }

        return album;
    }
}
