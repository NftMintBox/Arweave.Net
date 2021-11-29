using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public interface ITransaction
	{
		/// <summary>
		/// Currently supported formats are 1 and 2 (often referred to as v1 and v2 respectively). The v1 format is deprecated.
		/// Integer
		/// </summary>
		public int Format { get;  }

		/// <summary>
		/// A SHA-256 hash of the transaction signature.
		/// Base64URL string. 
		/// </summary>
		public string? ID { get;  }

		/// <summary>
		/// An anchor - a protection against replay attacks. It may be either a hash of one of the last 50 blocks or the last outgoing transaction ID from the sending wallet. If this is the first transaction from the wallet then an empty string may be used. 
		/// The recommended way is to use the value returned by GET /tx_anchor. Two different transactions can have the same last_tx if a block hash is used.
		/// Base64URL string
		/// </summary>
		public string? LastTX { get;  }

		/// <summary>
		/// The full RSA modulus value of the sending wallet. The modulus is the n value from the JWK. The RSA public key.
		/// Base64URL string
		/// </summary>
		public string? Owner { get;  }


		/// <summary>
		/// A list of name-value pairs, each pair is serialized as {"name": "a BaseURL string", "value":" a Base64URL string"}
		/// If no tags are being used then use an empty array []. The total size of the names and values may not exceed 2048 bytes. 
		/// Tags might be useful for attaching a message to a transaction sent to another wallet, for example a reference number or identifier to help account for the transaction.
		/// </summary>
		public List<Tag> Tags { get;  }


		/// <summary>
		/// The target address to send tokens to (if required). If no tokens are being transferred to another wallet then use an empty string. Note that sending tokens to the owner address is not supported. The address is the SHA-256 hash of the RSA public key.
		/// Base64URL string
		/// </summary>
		public string? Target { get;  } 

		/// <summary>
		/// The amount to transfer from the owner wallet to the target wallet address. If no tokens are being transferred then use an empty string.
		/// A winston string
		/// </summary>
		public string? quantity { get;  }

		/// <summary>
		/// The data to be submitted. If no data is being submitted then use an empty string. 
		/// For v2 transactions there is no need, although it is possible, to use this field even if there is data (means,data_size > 0 and data_root is not empty). 
		/// In v1 transactions, data cannot be bigger than 10 MiB. In v2 transactions, the limit is decided by the nodes. At the time this was written, all nodes in the network accept up to 12 MiB of data via this field.
		/// Base64URL string
		/// </summary>
		public byte[]? Data { get;  }

		/// <summary>
		/// The transaction fee. See the price endpoint docs for more info.
		/// A winston string​
		/// </summary>
		public string? reward { get;  }

		/// <summary>
		/// An RSA signature of a merkle root of the SHA-384 hashes of transaction fields (except for id, which is the hash of the signature). See Transaction Signing for more.
		/// Base64URL string
		/// </summary>
		public string? signature { get;  }

		/// <summary>
		/// The size in bytes of the transactino data. Use "0" if there is no data. The string representation of the number must not exceed 21 bytes.
		/// Only use with v2 transactions. 
		/// String
		/// </summary>
		public string? data_size { get;  }

		/// <summary>
		/// Only use with v2 transactions. The merkle root of the transaction data. If there is no data then use an empty string.
		/// Base64URL string
		/// </summary>
		public string? data_root { get;  }

}
	public class Transaction : ICloneable
	{
		/// <summary>
		/// Currently supported formats are 1 and 2 (often referred to as v1 and v2 respectively). The v1 format is deprecated.
		/// Integer
		/// </summary>
		[JsonPropertyName("format")]
		public int Format { get; internal set; } = 2;


		/// <summary>
		/// A SHA-256 hash of the transaction signature.
		/// Base64URL string. 
		/// </summary>
		[JsonPropertyName("id")]
		public string ID { get; private set; } = "";

		/// <summary>
		/// An anchor - a protection against replay attacks. It may be either a hash of one of the last 50 blocks or the last outgoing transaction ID from the sending wallet. 
		/// If this is the first transaction from the wallet then an empty string may be used. 
		/// The recommended way is to use the value returned by GET /tx_anchor. Two different transactions can have the same last_tx if a block hash is used.
		/// Base64URL string
		/// </summary>
		[JsonPropertyName("last_tx")]
		public string LastTx { get; set; } = "";

		/// <summary>
		/// The full RSA modulus value of the sending wallet. The modulus is the n value from the JWK. The RSA public key.
		/// Base64URL string
		/// </summary>
		[JsonPropertyName("owner")]
		public string Owner { get; internal set; } = "";


		/// <summary>
		/// A list of name-value pairs, each pair is serialized as {"name": "a BaseURL string", "value":" a Base64URL string"}
		/// If no tags are being used then use an empty array []. The total size of the names and values may not exceed 2048 bytes. 
		/// Tags might be useful for attaching a message to a transaction sent to another wallet, for example a reference number or identifier to help account for the transaction.
		/// </summary>
		[JsonPropertyName("tags")]
		public List<Tag> Tags { get; private set; } = new List<Tag>();


		/// <summary>
		/// The target address to send tokens to (if required). If no tokens are being transferred to another wallet then use an empty string. 
		/// Note that sending tokens to the owner address is not supported. 
		/// The address is the SHA-256 hash of the RSA public key.
		/// Base64URL string
		/// </summary>
		[JsonPropertyName("target")]
		public string Target { get; private set; } = "";

		/// <summary>
		/// The amount to transfer from the owner wallet to the target wallet address. If no tokens are being transferred then use an empty string.
		/// A winston string
		/// </summary>
		[JsonPropertyName("quantity")]
		[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
		public int? Quantity { get; private set; }


		/// <summary>
		/// The size in bytes of the transactinon data. Use "0" if there is no data. 
		/// The string representation of the number must not exceed 21 bytes.
		/// Only use with v2 transactions. 
		/// String
		/// </summary>
		[JsonPropertyName("data_size")]
		[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
		public int DataSize { get; private set; }

		/// <summary>
		/// The data to be submitted. If no data is being submitted then use an empty string. 
		/// For v2 transactions there is no need, although it is possible, to use this field even if there is data (means,data_size > 0 and data_root is not empty). 
		/// In v1 transactions, data cannot be bigger than 10 MiB. In v2 transactions, the limit is decided by the nodes. 
		/// At the time this was written, all nodes in the network accept up to 12 MiB of data via this field.
		/// Base64URL string
		/// </summary>
		[JsonPropertyName("data")]
		[JsonConverter(typeof(JsonBase64UrlToByteConverter))]
		public byte[] Data { get; set; } = Array.Empty<byte>();


		/// <summary>
		/// The transaction fee. See the price endpoint docs for more info.
		/// A winston string​
		/// </summary>
		[JsonPropertyName("reward")]
		[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
		public int? Reward { get; internal set; }

		/// <summary>
		/// An RSA signature of a merkle root of the SHA-384 hashes of transaction fields (except for id, which is the hash of the signature). 
		/// See Transaction Signing for more.
		/// Base64URL string
		/// </summary>
		[JsonPropertyName("signature")]
		public string? Signature { get; private set; }

		/// <summary>
		/// Only use with v2 transactions. The merkle root of the transaction data. If there is no data then use an empty string.
		/// Base64URL string
		/// </summary>
		[JsonPropertyName("data_root")]
		[JsonInclude]
		public string DataRoot { get; private set; } = String.Empty;

		[JsonIgnore]
		internal Chunks? Chunks { get; private set; }

		public TransactionStatus? CurrentStatus { get; set; }

		private Transaction() {}
		
		public Transaction(TransactionBuilder builder, byte[] data):this(builder)
		{
			Data = data;
			DataSize = data.Length;

			PrepareChunks(data);
		}

		internal Transaction(TransactionBuilder builder)
		{
			if (builder.Format != null) Format = builder.Format.Value;

			LastTx = builder.LastTx;
			Owner = builder.Owner;
			DataSize = builder.DataSize;

			if (builder.Data != null)
			{
				Data = (byte[])builder.Data.Clone();
			}

			Reward = builder.Reward;
			DataRoot = builder.DataRoot;
			Quantity = builder.Quantity;
			Target = builder.Target;

			if (builder.Tags != null)
			{
				foreach (var tag in builder.Tags)
				{
					Tags.Add(new Tag(tag.Name, tag.Value));
				}
			}
		}

		//public void SetOwner(JsonWebKey jwk)
		//{
		//	Owner = jwk.N;
		//}


		public void SetSignature(string id, string owner, string signature, int? reward = null, List<Tag>? tags = null)
		{
			ID = id;
			Owner = owner;

			if (tags != null)
			{
				Tags = tags;
			}

			if (reward != null) 
			{ 
				Reward = reward.Value; 
			}

			Signature = signature;
		}

		public byte[] GetSignatureData()
		{
			switch (Format)
			{
				case 1:
					throw new NotImplementedException();
				case 2:
					if (DataRoot == null) 
					{ 
						PrepareChunks(Data!); 
					}
					
					List<byte[]> tagList = new List<byte[]>();

					foreach (var tag in Tags)
					{
						tagList.Add(Base64UrlEncoder.DecodeBytes(Base64UrlEncoder.Encode(tag.Name)));
						tagList.Add(Base64UrlEncoder.DecodeBytes(Base64UrlEncoder.Encode(tag.Value)));
					}

					if (!Reward.HasValue)
					{
						throw new ArgumentNullException("Reward");
					}

					string quantity = "";

					if (Quantity.HasValue) quantity = Quantity.Value.ToString();

					List<byte[]> all = new List<byte[]>
					{
						Encoding.UTF8.GetBytes(Format.ToString()),
						Base64UrlEncoder.DecodeBytes(Owner),
						Base64UrlEncoder.DecodeBytes(Target),
						Encoding.UTF8.GetBytes(quantity),
						Encoding.UTF8.GetBytes(Reward.Value.ToString()),
						Base64UrlEncoder.DecodeBytes(LastTx),
					};

					all.AddRange(tagList);

					all.AddRange(new[]{
						Encoding.UTF8.GetBytes(DataSize.ToString()),
						Base64UrlEncoder.DecodeBytes(DataRoot)
					});

					//string json = JsonSerializer.Serialize(all, new JsonSerializerOptions
					//{
					//	DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
				
					//});

					byte[] result = DHasher.DeepHash(all);

					return result;

				default:
					throw new Exception($"Unexpected transaction format: { Format }");
			}
		}

		internal void PrepareChunks(byte[] data) 
		{
			// Note: we *do not* use `this.data`, the caller may be
			// operating on a transaction with an zero length data field.
			// This function computes the chunks for the data passed in and
			// assigns the result to this transaction. It should not read the
			// data *from* this transaction.

			if (Chunks == null && data.Length > 0)
			{
				Chunks = Merkle.GenerateTransactionChunks(data);
				DataRoot = Base64UrlEncoder.Encode(Chunks.data_root);
			}

			if (Chunks == null && data.Length == 0)
			{
				Chunks = Chunks.Empty;
				DataRoot = "";
			}
		}

		public TransactionChunk GetChunk(int idx, byte[] data)
		{
			if (Chunks == null)
			{
				throw new Exception("Chunks have not been prepared");
			}

			ProofHash proof = Chunks.proofs[idx];
			Chunk chunk = Chunks.chunks[idx];

			return new TransactionChunk(
				dataRoot: DataRoot,
				dataSize: DataSize,
				dataPath: Base64UrlEncoder.Encode(proof.Proof),
				offset: proof.Offset,
				data: data.Take(new Range(chunk.MinByteRange, chunk.MaxByteRange)).ToArray()
				//chunk: Base64UrlEncoder.Encode(data.Take(new Range(chunk.minByteRange, chunk.maxByteRange)).ToArray())
			);

			//return new
			//{
			//	data_root = data_root,
			//	data_size = data_size,
			//	data_path = Base64UrlEncoder.Encode(proof.proof),
			//	offset = proof.offset.ToString(),
			//	chunk = Base64UrlEncoder.Encode(data.Take(new Range(chunk.minByteRange, chunk.maxByteRange)).ToArray())
			//};
		}

		public object Clone()
		{
			Transaction newTransaction = new Transaction();

			newTransaction.Data = (byte[])Data.Clone();
			newTransaction.DataRoot = DataRoot;
			newTransaction.DataSize = DataSize;
			newTransaction.Format = Format;
			newTransaction.ID = ID;
			newTransaction.LastTx = LastTx;
			newTransaction.Owner = Owner;
			newTransaction.Quantity = Quantity;
			newTransaction.Reward = Reward;
			newTransaction.Signature = Signature;
			newTransaction.Target = Target;

			if (Chunks != null)
			{
				newTransaction.Chunks = (Chunks)Chunks.Clone();
			}

			if (Tags != null)
			{
				newTransaction.Tags = new List<Tag>();
				foreach (var item in Tags)
				{
					newTransaction.Tags.Add(new Tag(item.Name, item.Value));
				}
			}

			return newTransaction;
		
		}

		//public JsonDocument Serialize()
		//{
		//	using (MemoryStream memoryStream = new MemoryStream())
		//	using (Utf8JsonWriter doc = new Utf8JsonWriter(memoryStream))
		//	{
		//		doc.WriteStartObject();
				
		//	}
		//}
	}
}
