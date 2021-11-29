using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	internal class BranchNode: MerkleNode
	{
		public int ByteRange { get; private set; }

		public MerkleNode? LeftChild { get; private set; }
		public MerkleNode? RightChild { get; private set; }


		public BranchNode(byte[] id, int byteRange, int maxByteRange, MerkleNode? leftChild, MerkleNode? rightChild)
		{
			ID = id;
			MaxByteRange = maxByteRange;
			ByteRange = byteRange;
			LeftChild = leftChild;
			RightChild = rightChild;

		}
	}
}
