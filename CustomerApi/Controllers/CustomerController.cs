using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
namespace CustomerApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : Controller {
        private readonly IRepository<Customer> _repository;

        public CustomerController(IRepository<Customer> repos) {
            _repository = repos;
        }

        //GET Customer
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get(int id) {
            Customer? customer = await _repository.Get(id);
            if (customer == null) {
                return NotFound();
            }

            return new ObjectResult(customer);
        }

        //POST Customer
        [HttpPost]
        public IActionResult Post([FromBody] Customer customer) {

            Task<Customer> newCustomer = _repository.Add(customer);
            return CreatedAtRoute("GetProduct", new { id = newCustomer.Id }, newCustomer);
        }

        //Put Customer
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Customer customer) {
            if (customer.Id != id) {
                return BadRequest();
            }

            Customer? modifiedCustomer = await _repository.Get(id);

            if (modifiedCustomer == null) {
                return NotFound();
            }

            modifiedCustomer.Name = customer.Name;
            modifiedCustomer.Email = customer.Email;
            modifiedCustomer.Phone = customer.Phone;
            modifiedCustomer.BillingAddress = customer.BillingAddress;
            modifiedCustomer.ShippingAddress = customer.ShippingAddress;
            modifiedCustomer.creditStanding = customer.creditStanding;

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
