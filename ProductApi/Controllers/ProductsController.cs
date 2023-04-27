using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Models;
using SharedModels;
namespace ProductApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase {
        private readonly IConverter<Product, ProductDto> _productConverter;
        private readonly IServiceGateway<ReviewDto> _reviewGateway;
        private readonly IRepository<Product> _repository;

        public ProductsController(
            IConverter<Product, ProductDto> converter,
            IServiceGateway<ReviewDto> reviewGateway,
            IRepository<Product> repository
            ) {
            _productConverter = converter;
            _reviewGateway = reviewGateway;
            _repository = repository;
        }

        // GET products
        [HttpGet]
        public async Task<IActionResult> GetAll() {
            List<Product> products = (await _repository.GetAll()).ToList();
            if (!products.Any()) {
                return NoContent();
            }
            IEnumerable<ProductDto> productDtoList = products.Select(x => _productConverter.Convert(x));
            return Ok(productDtoList);
        }

        // GET products/5
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get(int id) {
            Product? item = await _repository.Get(id);
            if (item == null) {
                return NotFound();
            }
            ProductDto productDto = _productConverter.Convert(item);
            productDto.Reviews = await _reviewGateway.GetForProduct(item.Id);
            
            return Ok(productDto);
        }

        // POST products
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductDto productDto) {

            Product product = _productConverter.Convert(productDto);
            Product newProduct = await _repository.Add(product);

            return Ok(newProduct);
        }

        // PUT products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProductDto productDto) {
            if (productDto.Id != id) {
                return BadRequest();
            }

            Product? modifiedProduct = await _repository.Get(id);

            if (modifiedProduct == null) {
                return NotFound($"Product with id: {id} does not exist");
            }

            modifiedProduct.Name = productDto.Name;
            modifiedProduct.Price = productDto.Price;
            modifiedProduct.ItemsInStock = productDto.ItemsInStock;
            modifiedProduct.ItemsReserved = productDto.ItemsReserved;

            bool editSuccess = await _repository.Edit(modifiedProduct);
            return editSuccess ? new OkResult() : new BadRequestResult();
        }

        // PUT products
        [HttpPut]
        public async Task<IActionResult> PutMany([FromBody] List<ProductDto> editedProductDtos) {

            List<Product> productsToEdit = (await _repository.GetAll()).ToList();
            var editedProducts = new List<Product>();

            //If any od the products don't exist, return NotFound
            foreach (ProductDto editedProduct in editedProductDtos) {
                if (!productsToEdit.Select(product => product.Id).Contains(editedProduct.Id)) {
                    return NotFound($"Product with id: {editedProduct.Id} does not exist");
                }
                editedProducts.Add(_productConverter.Convert(editedProduct));
            }

            bool editSuccess = await _repository.Edit(editedProducts);
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
