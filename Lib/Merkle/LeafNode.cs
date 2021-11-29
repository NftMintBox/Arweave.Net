using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	internal class LeafNode: MerkleNode
	{
	
		public byte[] DataHash { get; private set; }

		public int MinByteRange { get; private set; }

		public LeafNode(byte[] id, byte[] dataHash, int minByteRange, int maxByteRange)
		{
			ID = id;
			MaxByteRange = maxByteRange;
			DataHash = dataHash;
			MinByteRange = minByteRange;
		}

	}
}
