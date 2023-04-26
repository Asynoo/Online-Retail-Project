using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
using SharedModels;

namespace CustomerApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : Controller {
        private readonly IRepository<Customer> _repository;
        private readonly IConverter<Customer, CustomerDto> _customerConverter;

        public CustomerController(IRepository<Customer> repos, IConverter<Customer, CustomerDto> converter) {
            _repository = repos;
            _customerConverter = converter;

        }
        
        //GET all Customers
        [HttpGet( Name = "GetCustomers")]
        public async Task<IActionResult> Get() {
            IEnumerable<Customer> customers = (await _repository.GetAll()).ToList();
            if (!customers.Any()) {
                return NotFound();
            }

            IEnumerable<CustomerDto> customerDtos = customers.Select(x => _customerConverter.Convert(x));
            return new ObjectResult(customerDtos);
        }

        //GET Customer
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get(int id) {
            Customer? customer = await _repository.Get(id);
            if (customer == null) {
                return NotFound();
            }

            CustomerDto customerDto = _customerConverter.Convert(customer);
            return new ObjectResult(customerDto);
        }

        //POST Customer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto customerDto)
        {

            Customer customer = _customerConverter.Convert(customerDto);
            Customer newCustomer = await _repository.Add(customer);
            return CreatedAtRoute("GetCustomer", new { id = newCustomer.Id }, _customerConverter.Convert(newCustomer));
        }

        //Put Customer
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CustomerDto customerDto) {
            if (customerDto.Id != id) {
                return BadRequest();
            }

            Customer? modifiedCustomer = await _repository.Get(id);

            if (modifiedCustomer == null) {
                return NotFound($"Customer with id: {id} does not exist");
            }

            modifiedCustomer.Name = customerDto.Name;
            modifiedCustomer.Email = customerDto.Email;
            modifiedCustomer.Phone = customerDto.Phone;
            modifiedCustomer.BillingAddress = customerDto.BillingAddress;
            modifiedCustomer.ShippingAddress = customerDto.ShippingAddress;
            modifiedCustomer.creditStanding = customerDto.creditStanding;

            bool editSuccess = await _repository.Edit(modifiedCustomer);
            return editSuccess ? new OkResult() : new BadRequestResult();
        }

        //Delete customer
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) {
            if (await _repository.Get(id) is null) {
                return NotFound();
            }

            bool deleteSuccess = await _repository.Remove(id);
            return deleteSuccess ? new OkResult() : new BadRequestResult();
        }
    }
}
