using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Messaging;
using OrderApi.Models;
using SharedModels;
namespace OrderApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase {
        private readonly IConverter<OrderLine, OrderLineDto> _orderLineConverter = new OrderLineConverter();

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
        public async Task<IActionResult> Post([FromBody] OrderPostBindingModel order) {
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
            
            //Verify that the customer has enough credit
            if (orderCustomer.creditStanding <= 0) {
                //TODO I have no idea what credit standing does, help
                return BadRequest($"Customer: {orderCustomer.Name} does not have enough credit standing to make an order!");
            }
            
            //Verify that he customer has no outstanding orders
            IEnumerable<Order> orders = await _repository.GetAll();
            if (orders.Any(x => x.CustomerId == orderCustomer.Id && x.Status == OrderStatus.Shipped)) {
                return BadRequest($"Customer: {orderCustomer.Name} has an unpaid order. Pay the order before making another one!");
            }
            
            //Verify that each ordered product type has enough items on stock
            foreach (OrderLinePostBindingModel orderLine in order.OrderLines) {
                ProductDto matchingProduct = orderedProducts.First(y => y.Id == orderLine.ProductId);
                if (orderLine.Quantity > matchingProduct.ItemsInStock) {
                    return BadRequest($"Product: {matchingProduct.Name} does not have enough items on stock!");
                }
            }

            // Once the stock is verified, reserve these products and update the new amount in the products API using messaging
            Order orderModel = new(order);
            orderModel.Status = OrderStatus.Pending;
            Order newOrderDto = await _repository.Add(orderModel);

            await _messagePublisher.PublishOrderStatusChangedMessage(orderModel.CustomerId, orderModel.Id,
                orderModel.OrderLines.Select(x => _orderLineConverter.Convert(x)).ToList(), "completed");

            bool completed = false;
            int timeoutTries = 50;
            //Wait for the message listener to update the order to completed
            while (!completed && timeoutTries > 0) {
                Order? pendingOrder = await _repository.Get(newOrderDto.Id);
                // If the order is null, it means that it was deleted by the message listener
                if (pendingOrder is null) {
                    return BadRequest("Failed to create order!");
                }
                if (pendingOrder.Status == OrderStatus.Completed)
                    completed = true;
                Thread.Sleep(250);
                timeoutTries--;
            }
            if ( !completed) {
                await _repository.Remove(newOrderDto.Id);
                return BadRequest("Failed to create order! Product service timeout.");
            }
            
            return CreatedAtRoute("GetOrder", new { id = newOrderDto.Id }, newOrderDto);
        }

        /// <summary>
        /// This action method cancels an order and publishes an OrderStatusChangedMessage
        /// with topic set to "cancelled".
        /// </summary>
        /// <param name="id">Id of the order</param>
        /// <returns>The Updated order model</returns>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            Order? order = await _repository.Get(id);
            if (order is null)
            {
                return NotFound($"Order with ID{id} not found");
            }
            if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Pending)
            {
                return BadRequest("Order could not be cancelled as it was not completed");
            }
            //cancel order
            order.Status = OrderStatus.Cancelled;
            await _repository.Edit(order);
            await _messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.Id, order.OrderLines.Select(x => _orderLineConverter.Convert(x)).ToList(), "cancelled");
            return Ok(order);
        }
        
        /// <summary>
        /// This action method ships an order and publishes an OrderStatusChangedMessage
        /// with topic set to "shipped".
        /// </summary>
        /// <param name="id">Id of the order</param>
        /// <returns>The Updated order model</returns>
        [HttpPut("{id}/ship")]
        public async Task<IActionResult> Ship(int id) {
            Order? order = await _repository.Get(id);
            if (order is null)
            {
                return NotFound($"Order with ID{id} not found");
            }
            if (order.Status != OrderStatus.Completed)
            {
                return BadRequest("Order could not be shipped as the status was not completed");
            }
            //cancel order
            order.Status = OrderStatus.Shipped;
            await _repository.Edit(order);
            await _messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.Id, 
                order.OrderLines.Select(x => _orderLineConverter.Convert(x)).ToList(), "shipped");
            return Ok(order);
        }

        /// <summary>
        ///  This action method marks an order as paid and publishes a CreditStandingChangedMessage<br/>
        ///  (which have not yet been implemented), if the credit standing changes.
        /// </summary>
        /// <param name="id">Id of the order</param>
        /// <returns>The updated order</returns>
        
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> Pay(int id) {
            Order? order = await _repository.Get(id);
            if (order is null) {
                return NotFound($"Order with ID{id} not found");
            }
            if (order.Status != OrderStatus.Shipped) {
                return BadRequest("Order could not be paid as the status was not shipped");
            }
            order.Status = OrderStatus.Paid;
            await _repository.Edit(order);
            await _messagePublisher.CreditStandingChangedMessage(order.CustomerId, 100 , "paid"); //todo make this increase the customer credit standing

            return Ok(order);
            
        }
    }
}
