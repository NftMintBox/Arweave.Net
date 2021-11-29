using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public enum ArweaveErrorType
	{
		TX_NOT_FOUND,
		TX_FAILED,
		TX_INVALID,
		BLOCK_NOT_FOUND
	}

	public class ArweaveError: Exception
	{
		public ArweaveErrorType ErrorType { get; private set; }
		public ArweaveError(ArweaveErrorType errorType)
		{
			ErrorType = errorType;
		}
	}
}
