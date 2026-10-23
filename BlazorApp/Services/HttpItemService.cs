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
        var fileList = files?.ToList() ?? new List<IBrowserFile>();
        
        await ValidateFilesAsync(fileList);

        using var content = new MultipartFormDataContent();
        AddDtoFields(content, itemDto);
        AddFilesToContent(content, fileList);

        HttpResponseMessage response = await HttpClient.PostAsync("api/item", content);
        
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create item: {error}");
        }

        return await response.Content.ReadFromJsonAsync<ItemDto>() ??
               throw new InvalidOperationException("Failed to deserialize created item response.");
    }

    public async Task<ItemDto> UpdateItemAsync(int id, ItemDto itemDto, IEnumerable<IBrowserFile> files)
    {
        var fileList = files?.ToList() ?? new List<IBrowserFile>();
        
        await ValidateFilesAsync(fileList);

        using var content = new MultipartFormDataContent();
        AddDtoFields(content, itemDto);
        AddFilesToContent(content, fileList);

        HttpResponseMessage response = await HttpClient.PutAsync($"api/item/{id}", content);
        
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

    private void AddDtoFields(MultipartFormDataContent content, ItemDto itemDto)
    {
        content.Add(new StringContent(itemDto.Name ?? string.Empty), "name");
        content.Add(new StringContent(itemDto.Description ?? string.Empty), "description");
        content.Add(new StringContent(itemDto.Price.ToString()), "price");
        content.Add(new StringContent(itemDto.Keyword ?? string.Empty), "keyword");
    }

    private void AddFilesToContent(MultipartFormDataContent content, List<IBrowserFile> files)
    {
        int count = 0;
        foreach (var file in files)
        {
            if (count >= 4) break;

            var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            var fileContent = new StreamContent(stream);
            
            if (!string.IsNullOrEmpty(file.ContentType))
            {
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            }

            content.Add(fileContent, "photos", file.Name);
            count++;
        }
    }

    private async Task ValidateFilesAsync(List<IBrowserFile> files)
    {
        if (files.Count > 4)
        {
            throw new ArgumentException("You can upload a maximum of 4 pictures.");
        }

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.Name).ToLowerInvariant();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            {
                throw new ArgumentException($"File {file.Name} must be in .png or .jpeg filetype.");
            }

            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var image = await Image.LoadAsync(stream);

            int width = image.Width;
            int height = image.Height;

            if (width < 600 || height < 600 || width > 2048 || height > 2048)
            {
                throw new ArgumentException($"Picture {file.Name} resolution must be between 600x600 and 2048x2048 pixels. Current: {width}x{height}");
            }
        }
    }
}