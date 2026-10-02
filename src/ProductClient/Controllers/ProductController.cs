using Microsoft.AspNetCore.Mvc;
using ProductClient.Models;
using ProductClient.Services;

namespace ProductClient.Controllers;

public class ProductController : Controller
{
    private readonly IProductApiService _apiService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductApiService apiService, ILogger<ProductController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // GET: /Product or /
    public async Task<IActionResult> Index()
    {
        var products = await _apiService.GetAllAsync();
        return View(products);
    }

    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _apiService.GetByIdAsync(id);
        if (product == null)
        {
            TempData["ErrorMessage"] = $"Товар с указанным ID ({id}) не найден.";
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    // GET: /Product/Create
    public IActionResult Create()
    {
        return View(new Product { Price = 1000, Quantity = 1 });
    }

    // POST: /Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        var (success, errorMessage) = await _apiService.CreateAsync(product);
        if (success)
        {
            TempData["SuccessMessage"] = $"Товар '{product.Name}' успешно добавлен в каталог!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, errorMessage ?? "Не удалось создать товар.");
        return View(product);
    }

    // GET: /Product/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _apiService.GetByIdAsync(id);
        if (product == null)
        {
            TempData["ErrorMessage"] = $"Товар с указанным ID ({id}) не найден.";
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    // POST: /Product/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        var (success, errorMessage) = await _apiService.UpdateAsync(product);
        if (success)
        {
            TempData["SuccessMessage"] = $"Товар '{product.Name}' успешно обновлен!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, errorMessage ?? "Не удалось обновить товар.");
        return View(product);
    }

    // POST: /Product/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _apiService.DeleteAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = $"Товар с ID {id} успешно удален!";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage ?? $"Не удалось удалить товар с ID {id}.";
        }

        return RedirectToAction(nameof(Index));
    }
}
