using ProductClient.Models;

namespace ProductClient.Services;

public interface IProductApiService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<(bool Success, string? ErrorMessage)> CreateAsync(Product product);
    Task<(bool Success, string? ErrorMessage)> UpdateAsync(Product product);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
}
