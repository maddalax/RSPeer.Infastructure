using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;

namespace RSPeer.Application.Features.Store.Paypal.Models
{
	public class PaypalCreatedOrder
	{
		public int Id { get; set; }
		public int UserId { get; set; }

		[JsonPropertyName("id")] public string PaypalId { get; set; }

		public bool IsSandbox { get; set; }

		[JsonPropertyName("intent")] public string Intent { get; set; }

		[JsonPropertyName("state")] public string State { get; set; }

		[JsonPropertyName("payer")] public Payer Payer { get; set; }

		[JsonPropertyName("transactions")] public List<Transaction> Transactions { get; set; }

		[JsonPropertyName("note_to_payer")] public string NoteToPayer { get; set; }

		[JsonPropertyName("create_time")] public DateTimeOffset CreateTime { get; set; }

		[JsonPropertyName("links")] public List<Link> Links { get; set; }
	}

	public class Link
	{
		[JsonPropertyName("href")] public string Href { get; set; }

		[JsonPropertyName("rel")] public string Rel { get; set; }

		[JsonPropertyName("method")] public string Method { get; set; }
	}
}