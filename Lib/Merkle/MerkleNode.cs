using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	internal abstract class MerkleNode
	{
		public byte[]? ID { get; protected set; }
		public int MaxByteRange { get; protected set; }

	}
}
