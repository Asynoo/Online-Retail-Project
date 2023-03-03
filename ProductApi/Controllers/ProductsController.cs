using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;
namespace ProductApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase {
        private readonly IRepository<Product> _repository;

        public ProductsController(IRepository<Product> repos) {
            _repository = repos;
        }

        // GET products
        [HttpGet]
        public async Task<IEnumerable<Product>> Get() {
            return await _repository.GetAll();
        }

        // GET products/5
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get(int id) {
            Product? item = await _repository.Get(id);
            if (item == null) {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST products
        [HttpPost]
        public IActionResult Post([FromBody] Product product) {

            Task<Product> newProduct = _repository.Add(product);

            return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
        }

        // PUT products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product) {
            if (product.Id != id) {
                return BadRequest();
            }

            Product? modifiedProduct = await _repository.Get(id);

            if (modifiedProduct == null) {
                return NotFound($"Product with id: {id} does not exist");
            }

            modifiedProduct.Name = product.Name;
            modifiedProduct.Price = product.Price;
            modifiedProduct.ItemsInStock = product.ItemsInStock;
            modifiedProduct.ItemsReserved = product.ItemsReserved;

            bool editSuccess = await _repository.Edit(modifiedProduct);
            return editSuccess ? new OkResult() : new BadRequestResult();
        }

        // DELETE products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) {
            if (await _repository.Get(id) is null) {
                return NotFound($"Product with id: {id} does not exist");
            }

            bool deleteSuccess = await _repository.Remove(id);
            return deleteSuccess ? new OkResult() : new BadRequestResult();
        }
    }
}
