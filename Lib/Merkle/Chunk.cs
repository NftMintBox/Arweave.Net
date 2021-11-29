using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	internal struct Chunk: ICloneable
	{
		public byte[] DataHash { get; private set; }
		public int MinByteRange { get; private set; }	

		public int MaxByteRange { get;  private set; }

		public Chunk(byte[] dataHash, int minByteRange, int maxBytRange)
		{
			DataHash = dataHash;
			MinByteRange = minByteRange;
			MaxByteRange = maxBytRange;
		}

		public object Clone()
		{
			Chunk clone = new Chunk();

			clone.DataHash = (byte[])DataHash.Clone();
			clone.MinByteRange = MinByteRange;
			clone.MaxByteRange = MaxByteRange;

			return clone;
		}
	}

	internal class Chunks: ICloneable
	{
		public static Chunks Empty => new Chunks(); 

		public byte[] data_root { get; set; }
		public List<Chunk> chunks { get; set; }
		public List<ProofHash> proofs { get; set; }

		private Chunks()
		{
			data_root = Array.Empty<byte>();
			chunks = new List<Chunk>();
			proofs = new List<ProofHash>();
		}

		public Chunks(byte[] dataRoot, List<Chunk> chunks, List<ProofHash> proofs)
		{
			data_root = dataRoot;
			this.chunks = chunks;
			this.proofs = proofs;
		}

		public object Clone()
		{
			Chunks clone = new Chunks();

			clone.data_root = (byte[])data_root.Clone();

			foreach (var item in chunks)
			{
				clone.chunks.Add((Chunk)item.Clone());
			}

			foreach (var item in proofs)
			{
				clone.proofs.Add((ProofHash)item.Clone());
			}

			return clone;
		}
	}
}
