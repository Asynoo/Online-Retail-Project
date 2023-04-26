using ProductApi.Models;
using SharedModels;
namespace ReviewApi.Models {
    public class ReviewConverter : IConverter<Review, ReviewDto> {
        public Review Convert(ReviewDto sharedReview) {
            return new Review {
                Id = sharedReview.Id,
                ProductId = sharedReview.ProductId,
                CustomerId = sharedReview.CustomerId,
                Title = sharedReview.Title,
                Rating = sharedReview.Rating,
                Description = sharedReview.Description,
                AddedAt = sharedReview.AddedAt
            };
        }

        public ReviewDto Convert(Review hiddenReview) {
            return new ReviewDto {
                Id = hiddenReview.Id,
                ProductId = hiddenReview.ProductId,
                CustomerId = hiddenReview.CustomerId,
                Title = hiddenReview.Title,
                Rating = hiddenReview.Rating,
                Description = hiddenReview.Description,
                AddedAt = hiddenReview.AddedAt
            };
        }
    }
}
