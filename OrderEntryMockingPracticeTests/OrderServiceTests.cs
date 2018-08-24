using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using MSTestExtensions;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using StringAssert = Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert;
using Shouldly;
using System.Text.RegularExpressions;

namespace OrderEntryMockingPracticeTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private Mock<ICustomerRepository> _mockCustomerRepository;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IOrderFulfillmentService> _mockOrderFulfillmentService;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<ITaxRateService> _mockTaxRateService;

        private OrderService _orderService;
        private Order _order;
        private List<TaxEntry> _taxEntries;

        private const int OrderId = 10;
        private const string OrderNumber = "10";
        private const int CustomerId = 1;
        private const decimal NetTotal = 35.0m;
        private const decimal PostTaxTotal = NetTotal * 1.2m;

        [SetUp]
        public void SetUp()
        {
            _mockCustomerRepository = new Moq.Mock<ICustomerRepository>();
            _mockEmailService = new Moq.Mock<IEmailService>();
            _mockOrderFulfillmentService = new Moq.Mock<IOrderFulfillmentService>();
            _mockProductRepository = new Moq.Mock<IProductRepository>();
            _mockTaxRateService = new Moq.Mock<ITaxRateService>();

            _mockCustomerRepository.Setup(c => c.Get(It.IsInRange<int>(0, 100, Range.Inclusive)))
                .Returns(new Customer {CustomerId = CustomerId, PostalCode = "9999", Country =  "USA"});

            _mockOrderFulfillmentService.Setup(o => o.Fulfill(It.IsAny<Order>()))
                .Returns((Order o) => new OrderConfirmation{
                    OrderId = OrderId,
                    OrderNumber = OrderNumber,
                    CustomerId = (int)o.CustomerId
                });

            _mockProductRepository.Setup(r => r.IsInStock(It.IsIn("yay", "yes"))).Returns(true);
            _mockProductRepository.Setup(r => r.IsInStock(It.IsIn("nope", "nah"))).Returns(false);

            _taxEntries = new List<TaxEntry>()
                {
                    new TaxEntry()
                    {
                        Description = "tax1",
                        Rate = 0.60m,
                    },
                    new TaxEntry()
                    {
                        Description = "tax2",
                        Rate = 0.60m,
                    }
            };
            _mockTaxRateService.Setup(t => t.GetTaxEntries(It.IsRegex("[0-9+]+$"), 
                It.IsRegex("[A-Z]+"))).Returns(_taxEntries);

            _orderService = new OrderService(_mockCustomerRepository.Object, _mockEmailService.Object,
                _mockOrderFulfillmentService.Object, _mockProductRepository.Object, 
                _mockTaxRateService.Object);

            _order = CreateValidOrder();
        }

        private static OrderItem CreateOrderItem(string name, decimal price, string sku, int quantity) => new OrderItem
        {
            Product = new Product
            {
                Name = name,
                Price = price,
                Sku = sku
            },
            Quantity = quantity,
        };

        private static Order CreateOrder(int customerId, List<OrderItem> itemsOrdered) => new Order
        {
            OrderItems = itemsOrdered,
            CustomerId = customerId
        };

        private static Order CreateValidOrder()
        {
            return CreateOrder(1, new List<OrderItem> { CreateOrderItem("uni1", 10.0m, "yes", 2), CreateOrderItem("uni2", 5.0m, "yay", 3) });
        }

        private void EmptyTaxBadPostalAndCountry()
        {
            _mockCustomerRepository.Setup(c => c.Get(It.IsInRange<int>(0, 100, Range.Inclusive)))
                .Returns(new Customer { CustomerId = CustomerId, PostalCode = "xxx", Country = "333" });
            _mockTaxRateService.Setup(t => t.GetTaxEntries(It.IsRegex("[A-Z]+"),
                It.IsRegex("[a-z]+"))).Returns(new List<TaxEntry>());
        }

        [Test]
        public void PlaceOrder_ValidOrderAllItemsUniqueAndInStock_ReturnsValidOrderSummary()
        {
            var orderSummary = _orderService.PlaceOrder(_order);
            orderSummary.ShouldNotBeNull("order summary should never be null");
            orderSummary.OrderNumber.ShouldBe(OrderNumber, "orderNumbers do not match");
            orderSummary.OrderId.ShouldBe(OrderId, "OrderIds do not match");
            orderSummary.CustomerId.ShouldBe(CustomerId, "customerIds don't match");
            orderSummary.NetTotal.ShouldBe(NetTotal, "net total is incorrect");
            orderSummary.Total.ShouldBe(PostTaxTotal, "order total is incorrect");
            orderSummary.Taxes.ShouldBe(_taxEntries, "taxes are incorrect");
        }

        [Test]
        public void PlaceOrder_InvalidOrderNotAllItemsUnique_ExceptionThrown()
        {
            var badOrder = CreateOrder(1, new List<OrderItem> { CreateOrderItem("nonu1", 10.0m, "yes", 2), CreateOrderItem("nonu2", 10.0m, "yes", 3) });
            Should.Throw<SKUsNotUniqueException>(() => _orderService.PlaceOrder(badOrder));
        }

        [Test]
        public void PlaceOrder_InvalidOrderNotAllProductsInStock_ExceptionThrown()
        {
            var badOrder = CreateOrder(1, new List<OrderItem> { CreateOrderItem("out1", 33.4m, "nope", 2), CreateOrderItem("out2", 30.5m, "nah", 3) });
            Should.Throw<ProductsNotInStockException>(() => _orderService.PlaceOrder(badOrder));
        }

        [Test]
        public void PlaceOrder_InvalidOrder_ExceptionThrown()
        {
            var badOrder = CreateOrder(1, new List<OrderItem> { CreateOrderItem("invalid1", 33.4m, "nope", 2), CreateOrderItem("invalid2", 33.45m, "nope", 3) });
            Should.Throw<SKUsNotUniqueAndProductNotInStockException>(() => _orderService.PlaceOrder(badOrder));
        }

        [Test]
        public void PlaceOrder_ValidOrder_OrderFulfillmentServiceCalled()
        {
            OrderSummary orderSummary = _orderService.PlaceOrder(_order);
            _mockOrderFulfillmentService.Verify(f => f.Fulfill(_order), Times.Once);
        }

       
        [Test]
        public void PlaceOrder_ValidOrder_CanQueryTax()
        {
            var orderSummary = _orderService.PlaceOrder(_order);
            _mockTaxRateService.Verify(s => s.GetTaxEntries(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void PlaceOrder_ValidOrder_ConfirmationEmailSent()
        {
            var orderSummary = _orderService.PlaceOrder(_order);
            _mockEmailService.Verify(e => e.SendOrderConfirmationEmail(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }
       
        [Test]
        public void PlaceOrder_InvalidCustomer_CannotRetrieveFromCustomerRepository()
        {
            var badCustomerOrder = CreateOrder(222222,
                new List<OrderItem>
                {
                    CreateOrderItem("uni1", 10.0m, "yes", 2),
                    CreateOrderItem("uni2", 5.0m, "yay", 3)
                });
           Should.Throw<Exception>(() => _orderService.PlaceOrder(badCustomerOrder));
        }

        [Test]
        public void PlaceOrder_InvalidTax_CannotRetrieveFromTaxRateService()
        {
            EmptyTaxBadPostalAndCountry();
            var orderSummary = _orderService.PlaceOrder(_order);
            orderSummary.Taxes.ShouldBe(new List<TaxEntry>(), "tax should not have any entries");
        }

    }
}
