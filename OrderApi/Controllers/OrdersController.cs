using System.Net;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using RestSharp;
using SharedModels;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        //private readonly IRepository<Order> _repository;
        IOrderRepository _repository;
        IServiceGateway<ProductDto> _productServiceGateway;
        IMessagePublisher _messagePublisher;


        public OrdersController(IRepository<Order> repos, IServiceGateway<ProductDto> gateway, IMessagePublisher publisher)
        {
            _repository = repos as IOrderRepository;
            _productServiceGateway = gateway;
            _messagePublisher = publisher;

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
            Task<IEnumerable<ProductDto>?> productResponse = productClient.GetAsync<IEnumerable<ProductDto>>(productRequest);
            productResponse.Wait();
            IEnumerable<ProductDto>? orderedProducts = productResponse.Result;
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
            Task<CustomerDto?> response = customerClient.GetAsync<CustomerDto>(request);
            response.Wait();
            CustomerDto? orderCustomer = response.Result;
            if (orderCustomer is null) {
                return NotFound("Customer not found");
            }

            List<ProductDto> productsToUpdate = new();
            //Verify that each ordered product type has enough items on stock
            foreach (OrderLine orderLine in order.OrderLines) {
                ProductDto matchingProduct = orderedProducts.First(y => y.Id == orderLine.ProductId);
                if (orderLine.Quantity > matchingProduct.ItemsInStock) {
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
        
                // POST orders
        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            if (ProductItemsAvailable(order))
            {
                try
                {
                    // Publish OrderStatusChangedMessage. If this operation
                    // fails, the order will not be created
                    await _messagePublisher.PublishOrderStatusChangedMessage(
                        order.customerId, order.OrderLines, "completed");

                    // Create order.
                    order.Status = Order.OrderStatus.completed;
                    var newOrder = _repository.Add(order);
                    return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
                }
                catch
                {
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
                // If there are not enough product items available.
                return StatusCode(500, "Not enough items in stock.");
            }
        }
        //todo fix these messaging stuff
        
        private  bool ProductItemsAvailable(Order order)
        {
            foreach (var orderLine in order.OrderLines)
            {
                // Call product service to get the product ordered.
                var orderedProduct = _productServiceGateway.Get(orderLine.ProductId);
                if (orderLine.Quantity > orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }

        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

    }
}
