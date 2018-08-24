using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MoreLinq.Extensions;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailService _emailService;
        private readonly IOrderFulfillmentService _orderFulfillmentService;
        private readonly IProductRepository _productRepository;
        private readonly ITaxRateService _taxRateService;

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
                
                var customer = _customerRepository.Get((int)order.CustomerId);
                if (customer == null)
                {
                    throw new Exception("customer does not exist");
                }

                var taxEntries = _taxRateService.GetTaxEntries(customer.PostalCode, customer.Country);
                var taxrate = TotalTaxRate(taxEntries);
                var netTotal = NetTotal(orderItems);
                var ordertotal = OrderTotal(netTotal, taxrate);


                orderSummary = new OrderSummary
                {
                    OrderId = confirmation.OrderId,
                    CustomerId = (int) customer.CustomerId,
                    OrderNumber = confirmation.OrderNumber,
                    OrderItems = orderItems,
                    NetTotal = netTotal,
                    Total = ordertotal,
                    Taxes = taxEntries
                };

                    _emailService.SendOrderConfirmationEmail((int) customer.CustomerId, confirmation.OrderId);
                
            }
            else if (AllOrderItemsInStock(orderItems))
            {
                throw new SKUsNotUniqueException();
            }
            else if (AreOrderItemsUnique(orderItems))
            {
                throw new ProductsNotInStockException();
            }
            else
            {
                throw new SKUsNotUniqueAndProductNotInStockException();
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

        private static decimal TotalTaxRate(IEnumerable<TaxEntry> taxEntries) => taxEntries.Sum(taxEntry => taxEntry.Rate);

        private static decimal NetTotal(IEnumerable<OrderItem> orderItems) => orderItems.Sum(item => item.Quantity * item.Product.Price);

        private static decimal OrderTotal(decimal netTotal, decimal taxRate) => taxRate * netTotal;
    }

    [Serializable]
    public class SKUsNotUniqueException : Exception
    {
        public SKUsNotUniqueException()
        {
        }

        public SKUsNotUniqueException(string message)
            : base(message)
        {
        }

        public SKUsNotUniqueException(string message, Exception inner)
            : base(message, inner)
        {
        }
        protected SKUsNotUniqueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class ProductsNotInStockException : Exception
    {
        public ProductsNotInStockException()
        {
        }

        public ProductsNotInStockException(string message)
            : base(message)
        {
        }

        public ProductsNotInStockException(string message, Exception inner)
            : base(message, inner)
        {
        }
        protected ProductsNotInStockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class SKUsNotUniqueAndProductNotInStockException : Exception
    {
        public SKUsNotUniqueAndProductNotInStockException()
        {
        }

        public SKUsNotUniqueAndProductNotInStockException(string message)
            : base(message)
        {
        }

        public SKUsNotUniqueAndProductNotInStockException(string message, Exception inner)
            : base(message, inner)
        {
        }
        protected SKUsNotUniqueAndProductNotInStockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
