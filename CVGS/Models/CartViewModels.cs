using CVGS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CVGS.Models
{
    public class CartViewModel
    {
        public List<Game> GamesInCart { get; set; }
        public decimal Total { get; set; }
        public decimal TotalWithTaxes { get; set; }
    }

    public class CheckoutViewModel : CartViewModel
    {
        public string PaymentMethodId { get; set; }
        public string CreditCardLast4 { get; set; }
        public List<CreditCard> CreditCards { get; set; }
        public decimal TaxTotal { get; set; }
    }
}