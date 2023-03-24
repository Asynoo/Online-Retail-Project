using SharedModels;

namespace CustomerApi.Models;

public class CustomerConverter : IConverter<Customer, CustomerDto>
{
    public Customer Convert(CustomerDto sharedCustomer) {
        return new Customer {
            Id = sharedCustomer.Id,
            Name = sharedCustomer.Name,
            Email = sharedCustomer.Email,
            Phone = sharedCustomer.Phone,
            BillingAddress = sharedCustomer.BillingAddress,
            ShippingAddress = sharedCustomer.ShippingAddress,
            creditStanding = sharedCustomer.creditStanding,
            
        };
    }

    public CustomerDto Convert(Customer hiddenCustomer) {
        return new CustomerDto {
            Id = hiddenCustomer.Id,
            Name = hiddenCustomer.Name,
            Email = hiddenCustomer.Email,
            Phone = hiddenCustomer.Phone,
            BillingAddress = hiddenCustomer.BillingAddress,
            ShippingAddress = hiddenCustomer.ShippingAddress,
            creditStanding = hiddenCustomer.creditStanding,
        };
    }
}