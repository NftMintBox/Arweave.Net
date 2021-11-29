using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public static class DHasher
	{

		public static byte[] DeepHash(byte[] data)
		{
			var tag = Utils.CombineArrays(
				Encoding.UTF8.GetBytes("blob"),
				Encoding.UTF8.GetBytes(data.Length.ToString())
			);

			var taggedHash = Utils.CombineArrays(
			  Arweave.Crypto.Hash(tag, Crypto.HashAlorithm.SHA384),
			  Arweave.Crypto.Hash(data, Crypto.HashAlorithm.SHA384)
			);

			return Arweave.Crypto.Hash(taggedHash, Crypto.HashAlorithm.SHA384);
		}

		public static byte[] DeepHash(byte[][] data)
		{
			var tag = Utils.CombineArrays(
				  Encoding.UTF8.GetBytes("list"),
				  Encoding.UTF8.GetBytes(data.Length.ToString())
				);

			return DeepHashChunks(
				data,
				Arweave.Crypto.Hash(tag, Crypto.HashAlorithm.SHA384)
			);

		}

		public static byte[] DeepHash(List<byte[]> data)
		{
			var tag = Utils.CombineArrays(
				  Encoding.UTF8.GetBytes("list"),
				  Encoding.UTF8.GetBytes(data.Count.ToString())
				);


			return DeepHashChunks(
				data.ToArray(),
				Arweave.Crypto.Hash(tag, Crypto.HashAlorithm.SHA384)
			);

		}

		public static byte[] DeepHashChunks(byte[][] chunks, byte[] acc)
		{
			if (chunks.Length < 1)
			{
				return acc;
			}

			byte[] hashPair = Utils.CombineArrays(
				acc, 
				DeepHash(chunks[0])
			);


			byte[] newAcc = Arweave.Crypto.Hash(hashPair, Crypto.HashAlorithm.SHA384);

			return DeepHashChunks(chunks.Skip(1).ToArray(), newAcc);

		}
	}
}
