using Microsoft.AspNetCore.Mvc;
using ProductClient.Models;

namespace ProductClient.Controllers.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Notebook", Description = "Ноутбук для работы и учебы", Price = 350000, Quantity = 5 },
        new Product { Id = 2, Name = "Mouse", Description = "Беспроводная мышь", Price = 8000, Quantity = 15 },
        new Product { Id = 3, Name = "Keyboard", Description = "Механическая клавиатура", Price = 15000, Quantity = 10 }
    };
    private static readonly object _lock = new();

    [HttpGet]
    public IActionResult GetAll()
    {
        lock (_lock)
        {
            return Ok(_products.ToList());
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { message = $"Товар с ID {id} не найден." });
            }
            return Ok(product);
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] Product product)
    {
        if (product == null || string.IsNullOrWhiteSpace(product.Name) || product.Price <= 0 || product.Quantity < 0)
        {
            return BadRequest(new { message = "Некорректные данные товара. Название обязательно, цена должна быть больше 0, количество не может быть отрицательным." });
        }

        lock (_lock)
        {
            product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Product updated)
    {
        if (updated == null || string.IsNullOrWhiteSpace(updated.Name) || updated.Price <= 0 || updated.Quantity < 0)
        {
            return BadRequest(new { message = "Некорректные данные для обновления товара." });
        }

        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { message = $"Товар с ID {id} не найден." });
            }

            product.Name = updated.Name;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.Quantity = updated.Quantity;

            return Ok(product);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { message = $"Товар с ID {id} не найден." });
            }

            _products.Remove(product);
            return Ok(new { message = $"Товар с ID {id} успешно удален." });
        }
    }
}
