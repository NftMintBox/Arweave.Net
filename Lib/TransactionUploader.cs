using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Arweave.NET.Lib
{

	public class TransactionUploader
	{
		public delegate Task TransactionUploaderAsyncEventHandler(object sender, TransactionUploaderEventArgs e);
		public delegate void TransactionUploaderEventHandler(object sender, TransactionUploaderEventArgs e);

		public event TransactionUploaderAsyncEventHandler? ProgressChangedAsync;
		public event TransactionUploaderEventHandler? ProgressChanged;

		// Maximum amount of chunks we will upload in the body.
		const int MAX_CHUNKS_IN_BODY = 1;

		const int ERROR_DELAY = 1000 * 40;

		List<string> FATAL_CHUNK_UPLOAD_ERRORS = new List<string> {
			  "invalid_json",
			  "chunk_too_big",
			  "data_path_too_big",
			  "offset_too_big",
			  "data_size_too_big",
			  "chunk_proof_ratio_not_attractive",
			  "invalid_proof",
		};

		int chunkIndex = 0;
		bool txPosted;
		Transaction transaction;
		int lastRequestTimeEnd;

		int totalErrors = 0; // Not serialized.

		byte[] data { get; set; }
		public int lastResponseStatus { get; set; }
		public string lastResponseError { get; set; } = "";

		DateTime dtStart = DateTime.UtcNow;

		JsonSerializerOptions options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
			DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
		};

		Api api;

		public TransactionUploader(Api api, Transaction transaction)
		{
			if (transaction.ID == null)
			{
				throw new Exception("Transaction is not signed");
			}
			if (transaction.Chunks == null)
			{
				throw new Exception("Transaction chunks not prepared");
			}

			
			this.data = transaction.Data;

			

			this.transaction = (Transaction)transaction.Clone();

			// Make a copy of transaction, zeroing the data so we can serialize.
			//FIXME: i don't understand this logic!

			this.transaction.Data = Array.Empty<byte>();

			this.api = api;
		}

		

		public bool IsComplete
		{
			get => txPosted && chunkIndex == transaction.Chunks!.chunks.Count;
		}

		public int TotalChunks => transaction.Chunks!.chunks.Count;

		public int UploadedChunks => chunkIndex;

		public int PctComplete => (int)Math.Truncate(((double)UploadedChunks / TotalChunks) * 100);

		internal async Task UploadChunk(int? chunkIndex, CancellationToken cancellationToken = default)
		{
			if (this.IsComplete)
			{
				throw new Exception("Upload is already complete");
			}

			if (this.lastResponseError != "")
			{
				this.totalErrors++;
			}
			else
			{
				this.totalErrors = 0;
			}

			// We have been trying for about an hour receiving an
			// error every time, so eventually bail.
			if (this.totalErrors == 100)
			{
				throw new Exception($"Unable to complete upload: { this.lastResponseStatus }: { this.lastResponseError}");
			}


			int delay = 0;

			if (lastResponseError != "")
			{

				delay = Math.Max((int)(lastRequestTimeEnd + ERROR_DELAY - DateTime.UtcNow.ToEpoch()), ERROR_DELAY);
			}

			///Thread.Sleep(delay);

			//FIXME: I don't like it
			await Task.Delay(delay);

			lastResponseError = "";

			if (!txPosted)
			{
				await PostTransactionAsync(cancellationToken);

				dtStart = DateTime.UtcNow;
				
				await OnProgressChangedAsync();
				OnProgressChanged();
				return;
			}


			if (chunkIndex.HasValue)
			{
				this.chunkIndex = chunkIndex.Value;
			}

			int idx = chunkIndex.GetValueOrDefault(this.chunkIndex);

			TransactionChunk chunk = transaction.GetChunk(idx, data);

			PathValidationResult chunkOk = await Merkle.ValidatePath(
				transaction.Chunks!.data_root,
				chunk.Offset,
				0,
				chunk.DataSize,
				Base64UrlEncoder.DecodeBytes(chunk.DataPath)
			);

			if (!chunkOk.IsValid)
			{
				throw new Exception($"Unable to validate chunk { this.chunkIndex }");
			}

			string json = JsonSerializer.Serialize(chunk, options);

			int responseStatus = 0;
			string errorMessage = "";

			try
			{
				using (StringContent content = new StringContent(json, Encoding.UTF8, "application/json"))
				{
					ApiResponse<string> response = await api.Post<string>("chunk", content, cancellationToken);
					responseStatus = (int)response.Status;
				}
			}
			catch (ApiResponseException ex)
			{
				responseStatus = (int)ex.Status;
				errorMessage = ex.Message;
			}

			this.lastRequestTimeEnd = (int)DateTime.UtcNow.ToEpoch();
			this.lastResponseStatus = responseStatus;

			if (responseStatus == 200)
			{
				this.chunkIndex++;
			}
			else
			{
				this.lastResponseError = errorMessage;

				if (FATAL_CHUNK_UPLOAD_ERRORS.Contains(this.lastResponseError.ToLower()))
				{
					throw new Exception($"Fatal error uploading chunk { this.chunkIndex }: { this.lastResponseError}");
				}
			}

			await OnProgressChangedAsync();
			OnProgressChanged();
		}

		/// <summary>
		///  POST to /tx
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		private async Task PostTransactionAsync(CancellationToken cancellationToken = default)
		{
			bool uploadInBody = TotalChunks <= MAX_CHUNKS_IN_BODY;

			if (uploadInBody)
			{
				// Post the transaction with data.

				this.transaction.Data = this.data;

				string json = JsonSerializer.Serialize(transaction, options);

				int responseStatus = 0;

				try
				{
					using (StringContent content = new StringContent(json, Encoding.UTF8, "application/json"))
					{
						ApiResponse<string> response = await api.Post<string>("tx", content, cancellationToken);
						responseStatus = (int)response.Status;
					}
				}
				catch (ApiResponseException ex)
				{
					responseStatus = (int)ex.Status;

					this.lastResponseError = ex.Message;
				}

				//fixme
				this.lastRequestTimeEnd = (int)DateTime.UtcNow.ToEpoch();
				this.lastResponseStatus = responseStatus;
				this.transaction.Data = Array.Empty<byte>();

				if (responseStatus >= 200 && responseStatus < 300)
				{
					// We are complete.
					this.txPosted = true;
					this.chunkIndex = MAX_CHUNKS_IN_BODY;
					return;
				}


				throw new Exception($"Unable to upload transaction: { responseStatus }, { this.lastResponseError}");
			}
			else
			{
				//transaction posted without data as data will then be uploaded by chucks

				string json = JsonSerializer.Serialize(transaction, options);

				int responseStatus = 0;
				string errorMessage = "";

				try
				{
					using (StringContent content = new StringContent(json, Encoding.UTF8, "application/json"))
					{
						ApiResponse<string> response = await api.Post<string>("tx", content, cancellationToken);
						responseStatus = (int)response.Status;
					}
				}
				catch (ApiResponseException ex)
				{
					responseStatus = (int)ex.Status;
					errorMessage = ex.Message;
				}

				//fixme
				lastRequestTimeEnd = (int)DateTime.UtcNow.ToEpoch();
				lastResponseStatus = responseStatus;
				transaction.Data = Array.Empty<byte>();

				if (!(responseStatus >= 200 && responseStatus < 300))
				{
					this.lastResponseError = errorMessage;
					throw new Exception($"Unable to upload transaction: { responseStatus }, { this.lastResponseError}");
				}

				this.txPosted = true;

			}
		}



		internal static async Task<SerializedUploader> FromTransactionID(Api api, string transactionID, CancellationToken cancellationToken)
		{
			ApiResponse<TransactionBuilder> response;

			try
			{
				response = await api.Get<TransactionBuilder>($"tx/{transactionID}", cancellationToken);
			}
			catch (ApiResponseException ex)
			{
				throw new Exception($"Tx ${transactionID} not found: ${ex.Status}");
			}

			TransactionBuilder transaction = response.Data!;

			SerializedUploader serializedUploader = new SerializedUploader
			{
				txPosted = true,
				chunkIndex = 0,
				lastResponseError = "",
				lastRequestTimeEnd = 0,
				lastResponseStatus = 0,
				transaction = transaction
			};

			return serializedUploader;
		}

		internal static async Task<TransactionUploader> FromSerialized(Api api, SerializedUploader serialized, byte[] data)
		{
			if (serialized == null ||
				!serialized.chunkIndex.HasValue ||
				serialized.transaction == null)
			{
				throw new Exception("Serialized object does not match expected format.");
			}


			// Everything looks ok, reconstruct the TransactionUpload,
			// prepare the chunks again and verify the data_root matches
			var transaction = new Transaction(serialized.transaction, data);

			if (transaction.Chunks == null)
			{
				transaction.PrepareChunks(data);
			}

			var uploader = new TransactionUploader(api, transaction);

			// Copy the serialized upload information, and data passed in.
			uploader.chunkIndex = serialized.chunkIndex.Value;
			uploader.lastRequestTimeEnd = serialized.lastRequestTimeEnd;
			uploader.lastResponseError = serialized.lastResponseError;
			uploader.lastResponseStatus = serialized.lastResponseStatus;
			uploader.txPosted = serialized.txPosted;
			
			//FIXME: i don't understand why data is set in so many places
			//uploader.data = data;

			if (uploader.transaction.DataRoot != serialized.transaction.DataRoot)
			{
				throw new Exception("Data mismatch: Uploader doesn't match provided data.");

			}

			return uploader;
		}


		protected virtual async Task OnProgressChangedAsync()
		{

			if (ProgressChangedAsync != null)
			{
				await ProgressChangedAsync.Invoke(this, new TransactionUploaderEventArgs
				{
					CurrentProgress = this.PctComplete,
					ExecutionTime = DateTime.UtcNow - dtStart,
					ErrorMessage = lastResponseError,
					TransactionID = transaction.ID,
					TotalChunks = this.TotalChunks,
					ChunksUploaded = this.UploadedChunks
				});;
			}
		}


		protected virtual void OnProgressChanged()
		{
			
			if (ProgressChanged != null)
			{
				ProgressChanged?.Invoke(this, new TransactionUploaderEventArgs
				{
					CurrentProgress = this.PctComplete,
					ExecutionTime = DateTime.UtcNow - dtStart,
					ErrorMessage = lastResponseError,
					TransactionID = transaction.ID,
					TotalChunks = this.TotalChunks,
					ChunksUploaded = this.UploadedChunks
				});
			}
		}

		public async Task PostAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				while (!IsComplete)
				{
					await UploadChunk(null, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				if (lastResponseStatus > 0)
				{
					throw new ApiResponseException(lastResponseError, ex, (HttpStatusCode)lastResponseStatus, lastResponseError);
				}
			}
		}
	}
}