using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;
namespace ProductApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase {
        private readonly IConverter<Product, ProductDto> _productConverter;
        private readonly IRepository<Product> _repository;

        public ProductsController(IRepository<Product> repos, IConverter<Product, ProductDto> converter) {
            _repository = repos;
            _productConverter = converter;
        }

        // GET products
        [HttpGet]
        public async Task<IEnumerable<ProductDto>> Get() {
            var productDtoList = new List<ProductDto>();
            foreach (Product product in await _repository.GetAll()) {
                ProductDto productDto = _productConverter.Convert(product);
                productDtoList.Add(productDto);
            }
            return productDtoList;
        }

        // GET products/5
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get(int id) {
            Product? item = await _repository.Get(id);
            if (item == null) {
                return NotFound();
            }
            ProductDto productDto = _productConverter.Convert(item);
            return new ObjectResult(productDto);
        }

        // POST products
        [HttpPost]
        public IActionResult Post([FromBody] ProductDto productDto) {

            if (productDto == null) {
                return BadRequest();
            }

            Product product = _productConverter.Convert(productDto);
            Task<Product> newProduct = _repository.Add(product);

            return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
            //return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, _productConverter.Convert(newProduct));
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
