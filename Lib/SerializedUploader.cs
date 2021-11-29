using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public class SerializedUploader
	{
		public int? chunkIndex { get;set; }
		public bool txPosted { get;set; }

		public TransactionBuilder? transaction { get;set; }

		public int lastRequestTimeEnd { get; set; }

		public int lastResponseStatus { get; set; }

		public string lastResponseError { get; set; } = "";

	}
}
