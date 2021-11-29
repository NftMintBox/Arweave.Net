using Arweave.NET.Lib;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public class Wallets
	{
		ICryptoInterface crypto;
		Api api;

		public Wallets(Api api, ICryptoInterface crypto)
		{
			this.api = api;
			this.crypto = crypto;
		}

		public JsonWebKey Generate()
		{
			return crypto.GenerateJWK();
		}

		/// <summary>
		/// Get the last transaction ID for the given wallet address.
		/// </summary>
		/// <param name="address">The arweave address to get the balance for.he arweave address to get the balance for.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<string> GetLastTransactionIDAsync(string address, CancellationToken cancellationToken = default)
		{
			return await api.GetStringResponse($"wallet/{address}/last_tx", cancellationToken);
		}

		/// <summary>
		/// Get the wallet balance for the given address.
		/// </summary>
		/// <param name="address">The arweave address to get the balance for.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<int> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
		{
			ApiResponse<int> result = await api.Get<int>($"wallet/{address}/balance", cancellationToken);

			return result.Data;
		}

		public async Task<int> GetBalanceAsync(JsonWebKey wallet, CancellationToken cancellationToken = default)
		{
			string address = GetAddress(wallet);

			ApiResponse<int> result = await api.Get<int>($"wallet/{address}/balance", cancellationToken);

			return result.Data;
		}

		public string GetAddress(JsonWebKey jwk)
		{
			return OwnerToAddress(jwk.N);
		}

		private string OwnerToAddress(string owner)
		{
			return Base64UrlEncoder.Encode(crypto.Hash(Base64UrlEncoder.DecodeBytes(owner)));
		}
	}
}
