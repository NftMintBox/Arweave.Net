using Arweave.NET.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public class Silo
	{
		private Api api;

		private ICryptoInterface crypto;

		private Transactions transactions;

		public Silo(Api api, ICryptoInterface crypto, Transactions transactions)
		{
			this.api = api;
			this.crypto = crypto;
			this.transactions = transactions;
		}
	}
}
