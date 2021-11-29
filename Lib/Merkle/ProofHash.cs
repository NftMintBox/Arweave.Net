using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{

	internal class ProofHash: ICloneable
	{
		public int Offset { get; private set; }	
		public byte[] Proof { get; private set; }

	
		public ProofHash(int offset, byte[] proof)
		{
			Offset = offset;
			Proof = proof;
		}

		public object Clone()
		{
			ProofHash clone = new ProofHash(Offset, (byte[])Proof.Clone());

			return clone;
		}
	}
}
