using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using ReviewApi.Data;
using ReviewApi.Infrastructure;
using ReviewApi.Models;
using SharedModels;

namespace ReviewAPI.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class ReviewsController : Controller {
        private readonly IRepository<Review> _repository;
        private readonly IConverter<Review, ReviewDto> _reviewConverter;
        private readonly IServiceGateway<CustomerDto> _customerGateway;
        private readonly IServiceGateway<ProductDto> _productGateway;
        private readonly IServiceGateway<OrderDto> _orderGateway;

        public ReviewsController(
            IRepository<Review> repository,
            IConverter<Review, ReviewDto> reviewConverter,
            IServiceGateway<CustomerDto> customerGateway,
            IServiceGateway<ProductDto> productGateway,
            IServiceGateway<OrderDto> orderGateway) {
            _repository = repository;
            _reviewConverter = reviewConverter;
            _customerGateway = customerGateway;
            _productGateway = productGateway;
            _orderGateway = orderGateway;
        }

        // Endpoint to get all reviews
        [HttpGet]
        public async Task<IActionResult> GetAllReviews() {
            IEnumerable<Review> reviews = (await _repository.GetAll()).ToList();
            if (!reviews.Any()) {
                return NotFound("No Reviews found");
            }

            IEnumerable<ReviewDto> reviewDtos = reviews.Select(x => _reviewConverter.Convert(x));
            return Ok(reviewDtos);
        }

        // Endpoint to get a review by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id) {
            Review? review = await _repository.Get(id);
            if (review is null) {
                return NotFound($"Review with Id: {id} not found");
            }

            ReviewDto reviewDto = _reviewConverter.Convert(review);
            return Ok(reviewDto);
        }

        // Endpoint to create a new review
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewPostBindingModel review)
        {
            
            ProductDto? product = await _productGateway.GetAsync(review.ProductId);
            if (product == null) {
                return NotFound($"Product with Id: {review.ProductId} not found");
            }

            CustomerDto? customer = await _customerGateway.GetAsync(review.CustomerId);
            if (customer == null) {
                return NotFound($"Customer with Id: {review.CustomerId} not found");
            }
            
            
            // Check if the customer has ordered this product before
            //TODO Create CustomerHasOrderedProduct. I simply prepared the code for it here.
            /*
            if (!await _orderGateway.CustomerHasOrderedProductAsync(customer.Id, product.Id))
            {
                return BadRequest($"Customer with Id: {review.CustomerId} has not ordered product with Id: {review.ProductId}");
            }
            */
            Review? createdReview = await _repository.Add(new Review
            {
                ProductId = review.ProductId,
                CustomerId = review.CustomerId,
                Title = review.Title,
                Rating = review.Rating,
                Description = review.Description
            });
            
            if (createdReview == null) {
                return BadRequest("Failed to create review");
            }

            return Ok(createdReview);
        }

        /*// Endpoint to update a review by ID
        [HttpPut("{id}")]
        public IActionResult UpdateReview(int id, [FromBody] ReviewPutBindingModel review)
        {
            // Implementation
        }*/

        // Endpoint to delete a review by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id) {
            bool success = await _repository.Remove(id);
            return success ? Ok() : BadRequest($"Failed to delete review with ID:{id}");
        }

        // Endpoint to get reviews by product ID
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetReviewsByProductId(int productId) {
            IEnumerable<Review> reviews = (await _repository.GetAll(productId: productId)).ToList();
            if (!reviews.Any()) {
                return NotFound($"No Reviews found for Product id{productId}");
            }

            IEnumerable<ReviewDto> reviewDtos = reviews.Select(x => _reviewConverter.Convert(x));
            return Ok(reviewDtos);
        }

        // Endpoint to get reviews by customer ID
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetReviewsByCustomerId(int customerId) {
            IEnumerable<Review> reviews = (await _repository.GetAll(customerId: customerId)).ToList();
            if (!reviews.Any()) {
                return NotFound($"No Reviews found for Customer id{customerId}");
            }

            IEnumerable<ReviewDto> reviewDtos = reviews.Select(x => _reviewConverter.Convert(x));
            return Ok(reviewDtos);
        }
    }
}
