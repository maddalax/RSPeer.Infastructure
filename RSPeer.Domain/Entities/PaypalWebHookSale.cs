using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSPeer.Domain.Entities
{
    public class PaypalWebHookSale
    {
        public int Id { get; set; }
        public string PaymentId { get; set; }
        [Column(TypeName="json")]
        public string Value { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}