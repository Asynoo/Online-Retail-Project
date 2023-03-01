using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> _repository;

        public ProductsController(IRepository<Product> repos)
        {
            _repository = repos;
        }

        // GET products
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return _repository.GetAll();
        }

        // GET products/5
        [HttpGet("{id}", Name="GetProduct")]
        public IActionResult Get(int id)
        {
            var item = _repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST products
        [HttpPost]
        public IActionResult Post([FromBody]Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            var newProduct = _repository.Add(product);

            return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
        }

        // PUT products/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]Product product)
        {
            if (product == null || product.Id != id)
            {
                return BadRequest();
            }

            var modifiedProduct = _repository.Get(id);

            if (modifiedProduct == null)
            {
                return NotFound();
            }

            modifiedProduct.Name = product.Name;
            modifiedProduct.Price = product.Price;
            modifiedProduct.ItemsInStock = product.ItemsInStock;
            modifiedProduct.ItemsReserved = product.ItemsReserved;

            _repository.Edit(modifiedProduct);
            return new NoContentResult();
        }

        // DELETE products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (_repository.Get(id) == null)
            {
                return NotFound();
            }

            _repository.Remove(id);
            return new NoContentResult();
        }
    }
}
