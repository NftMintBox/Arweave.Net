using Arweave.NET.Lib;
using Arweave.NET.Lib.Crypto;
using Microsoft.IdentityModel.Tokens;

namespace Arweave.NET
{
	public class Arweave
	{
		Api api;
		public Wallets Wallets { get; private set; }

		Chunks chunks;
		public Transactions Transactions { get; private set; }
		public Network Network { get; private set; }
		public Blocks Blocks { get; private set; }

		public static ICryptoInterface Crypto = new CryptoProvider();

		public Silo Silo { get; private set; }

		private Arweave(ApiConfig apiConfig)
		{
			this.api = new Api(apiConfig);
			Wallets = new Wallets(this.api, Crypto);
			this.chunks = new Chunks(this.api);
			this.Transactions = new Transactions(this.api, Crypto, this.chunks);
			this.Silo = new Silo(this.api, Crypto, this.Transactions);

			Network = new Network(this.api);
			Blocks = new Blocks(this.api, this.Network);
			//this.ar = new Ar();
		}

		public static Arweave Init(ApiConfig apiConfig)
		{
			return new Arweave(apiConfig);
		}

		public Config GetConfig()
		{
			return new Config
			{
				Api = api.Config
			};
		}

		public async Task<Transaction> CreateTransactionAsync(TransactionBuilder attributes, JsonWebKey jwk, CancellationToken cancellationToken = default)
		{

			if (attributes.Data == null && !(attributes.Target == null && attributes.Quantity == 0))
			{
				throw new ArgumentNullException("A new Arweave transaction must have a 'data' value, or 'target' and 'quantity' values.");
			}

			if (attributes.Owner == null && jwk != null)
			{
				attributes.Owner = jwk.N;
			}

			if (attributes.LastTx == null)
			{
				attributes.LastTx = await Transactions.GetTransactionAnchor(cancellationToken);
			}

			if (!attributes.Reward.HasValue)
			{
				int length = 0;

				if (attributes.Data != null)
				{
					length = attributes.Data.Length;
				}

				attributes.Reward = await Transactions.GetPriceAsync(length, attributes.Target);

			}

			//transaction.DataRoot = null;
			//transaction.DataSize = 0;	

			//if (attributes.Data != null)
			//{
			//	transaction.DataSize = attributes.Data.Length;
			//	transaction.Data = attributes.Data;
			//}

			byte[] data = Array.Empty<byte>();

			if (attributes.Data != null)
			{
				data = attributes.Data;
			}

			Transaction transaction = new Transaction(attributes, data);

			//transaction.GetSignatureData(); -- replaced with prepare chunks inside
			return transaction;

		}

		//public string[] Arql(object query)
		//{
		//	throw new NotImplementedException();

		//	//public arql(query: object) : Promise<string[]> {
		//	//	return this.api
		//	//	  .post("/arql", query)
		//	//	  .then((response) => response.data || []);
		//	//}
		//}


	}
}
