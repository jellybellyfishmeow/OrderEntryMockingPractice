using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void PlaceOrder_ValidOrderAllItemsUnique_NoExceptionThrown()
        {

        }

        [Test]
        public void PlaceOrder_InvalidOrderNotAllItemsUnique_ExceptionThrown()
        {

        }

        [Test]
        public void PlaceOrder_ValidOrderAllProductsInStock_NoExceptionThrown()
        {

        }

        [Test]
        public void PlaceOrder_InvalidOrderNotAllProductsInStock_ExceptionThrown()
        {

        }

        [Test]
        public void PlaceOrder_ValidOrder_ReturnsOrderSummary()
        {

        }

        [Test]
        public void PlaceOrder_ValidOrder_OrderFulfillmentServiceCalled()
        {


        }

        [Test]
        public void PlaceOrder_ValidOrder_ContainsFulfillmentConfirmationNumber()
        {

        }

        [Test]
        public void PlaceOrder_ValidOrder_ContainsFulfillmentId()
        {


        }
        [Test]
        public void PlaceOrder_ValidOrder_ContainsCorrectTaxes()
        {

        }

        [Test]
        public void PlaceOrder_ValidOrder_ConfirmationEmailSent()
        {

        }

        [Test]
        public void PlaceOrder_ValidOrder_CorrectNetTotal()
        {

        }
        [Test]
        public void PlaceOrder_ValidOrder_CorrectOrderTotal()
        {

        }

        [Test]
        public void PlaceOrder_ValidCustomer_RetrieveFromCustomerRepository()
        {

        }

        [Test]
        public void PlaceOrder_InvalidCustomer_CannotRetrieveFromCustomerRepository()
        {

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
