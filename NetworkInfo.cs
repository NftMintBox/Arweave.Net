using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public class NetworkInfo
	{
		public string? Network { get; set; }
		public int Version { get; set; }

		public int Release { get; set; }

		public int Height { get; set; }
		public string Current { get; set; } = "";

		public int Blocks { get; set; }

		public int Peers { get; set; }


		[JsonPropertyName("queue_length")]
		public int Queue_Length { get; set; }

		[JsonPropertyName("node_state_latency")]
		public int Node_State_Latency { get; set; }
	}
}
