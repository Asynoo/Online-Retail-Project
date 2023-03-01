using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomerController : Controller
{
    private readonly IRepository<Customer> _repository;

    public CustomerController(IRepository<Customer> repos)
    {
        _repository = repos;
    }

    //GET Customer
    [HttpGet("{id}", Name = "GetCustomer")]
    public IActionResult Get(int id)
    {
        var customer = _repository.Get(id);
        if (customer == null)
        {
            return NotFound();
        }

        return new ObjectResult(customer);
    }
    
    //POST Customer
    [HttpPost]
    public IActionResult Post([FromBody] Customer customer)
    {
        if (customer == null)
        {
            return BadRequest();
        }

        var newCustomer = _repository.Add(customer);
        return CreatedAtRoute("GetProduct", new { id = newCustomer.Id }, newCustomer);
    }
    
    //Put Customer
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] Customer customer)
    {
        if (customer == null || customer.Id != id)
        {
            return BadRequest();
        }

        var modifiedCustomer = _repository.Get(id);

        if (modifiedCustomer == null)
        {
            return NotFound();
        }

        modifiedCustomer.Name = customer.Name;
        modifiedCustomer.Email = customer.Email;
        modifiedCustomer.Phone = customer.Phone;
        modifiedCustomer.BillingAddress = customer.BillingAddress;
        modifiedCustomer.ShippingAddress = customer.ShippingAddress;
        modifiedCustomer.creditStanding = customer.creditStanding;
        
        _repository.Edit(modifiedCustomer);
        return new NoContentResult();
    }
    
    //Delete customer
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (_repository.Get(id) == null)
        {
            return NotFound();
        }
        
        _repository.Remove(id);
        return new NoContentResult();
    }
}

