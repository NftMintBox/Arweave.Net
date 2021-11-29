using Arweave.NET.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public class Config
	{
		public ApiConfig Api { get; set; }
		public ICryptoInterface Crypto { get; set; }
	}
}
