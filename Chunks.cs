using Arweave.NET.Lib;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace Arweave.NET
{
	internal class TransactionOffsetResponse
	{
		////fixme
		//public string size { get; set; } = "0";
		//public string offset { get; set; } = "0";

		[JsonPropertyName("size")]
		[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
		public long Size { get; set; }

		[JsonPropertyName("offset")]
		[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
		public long Offset { get; set; }
	}

	internal class TransactionChunkResponse
	{
		public string? chunk { get; set; }
		
		public string? data_path { get; set; }

		public string? tx_pat { get; set; }

	}


	/// <summary>
	/// Implemented
	/// </summary>
	public class Chunks
	{
		Api api;

		public Chunks(Api api) 
		{ 
			this.api = api;
		}

		internal async Task<TransactionOffsetResponse> GetTransactionOffset(string id, CancellationToken cancellationToken)
		{
			ApiResponse<TransactionOffsetResponse> response = await api.Get<TransactionOffsetResponse>($"tx/{id}/offset", cancellationToken);

			if (response.Data == null) throw new Exception($"Unable to get transaction offset. Transaction ID: {id}");

			return response.Data;			
		}

		internal async Task<TransactionChunkResponse> GetChunk(long offset, CancellationToken cancellationToken)
		{
			ApiResponse<TransactionChunkResponse> response = await api.Get<TransactionChunkResponse>($"chunk/{offset}", cancellationToken);

			if (response.Data == null) throw new Exception($"Unable to get chunk at offset {offset}");

			return response.Data;
		}

		internal async Task<byte[]> GetChunkData(long offset, CancellationToken cancellationToken)
		{
			TransactionChunkResponse tr = await GetChunk(offset, cancellationToken);

			//FIXME: what if null?
			return Base64UrlEncoder.DecodeBytes(tr.chunk);
		}

		public async Task<byte[]> DownloadChunkedData(string id, CancellationToken cancellationToken)
		{
			TransactionOffsetResponse offsetResponse = await GetTransactionOffset(id, cancellationToken);

			long size = offsetResponse.Size;
			long endOffset = offsetResponse.Offset;
			
			long startOffset = endOffset - size + 1;

			byte[] data = new byte[size];

			int b = 0;

			while (b < size)
			{
				byte[] chunkData;
				try
				{
					chunkData = await GetChunkData(startOffset + b, cancellationToken);
				}
				catch (Exception ex)
				{

					throw new Exception($"Failed to fetch chunk at offset { startOffset + b}. This could indicate that the chunk wasn't uploaded or hasn't yet seeded properly to a particular gatway/ node", ex);
				}

				if (chunkData != null)
				{
					data.SetValue(chunkData, b);
					b += chunkData.Length;
				}
				else
				{
					throw new Exception("Coudn't complete data download at ${byte}/${size}");
		
				}
			}

			return data;
		}

	}
}
