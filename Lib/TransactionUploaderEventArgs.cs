using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public class TransactionUploaderEventArgs : AsyncCompletedEventArgs
	{
		public TransactionUploaderEventArgs(Exception error, bool cancelled, object userState) : base(error, cancelled, userState)
		{
		}

		public TransactionUploaderEventArgs() : base(null, false, null) { }


		public TimeSpan ExecutionTime { get; internal set; }

		public double CurrentProgress { get; internal set; }

		public string? ErrorMessage { get; internal set; }
		public string? TransactionID { get; internal set; }

		public int ChunksUploaded {get; internal set; }
		public int TotalChunks {get; internal set; }
	}
}
