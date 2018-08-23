using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using StringAssert = Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert;

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
        // private OrderSummary _orderSummary;

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

            _mockCustomerRepository.Setup(c => c.Get(It.IsAny<int>())).Returns(new Customer {CustomerId = CustomerId});

            _mockOrderFulfillmentService.Setup(o => o.Fulfill(It.IsAny<Order>()))
                .Returns((Order o) => new OrderConfirmation{
                    OrderId = OrderId,
                    OrderNumber = OrderNumber,
                    CustomerId = (int) (o.CustomerId ?? CustomerId)
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
            _mockTaxRateService.Setup(t => t.GetTaxEntries(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_taxEntries);

            _orderService = new OrderService(_mockCustomerRepository.Object, _mockEmailService.Object,
                _mockOrderFulfillmentService.Object, _mockProductRepository.Object, 
                _mockTaxRateService.Object);

            _order = CreateValidOrder();
            // _orderSummary = _orderService.PlaceOrder(_order);
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

        [Test]
        public void PlaceOrder_ValidOrderAllItemsUniqueAndInStock_ReturnsOrderSummary()
        {
            var orderSummary = _orderService.PlaceOrder(_order);
            Assert.IsNotNull(orderSummary);
            //Assert.Equals(orderSummary.OrderNumber, orderNumber);
            //Assert.Equals(orderSummary.OrderId, orderId);
            //Assert.Equals(orderSummary.CustomerId, _order.CustomerId);
            // duplicating the tests? what's best practice here, since I can check for them in one test?
        }

        [Test]
        public void PlaceOrder_InvalidOrderNotAllItemsUnique_ExceptionThrown()
        {
            var badOrder = CreateOrder(1, new List<OrderItem> { CreateOrderItem("nonu1", 10.0m, "yes", 2), CreateOrderItem("nonu2", 10.0m, "yes", 3) });
           // Assert.Throws<SKUsNotUniqueException>(() => _orderService.PlaceOrder(_order));
        }

        [Test]
        public void PlaceOrder_InvalidOrderNotAllProductsInStock_ExceptionThrown()
        {
            var badOrder = CreateOrder(1, new List<OrderItem> { CreateOrderItem("out1", 33.4m, "nope", 2), CreateOrderItem("out2", 30.5m, "nah", 3) });
            //Assert.Throws<ProductsNotInStockException>(() => _orderService.PlaceOrder(_order));
        }

        [Test]
        public void PlaceOrder_InvalidOrder_ExceptionThrown()
        {
            var badOrder = CreateOrder(1, new List<OrderItem> { CreateOrderItem("invalid1", 33.4m, "nope", 2), CreateOrderItem("invalid2", 33.45m, "nope", 3) });
           // Assert.Throws<SKUsNotUniqueAndProductNotInStockException>(() => _orderService.PlaceOrder(_order));
        }

        [Test]
        public void PlaceOrder_ValidOrder_OrderFulfillmentServiceCalled()
        {
            OrderSummary orderSummary = _orderService.PlaceOrder(_order);
            _mockOrderFulfillmentService.Verify(f => f.Fulfill(_order), Times.Once);
        }

        [Test]
        public void PlaceOrder_ValidOrder_ContainsFulfillmentConfirmationOrderNumber()
        {
            var orderSummary = _orderService.PlaceOrder(_order);

            Assert.IsNotNull(orderSummary);
            Assert.AreEqual(OrderNumber, orderSummary.OrderNumber);
        }

        [Test]
        public void PlaceOrder_ValidOrder_ContainsFulfillmentOrderId()
        {
            var orderSummary = _orderService.PlaceOrder(_order);

            Assert.IsNotNull(orderSummary);
            Assert.AreEqual(OrderId, orderSummary.OrderId);
        }
        [Test]
        public void PlaceOrder_ValidOrder_ContainsCorrectTaxes()
        {
            var orderSummary = _orderService.PlaceOrder(_order);
            _mockTaxRateService.Verify(s => s.GetTaxEntries(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            Assert.AreEqual(_taxEntries, orderSummary.Taxes);
        }

        [Test]
        public void PlaceOrder_ValidOrder_ConfirmationEmailSent()
        {

        }

        [Test]
        public void PlaceOrder_ValidOrder_CorrectNetTotal()
        {
            var orderSummary = _orderService.PlaceOrder(_order);

            Assert.IsNotNull(orderSummary);
            Assert.AreEqual(NetTotal, orderSummary.NetTotal);

        }
        [Test]
        public void PlaceOrder_ValidOrder_CorrectOrderTotal()
        {
            var orderSummary = _orderService.PlaceOrder(_order);

            Assert.IsNotNull(orderSummary);
            Assert.AreEqual(PostTaxTotal, orderSummary.Total);
        }

        [Test]
        public void PlaceOrder_ValidCustomer_RetrieveFromCustomerRepository()
        {

        }

        [Test]
        public void PlaceOrder_InvalidCustomer_CannotRetrieveFromCustomerRepository()
        {
            _order = CreateValidOrder();


        }

        [Test]
        public void PlaceOrder_ValidTax_RetrieveFromTaxRateService()
        {

        }

        [Test]
        public void PlaceOrder_InvalidTax_CannotRetrieveFromTaxRateService()
        {

        }

        [Test]
        public void PlaceOrder_InStockProduct_RetrieveFromProductRepository()
        {

        }

        [Test]
        public void PlaceOrder_NotInStockProduct_CannotRetrieveFromProductRepository()
        {

        }
    }
}
