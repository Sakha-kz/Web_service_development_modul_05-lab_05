using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProductClient.Models;

namespace ProductClient.Services;

public class ProductApiService : IProductApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductApiService(IHttpClientFactory httpClientFactory, ILogger<ProductApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("ProductApi");

    public async Task<List<Product>> GetAllAsync()
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync("api/products");

            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<List<Product>>(_jsonOptions);
                return products ?? new List<Product>();
            }

            _logger.LogWarning("Failed to retrieve products. StatusCode: {StatusCode}", response.StatusCode);
            return new List<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching all products from Web API");
            return new List<Product>();
        }
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Product>(_jsonOptions);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found (404)", id);
                return null;
            }

            _logger.LogWarning("Error fetching product ID {ProductId}. StatusCode: {StatusCode}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching product with ID {ProductId}", id);
            return null;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateAsync(Product product)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("api/products", product);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            var errorMsg = response.StatusCode switch
            {
                HttpStatusCode.BadRequest => $"Некорректные данные товара (400): {errorBody}",
                HttpStatusCode.InternalServerError => "Внутренняя ошибка сервера API (500)",
                _ => $"Ошибка API ({response.StatusCode}): {errorBody}"
            };

            _logger.LogWarning("Failed to create product. StatusCode: {StatusCode}, Message: {Message}", response.StatusCode, errorMsg);
            return (false, errorMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating product");
            return (false, $"Сетевая ошибка при обращении к Web API: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(Product product)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"api/products/{product.Id}", product);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            var errorMsg = response.StatusCode switch
            {
                HttpStatusCode.NotFound => "Товар не найден (404)",
                HttpStatusCode.BadRequest => $"Некорректные данные товара (400): {errorBody}",
                HttpStatusCode.InternalServerError => "Внутренняя ошибка сервера API (500)",
                _ => $"Ошибка API ({response.StatusCode}): {errorBody}"
            };

            _logger.LogWarning("Failed to update product ID {ProductId}. StatusCode: {StatusCode}", product.Id, response.StatusCode);
            return (false, errorMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while updating product ID {ProductId}", product.Id);
            return (false, $"Сетевая ошибка при обращении к Web API: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (false, "Товар с указанным ID не найден (404)");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            return (false, $"Ошибка при удалении ({response.StatusCode}): {errorBody}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while deleting product with ID {ProductId}", id);
            return (false, $"Сетевая ошибка при обращении к Web API: {ex.Message}");
        }
    }
}
