using System.Text;

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
			  ArweaveFactory.Crypto.Hash(tag, Crypto.HashAlorithm.SHA384),
			  ArweaveFactory.Crypto.Hash(data, Crypto.HashAlorithm.SHA384)
			);

			return ArweaveFactory.Crypto.Hash(taggedHash, Crypto.HashAlorithm.SHA384);
		}

		public static byte[] DeepHash(byte[][] data)
		{
			var tag = Utils.CombineArrays(
				  Encoding.UTF8.GetBytes("list"),
				  Encoding.UTF8.GetBytes(data.Length.ToString())
				);

			return DeepHashChunks(
				data,
				ArweaveFactory.Crypto.Hash(tag, Crypto.HashAlorithm.SHA384)
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
				ArweaveFactory.Crypto.Hash(tag, Crypto.HashAlorithm.SHA384)
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


			byte[] newAcc = ArweaveFactory.Crypto.Hash(hashPair, Crypto.HashAlorithm.SHA384);

			return DeepHashChunks(chunks.Skip(1).ToArray(), newAcc);

		}
	}
}
