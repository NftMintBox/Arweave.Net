using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	internal class PathValidationResult
	{
		public bool IsValid { get; private set; }

		public long Offset { get; private set; }

		public long LeftBound {get; private set; }

		public long RightBound { get; private set; }

		public long ChunkSize { get; private set; }

		private PathValidationResult(bool isValid)
		{
			IsValid = isValid;
		}

		public static PathValidationResult Invalid => new PathValidationResult(false);

		public PathValidationResult(long offset, long leftBound, long rightBound, long chunkSize)
		{
			Offset = offset;
			LeftBound = leftBound;
			RightBound = rightBound;
			ChunkSize = chunkSize;
			IsValid = true;
		}

	}
}
