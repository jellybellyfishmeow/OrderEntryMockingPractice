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
            return null;
        }

        private double TaxTotal()
        {
            return 3.2;
        }

        private decimal NetTotal(int quantity, decimal price)
        {
            return quantity * price;
        }

        private decimal OrderTotal(decimal netTotal, decimal taxRate)
        {
            return taxRate * netTotal;
        }
    }
}
