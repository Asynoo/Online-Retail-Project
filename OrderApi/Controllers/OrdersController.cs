using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using RestSharp;
using SharedModels;
namespace OrderApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase {
        private readonly IServiceGateway<CustomerDto> _customerServiceGateway;
        private readonly IMessagePublisher _messagePublisher;

        private readonly IServiceGateway<ProductDto> _productServiceProductGateway;

        private readonly IRepository<Order> _repository;


        public OrdersController(IRepository<Order> repository, IServiceGateway<ProductDto> productGateway, IServiceGateway<CustomerDto> customerGateway, IMessagePublisher publisher) {
            _repository = repository;
            _productServiceProductGateway = productGateway;
            _customerServiceGateway = customerGateway;
            _messagePublisher = publisher;
        }

        // GET: orders
        [HttpGet]
        public async Task<IEnumerable<Order>> Get() {
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
        public async Task<IActionResult> Post([FromBody] Order order) {
            List<ProductDto>? orderedProducts = await _productServiceProductGateway.GetAll();
            if (orderedProducts == null || !orderedProducts.Any()) {
                return NotFound("No products found");
            }

            IEnumerable<int> orderProductIds = order.OrderLines.Select(x => x.ProductId);
            if (!orderProductIds.All(x => orderedProducts.Select(y => y.Id).Contains(x))) {
                return NotFound("Product doesn't exist");
            }

            //var orderProductIds = order.OrderLines.Select(x => new {x.ProductId, x.Quantity});
            //orderedProducts.All(x => x.pr)

            CustomerDto? orderCustomer = await _customerServiceGateway.Get(order.CustomerId);
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

            try
            {
                order.Status = Order.OrderStatus.Pending;
                Order newOrder = await _repository.Add(order);
                
                await _messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.OrderLines, "completed");

                bool completed = false;
                while (!completed)
                {
                    var pendingOrder = await _repository.Get(newOrder.Id);
                    if (pendingOrder.Status == Order.OrderStatus.Completed)
                        completed = true;
                    Thread.Sleep(100);
                }
                
                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);

                
            }
            catch
            {
                return StatusCode(500, "An error happened. Try again.");
            }
            
            
            

            
            if (await _productServiceProductGateway.UpdateMany(productsToUpdate)) {

                //todo maybe revert await _messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.OrderLines, "completed");

                // Create order.
                order.Status = Order.OrderStatus.Completed;
                Order newOrder = await _repository.Add(order);
                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            }

            // If the order could not be created, "return no content".
            return NoContent();
        }

        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            Order? order = await _repository.Get(id);
            if (order != null && (order.Status != Order.OrderStatus.Completed || order.Status != Order.OrderStatus.Pending))
            {
                return BadRequest("Order was cancelled as it was not completed");
            }
            //cancel order
            order.Status = Order.OrderStatus.Cancelled;
            await _repository.Edit(order);
            await _messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.OrderLines.ToList(), "cancelled");
            return Ok(id);
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public async Task<IActionResult> Ship(int id) {
            Order? order = await _repository.Get(id);
            if (order != null && order.Status != Order.OrderStatus.Completed)
            {
                return BadRequest("Order could not be shipped as the status was not completed");
            }
            //cancel order
            order.Status = Order.OrderStatus.Shipped;
            await _repository.Edit(order);
            await _messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.OrderLines.ToList(), "shipped");
            return Ok(id);
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> Pay(int id) {
            Order? order = await _repository.Get(id);
            if (order != null && order.Status != Order.OrderStatus.Shipped)
            {
                return BadRequest("Order could not be paid as the status was not shipped");
            }
            //cancel order
            order.Status = Order.OrderStatus.Paid;
            await _repository.Edit(order);
            await _messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.OrderLines.ToList(), "paid");
            return Ok(id);
        }
    }
}
