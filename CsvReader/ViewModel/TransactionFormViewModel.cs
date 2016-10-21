using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CsvReader.Models;

namespace CsvReader.ViewModel
{
    public class TransactionFormViewModel
    {
        public Transaction Transaction { get; set; }

        IEnumerable<string> CurrencyCodes
        {
            get
            {
                return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(x => (new RegionInfo(x.LCID)).ISOCurrencySymbol)
                .Distinct()
                .OrderBy(x => x);
            }
        }  
    }
}