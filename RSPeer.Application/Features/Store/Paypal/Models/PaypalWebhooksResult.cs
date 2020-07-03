using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;

namespace RSPeer.Application.Features.Store.Paypal.Models
{
    public class PaypalWebhooksResult
    {
        public List<Link> Links { get; set; }
        public int Coun { get; set; }
        public List<PaypalWebHook> Events { get; set; }
    }

     public class PaypalWebHook
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("create_time")]
        public DateTimeOffset CreateTime { get; set; }

        [JsonProperty("resource_type")]
        public string ResourceType { get; set; }

        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("resource")]
        public Amazon.Auth.AccessControlPolicy.Resource Resource { get; set; }

        [JsonProperty("links")]
        public List<Link> Links { get; set; }

        [JsonProperty("event_version")]
        public string EventVersion { get; set; }
    }
     
    public class WebhookResource
    {
        [JsonProperty("amount")]
        public Amount Amount { get; set; }

        [JsonProperty("payment_mode")]
        public string PaymentMode { get; set; }

        [JsonProperty("create_time")]
        public DateTimeOffset CreateTime { get; set; }

        [JsonProperty("transaction_fee")]
        public TransactionFee TransactionFee { get; set; }

        [JsonProperty("parent_payment")]
        public string ParentPayment { get; set; }

        [JsonProperty("update_time")]
        public DateTimeOffset UpdateTime { get; set; }

        [JsonProperty("soft_descriptor")]
        public string SoftDescriptor { get; set; }

        [JsonProperty("protection_eligibility_type")]
        public string ProtectionEligibilityType { get; set; }

        [JsonProperty("protection_eligibility")]
        public string ProtectionEligibility { get; set; }

        [JsonProperty("links")]
        public List<Link> Links { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("invoice_number")]
        public string InvoiceNumber { get; set; }
    }

    public class WebhookAmount
    {
        [JsonProperty("total")]
        public string Total { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("details")]
        public Details Details { get; set; }
    }

    public class WebhookDetails
    {
        [JsonProperty("subtotal")]
        public string Subtotal { get; set; }
    }

    public class WebhookTransactionFee
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

 
}