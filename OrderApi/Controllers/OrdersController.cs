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
            RestClient c = new RestClient("https://localhost:5001/products/"); //TODO: make this dynamic
            RestRequest request = new(order.ProductId.ToString());
            Task<Product?> response = c.GetAsync<Product>(request);
            response.Wait();
            Product? orderedProduct = response.Result;
            if (orderedProduct is null) {
                return NotFound("Products not found");
            }

            if (order.Quantity <= orderedProduct.ItemsInStock - orderedProduct.ItemsReserved) {
                // reduce the number of items in stock for the ordered product,
                // and create a new order.
                orderedProduct.ItemsReserved += order.Quantity;
                RestRequest updateRequest = new(orderedProduct.Id.ToString());
                updateRequest.AddJsonBody(orderedProduct);
                Task<RestResponse> updateResponse = c.PutAsync(updateRequest);
                updateResponse.Wait();
                
                if (updateResponse.IsCompletedSuccessfully) {
                    Order newOrder = await _repository.Add(order);
                    return CreatedAtRoute("GetOrder",
                        new { id = newOrder.Id }, newOrder);
                }
            }

            // If the order could not be created, "return no content".
            return NoContent();
        }

    }
}
