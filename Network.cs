using Arweave.NET.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arweave.NET
{
	
	public class Network
	{
		private Api api;

		public Network(Api api)
		{
			this.api = api;
		}

		public async Task<NetworkInfo> GetInfo(CancellationToken cancellationToken = default)
		{
			ApiResponse<NetworkInfo> response = await api.Get<NetworkInfo>("info", cancellationToken);

			return response.Data;
		}

		public async Task<List<string>> GetPeers(CancellationToken cancellationToken = default)
		{
			ApiResponse<List<string>> result = await api.Get<List<string>>("peers", cancellationToken);

			return result.Data;
		}
		
	}
}

