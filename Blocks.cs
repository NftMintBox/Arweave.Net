using Arweave.NET.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public class BlockData
	{

		public string? nonce { get; set; }

		public string? previous_block { get; set; }
		public int timestamp { get; set; }
		public int last_retarget { get; set; }
		public string? diff { get; set; }
		public int number { get; set; }
		public string? hash { get; set; }

		public string? indep_hash { get; set; }
		public string[]? txs { get; set; }
		public string? tx_root { get; set; }
		public string? wallet_list { get; set; }

		public string? reward_addr { get; set; }

		public Tag[]? tags { get; set; }
		public int reward_pool { get; set; }
		public int weave_size { get; set; }
		public int block_size { get; set; }
		public string? cumulative_diff { get; set; }
		public string? hash_list_merkle { get; set; }
	}

	public class Blocks
	{
		private static readonly string ENDPOINT = "block/hash/";

		Api api;
		Network network;

		public Blocks(Api api, Network network)
		{
			this.api = api;
			this.network = network;
		}

		public async Task<BlockData> Get(string indepHash, CancellationToken cancellationToken = default)
		{
			ApiResponse<BlockData> response = await api.Get<BlockData>($"{Blocks.ENDPOINT}{indepHash}", cancellationToken);

			if (response.Status == System.Net.HttpStatusCode.OK)
			{
				return response.Data;
			}
			else if (response.Status == System.Net.HttpStatusCode.NotFound)
			{
				throw new ArweaveError(ArweaveErrorType.BLOCK_NOT_FOUND);
			}
			else
			{
				throw new Exception($"Error while loading block data: { response}");
			}

		}

		public async Task<BlockData> GetCurrent(CancellationToken cancellationToken = default)
		{
			NetworkInfo networkInfo = await this.network.GetInfo(cancellationToken);

			return await Get(networkInfo.Current, cancellationToken);
		}
	}
}
