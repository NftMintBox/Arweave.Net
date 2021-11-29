using Arweave.NET.Lib;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public enum TransactionStatus { OK = 200, Pending = 202, InvalidHash = 400, NotFound = 404 }
	public class TransactionConfirmedData
	{
		[JsonPropertyName("block_indep_hash")]
		public string? BlockIndepHash { get; set; }

		[JsonPropertyName("block_height")]
		public int BlockHeight { get; set; }

		[JsonPropertyName("number_of_confirmations")]
		public int NumberOfConfirmations { get; set; }
	}

	public class TransactionStatusResponse
	{
		public TransactionStatus Status { get; set; }	
		public TransactionConfirmedData? Confirmed { get; set; }
	}

	public class Transactions
	{
		Api api;
		ICryptoInterface crypto;
		Chunks chunks;

		public Transactions(Api api, ICryptoInterface crypto, Chunks chunks)
		{
			this.api = api;
			this.crypto = crypto;
			this.chunks = chunks;
		}

		public async Task<string> GetTransactionAnchor(CancellationToken cancellationToken = default)
		{
			return await api.GetStringResponse("tx_anchor", cancellationToken);
		}

		public async Task<int> GetPriceAsync(int byteSize, string? targetAddress, CancellationToken cancellationToken = default)
		{
			string endpoint = $"price/{byteSize}";

			if (targetAddress != null)
			{
				endpoint = $"price/{byteSize}/{targetAddress}";
			}

			string price = await api.GetStringResponse(endpoint, cancellationToken);

			return Int32.Parse(price);
		}

		public async Task<Transaction> Get(string id, CancellationToken cancellationToken = default)
		{
			ApiResponse<TransactionBuilder> response = await api.Get<TransactionBuilder>($"tx/{id}");


			if (response.Status == System.Net.HttpStatusCode.OK)
			{
				Transaction transaction;
				
				int data_size = response.Data!.DataSize;

				if (
					  response.Data.Format >= 2 && 
					  data_size > 0 &&
					  data_size <= 1024 * 1024 * 12
				)
				{
					byte[] data = await GetData(id, cancellationToken);

					transaction = new Transaction(response.Data, data);

				}

				transaction = new Transaction(response.Data)
				{
					//FIXME
					Format = response.Data.Format ?? 1
				};

				transaction.CurrentStatus = (TransactionStatus)(int)response.Status;
				return transaction;

			}

			if (response.Status == System.Net.HttpStatusCode.NotFound)
			{
				throw new ArweaveError(ArweaveErrorType.TX_NOT_FOUND);
			}

			if (response.Status == System.Net.HttpStatusCode.Gone)
			{
				throw new ArweaveError(ArweaveErrorType.TX_FAILED);
			}

			throw new ArweaveError(ArweaveErrorType.TX_INVALID);

		}


		public async Task<List<string>?> Search(string tagName, string tagValue, CancellationToken cancellationToken)
		{
			object body = new
			{
				op = "equals",
				expr1 = tagName,
				expr2 = tagValue
			};

			string json = JsonSerializer.Serialize(body);

			using (StringContent content = new StringContent(json, Encoding.UTF8, "application/json"))
			{
				ApiResponse<List<string>> response = await api.Post<List<string>>("arql", content, cancellationToken);

				return response.Data;
			}		
		}

		public async Task<TransactionStatusResponse> GetStatus(string id, CancellationToken cancellationToken)
		{
			try
			{
				ApiResponse<TransactionConfirmedData> response = await api.Get<TransactionConfirmedData>($"tx/{id}/status", cancellationToken);

				return new TransactionStatusResponse
				{
					Confirmed = response.Data,
					Status = (TransactionStatus)response.Status
				};

			}
			catch (ApiResponseException err)
			{
				return new TransactionStatusResponse
				{
					Status = (TransactionStatus)err.Status
				};
			}

		}

		public async Task<byte[]> GetData(string id, CancellationToken cancellationToken)
		{
			// Only download from chunks, while /{txid} may work
			// it may give false positive about the data being seeded
			// if getData is problematic, please consider fetch-ing
			// an arweave gateway directly!

			return await chunks.DownloadChunkedData(id, cancellationToken);
		}

		public async Task<string> GetData(string id, bool decode, bool isString)
		{
			//not sure this is a good approach to hide decoding inside method call. Porting posponed

			/*
			 * 
			if (options && options.decode && options.string) {
				return ArweaveUtils.bufferToString(data);
			}
			// Since decode wasn't requested, caller expects b64url encoded data.
			return ArweaveUtils.bufferTob64Url(data);

			*/
			throw new NotImplementedException();
		}

		public void Sign(Transaction transaction, JsonWebKey jwk)
		{
			//transaction.SetOwner(jwk);

			byte[] dataToSign = transaction.GetSignatureData();
			byte[] rawSignature = crypto.Sign(jwk, dataToSign);
			byte[] id = crypto.Hash(rawSignature);

			transaction.SetSignature(
				id: Base64UrlEncoder.Encode(id),
				owner: jwk.N,
				signature: Base64UrlEncoder.Encode(rawSignature)
			);
			
		}

		public bool Verify(Transaction transaction)
		{
			var signaturePayload = transaction.GetSignatureData();

			/**
			* The transaction ID should be a SHA-256 hash of the raw signature bytes, so this needs
			* to be recalculated from the signature and checked against the transaction ID.
			*
			*
				const rawSignature = transaction.get("signature", {
					decode: true,
				  string: false,
				});

			*/

			byte[] rawSignature = Base64UrlEncoder.DecodeBytes(transaction.Signature);

			string expectedID = Base64UrlEncoder.Encode(crypto.Hash(rawSignature));

			if (transaction.ID != expectedID)
			{
				throw new Exception("Invalid transaction signature or ID! The transaction ID doesn't match the expected SHA-256 hash of the signature.");
			}

			return crypto.Verify(
				 transaction.Owner,
				 signaturePayload,
				 rawSignature
			   );
		}

		public async Task PostAsync(Transaction transaction, CancellationToken cancellationToken = default)
		{
			if (transaction.Chunks == null)
			{
				transaction.PrepareChunks(transaction.Data);
			}

			TransactionUploader uploader = GetUploader(transaction, transaction.Data);
			
			try
			{
				while (!uploader.IsComplete)
				{
					await uploader.UploadChunk(null, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				if (uploader.lastResponseStatus > 0)
				{
					throw new ApiResponseException(uploader.lastResponseError, ex, (HttpStatusCode)uploader.lastResponseStatus, uploader.lastResponseError);
				}
			}
		}

		public TransactionUploader GetUploader(Transaction transaction, byte[]? data = null)
		{
			
			if (data == null)
			{
				data = transaction.Data;
			}

			if (transaction.Chunks == null)
			{
				transaction.PrepareChunks(data);
			}

			TransactionUploader uploader = new TransactionUploader(this.api, transaction);

			//that's very strange!!

			//if (uploader.data == null || uploader.data.Length == 0)
			//{
			//	uploader.data = data;
			//}

			return uploader;
		} 
	

		public async Task<TransactionUploader> GetUploader(string transactionID, byte[] data, CancellationToken cancellationToken)
		{
			SerializedUploader upload = await TransactionUploader.FromTransactionID(api, transactionID, cancellationToken);

			return await GetUploader(upload, data);
		}

		public async Task<TransactionUploader> GetUploader(SerializedUploader upload, byte[] data)
		{
			if (data == null)
			{
				throw new Exception("You must provide data when resuming upload");
			}

			TransactionUploader uploader = await TransactionUploader.FromSerialized(
				this.api,
				upload,
				data
			);

			return uploader;
		}

		/// <summary>
		/// Async generator version of uploader
		/// 
		/// Usage example:
		///    
		///    for await(const uploader of arweave.transactions.upload(tx)) 
		///    {
		///			console.log(`${uploader.pctComplete}%`);
		///    }
		/// </summary>
		/// <param name="upload">a Transaction object, a previously save uploader, or a transaction id</param>
		/// <param name="data">the data of the transaction. Required when resuming an upload.</param>
		/// <returns></returns>
		public async Task UploadAsync(Transaction upload, byte[] data)
		{
			throw new NotImplementedException();

			//const uploader = await this.getUploader(upload, data);

			//while (!uploader.isComplete)
			//{
			//	await uploader.uploadChunk();
			//	yield uploader;
			//}

			//return uploader;

			
		}
	}
}
