using System.Net;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Models;
using RestSharp;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> _repository;

        public OrdersController(IRepository<Order> repos)
        {
            _repository = repos;
        }

        // GET: orders
        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            return await _repository.GetAll();
        }

        // GET orders/5
        [HttpGet("{id:int}", Name = "GetOrder")]
        public async Task<IActionResult> Get(int id) {
            Order? item = await _repository.Get(id);
            if (item is null) {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Order order) {
            // Call ProductApi to get the product ordered
            // You may need to change the port number in the BaseUrl below
            // before you can run the request.
            RestClient productClient = new("https://productApi/products/");
            RestRequest productRequest = new();
            Task<IEnumerable<Product>?> productResponse = productClient.GetAsync<IEnumerable<Product>>(productRequest);
            productResponse.Wait();
            IEnumerable<Product>? orderedProducts = productResponse.Result;
            if (orderedProducts == null || !orderedProducts.Any()) {
                return NotFound("No products found");
            }
            
            IEnumerable<int> orderProductIds = order.OrderLines.Select(x => x.ProductId);
            if (!orderProductIds.All(x => orderedProducts.Select(y => y.Id).Contains(x))) {
                return NotFound("Product doesn't exist");
            }
            
            //var orderProductIds = order.OrderLines.Select(x => new {x.ProductId, x.Quantity});
            //orderedProducts.All(x => x.pr)
            
            RestClient customerClient = new("https://customerApi/customers/");
            RestRequest request = new(order.CustomerId.ToString());
            Task<Customer?> response = customerClient.GetAsync<Customer>(request);
            response.Wait();
            Customer? orderCustomer = response.Result;
            if (orderCustomer is null) {
                return NotFound("Customer not found");
            }

            List<Product> productsToUpdate = new();
            //Verify that each ordered product type has enough items on stock
            foreach (OrderLine orderLine in order.OrderLines) {
                Product matchingProduct = orderedProducts.First(y => y.Id == orderLine.ProductId);
                if (orderLine.Quantity > matchingProduct.AvailableItems) {
                    return BadRequest($"Product: {matchingProduct.Name} does not have enough items on stock!");
                }
                matchingProduct.ItemsReserved += orderLine.Quantity;
                productsToUpdate.Add(matchingProduct);
            }
            // Once the stock is verified, reserve these products and update the new amount in the products API
            RestRequest updateRequest = new();
            updateRequest.AddJsonBody(productsToUpdate);
            Task<RestResponse> updateResponse = productClient.PutAsync(updateRequest);
            updateResponse.Wait();
            
            if (updateResponse.Result.StatusCode == HttpStatusCode.OK) {
                Order newOrder = await _repository.Add(order);
                return CreatedAtRoute("GetOrder",
                    new { id = newOrder.Id }, newOrder);
            }

                // If the order could not be created, "return no content".
            return NoContent();
        }

    }
}
