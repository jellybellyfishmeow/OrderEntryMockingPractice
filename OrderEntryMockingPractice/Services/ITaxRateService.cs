using Moq;
using System.Collections.Generic;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPractice.Services
{
    public interface ITaxRateService
    {
        IEnumerable<TaxEntry> GetTaxEntries(string postalCode, string country);
    }
}