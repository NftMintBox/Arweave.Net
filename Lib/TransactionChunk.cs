using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public class TransactionChunk
	{
		[JsonPropertyName("data_root")]	
		public string? DataRoot { get; private set; }

		[JsonIgnore]
		public int DataSize { get; private set; }


		[JsonPropertyName("data_size")]
		public string _dataSize { get => DataSize.ToString(); }

		[JsonPropertyName("data_path")]
		public string DataPath { get; private set; }

		[JsonIgnore]
		public int Offset { get; private set; }

		[JsonPropertyName("offset")]
		public string _offset { get => Offset.ToString(); }


		[JsonIgnore]
		public byte[] Data { get; set; } = Array.Empty<byte>();

		[JsonPropertyName("chunk")]
		public string __data { get => Base64UrlEncoder.Encode(Data); }

		//public string Chunk { get; private set; }

		//public TransactionChunk(string? dataRoot, long dataSize, string dataPath, long offset, string chunk)
		//{
		//	DataPath = dataPath;
		//	DataSize = dataSize;
		//	DataRoot = dataRoot;
		//	Offset = offset;
		//	Chunk = chunk;

		//}


		public TransactionChunk(string? dataRoot, int dataSize, string dataPath, int offset, byte[] data)
		{
			DataPath = dataPath;
			DataSize = dataSize;
			DataRoot = dataRoot;
			Offset = offset;
			Data = data;
		}
	}
}
