using Arweave.NET.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arweave.NET
{
	/// <summary>
	/// This class is used for data exchange with Arweave. Be careful!
	/// </summary>
	public class TransactionBuilder
	{

		[JsonPropertyName("format")]
		public int? Format { get; set; }


		[JsonPropertyName("last_tx")]
		public string LastTx { get; set; } = "";

		/// <summary>
		/// JWK.N
		/// </summary>
		public string Owner { get; set; } = "";

		[JsonPropertyName("tags")]
		public List<Tag>? Tags { get; set; }


		[JsonPropertyName("target")]
		public string Target { get; set; } = "";


		[JsonPropertyName("quantity")]
		[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
		public int Quantity { get; set; }

		[JsonIgnore]
		public byte[]? Data { get; set; }

		[JsonPropertyName("reward")]
		[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
		public int? Reward { get; set; }

		[JsonPropertyName("data_size")]
		[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
		public int DataSize { get; set; }


		[JsonPropertyName("data_root")]
		public string DataRoot { get; set; } = "";

		//public TransactionBuilder()
		//{

		//}

		//public TransactionBuilder(TransactionBuilder other)
		//{
		//	format = other.format;
		//	last_tx = other.last_tx;
		//	owner = other.owner;
		//	tags = other.tags;
		//	data_size = other.data_size;
		//	data = other.data;
		//	reward = other.reward;
		//	data_root = other.data_root;
		//	quantity = other.quantity;
		//	target = other.target;
		//}
	}
}
