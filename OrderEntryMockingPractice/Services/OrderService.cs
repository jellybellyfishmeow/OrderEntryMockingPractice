using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;
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
            var orderItems = order.OrderItems;
            var orderSummary = new OrderSummary();

            if (AreOrderItemsUnique(orderItems) && AllOrderItemsInStock(orderItems))
            {
                var confirmation = _orderFulfillmentService.Fulfill(order);
                
                // find customer
                var customer = _customerRepository.Get((int)order.CustomerId);
                // find taxes
                var taxEntries = _taxRateService.GetTaxEntries(customer.PostalCode, customer.Country);
                var taxrate = TotalTaxRate(taxEntries);
                // calculate nettotal
                var netTotal = NetTotal(orderItems);

                // calculate ordertotal
                var ordertotal = OrderTotal(netTotal, taxrate);

                // create summary
                /*
                 * public int OrderId { get; set; }
                    public string OrderNumber { get; set; }
                    public int CustomerId { get; set; }

                    public List<OrderItem> OrderItems { get; set; }
                    public decimal NetTotal { get; set; }
                    public IEnumerable<TaxEntry> Taxes { get; set; }
                    public decimal Total { get; set; }
                 */
                orderSummary = new OrderSummary
                {
                    OrderId = confirmation.OrderId,
                    CustomerId = (int)customer.CustomerId,
                    OrderNumber = confirmation.OrderNumber,
                    OrderItems = orderItems,
                    NetTotal = netTotal,
                    Total = ordertotal,
                    Taxes = taxEntries
                 };

                // email

            }
            else if (AllOrderItemsInStock(orderItems))
            {
                // throw unique
            }
            else if (AreOrderItemsUnique(orderItems))
            {
                // throw in stock
            }
            else
            {
                // throw both
            }


            return orderSummary;
        }

        private static bool AreOrderItemsUnique(List<OrderItem> orderItems)
        {
            return orderItems.DistinctBy(o => o.Product.Sku).Count() == orderItems.Count();
        }

        private bool AllOrderItemsInStock(IEnumerable<OrderItem> orderItems)
        {
            return orderItems.All(item => _productRepository.IsInStock(item.Product.Sku));
        }

        private decimal TotalTaxRate(IEnumerable<TaxEntry> taxEntries) => taxEntries.Sum(taxEntry => taxEntry.Rate);

        private decimal NetTotal(IEnumerable<OrderItem> orderItems) => orderItems.Sum(item => item.Quantity * item.Product.Price);

        private decimal OrderTotal(decimal netTotal, decimal taxRate) => taxRate * netTotal;
    }
}
