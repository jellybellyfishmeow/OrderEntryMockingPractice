using System.Collections.Generic;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private ICustomerRepository _customerRepository;
        private IEmailService _emailService;
        private IOrderFulfillmentService _orderFulfillmentService;
        private IProductRepository _productRepository;
        private ITaxRateService _taxRateService;

        public OrderService(ICustomerRepository customerRepository, IEmailService emailService, 
            IOrderFulfillmentService orderFulfillmentService, IProductRepository productRepository,
            ITaxRateService taxRateService)
        {
            _customerRepository = customerRepository;
            _emailService = emailService;
            _orderFulfillmentService = orderFulfillmentService;
            _productRepository = productRepository;
            _taxRateService = taxRateService;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            // check if unique
            // check if in stock

            // find taxes
            
            // calculate nettotal
            // calculate ordertotal
            return null;
        }

        private decimal TotalTaxRate(IEnumerable<TaxEntry> taxEntries)
        {
            return taxEntries.Sum(taxEntry => taxEntry.Rate);
        }

        private decimal NetTotal(IEnumerable<OrderItem> orderItems)
        {
            return orderItems.Sum(item => item.Quantity * item.Product.Price);
        }

        private decimal OrderTotal(decimal netTotal, decimal taxRate) => taxRate * netTotal;
    }
}
