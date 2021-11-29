using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	internal static class Merkle
	{
		const int MAX_CHUNK_SIZE = 256 * 1024;
		const int MIN_CHUNK_SIZE = 32 * 1024;

		const int NOTE_SIZE = 32;
		const int HASH_SIZE = 32;

		/// <summary>
		/// Generates the data_root, chunks & proofs needed for a transaction.
		/// This also checks if the last chunk is a zero-length chunk and discards that chunk and proof if so. 
		/// (we do not need to upload this zero length chunk)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Chunks GenerateTransactionChunks(byte[] data)
		{

			List<Chunk> chunks = ChunkData(data);
			List<MerkleNode> leaves = GenerateLeaves(chunks);

			MerkleNode root = BuildLayers(leaves);

			List<ProofHash> proofs = GenerateProofs(root);

			// Discard the last chunk & proof if it's zero length.
			Chunk lastChunk = chunks.Last();


			if (lastChunk.MaxByteRange - lastChunk.MinByteRange == 0) {

				chunks.RemoveAt(chunks.Count - 1);
				proofs.RemoveAt(proofs.Count - 1);
			}

			return new Chunks(root.ID, chunks, proofs);
		}

		private static List<ProofHash> GenerateProofs(MerkleNode root)
		{
			List<ProofHash> proofs = ResolveBranchProofs(root, Array.Empty<byte>());

			return proofs;
		}

		private static List<ProofHash> ResolveBranchProofs(MerkleNode node, byte[] proof, int depth = 0)
		{
			List<ProofHash> resut = new List<ProofHash>();

			if (node is LeafNode)
			{
				resut.Add(new ProofHash(
					offset : node.MaxByteRange - 1,
					proof : Utils.CombineArrays(proof, ((LeafNode)node).DataHash, IntToBuffer(node.MaxByteRange))
				));

				return resut;
			}

			if (node is BranchNode)
			{
				BranchNode branch = (BranchNode)node;

				byte[] partialProof = Utils.CombineArrays(
					proof,
					branch.LeftChild!.ID!,
					branch.RightChild!.ID!,
					IntToBuffer(((BranchNode)node).ByteRange)
				);

				resut.AddRange(ResolveBranchProofs(branch.LeftChild!, partialProof, depth + 1));
				resut.AddRange(ResolveBranchProofs(branch.RightChild!, partialProof, depth + 1));

				return resut;
			}

			throw new Exception("Unexpected node type");
		}


		private static List<MerkleNode> GenerateLeaves(List<Chunk> chunks)
		{
			//make parallel

			List<MerkleNode> result = new List<MerkleNode>();

			foreach (var chunk in chunks)
			{
				byte[] hash = Hash(
					Hash(chunk.DataHash),
					Hash(IntToBuffer(chunk.MaxByteRange))
				);

				result.Add(new LeafNode(
					hash,
					chunk.DataHash,
					chunk.MinByteRange,
					chunk.MaxByteRange)
				);

			}

			return result;
		}


		private static byte[] Hash(params byte[][] arrays)
		{
			byte[] data = Utils.CombineArrays(arrays);	

			return Arweave.Crypto.Hash(data);
			//return (byte[])h.Clone();
		}
		private static List<Chunk> ChunkData(byte[] data)
		{
			List<Chunk> chunks = new List<Chunk>();

			byte[] rest = data;
			int cursor = 0;

			while (rest.Length >= MAX_CHUNK_SIZE)
			{
				int chunkSize = MAX_CHUNK_SIZE;

				// If the total bytes left will produce a chunk < MIN_CHUNK_SIZE,
				// then adjust the amount we put in this 2nd last chunk.

				int nextChunkSize = rest.Length - MAX_CHUNK_SIZE;
				if (nextChunkSize > 0 && nextChunkSize < MIN_CHUNK_SIZE)
				{
					chunkSize = (int)Math.Ceiling(rest.Length / 2.0);
				}

				byte[] chunk = rest.Take(new Range(0, chunkSize)).ToArray();
				byte[] dataHash = Arweave.Crypto.Hash(chunk);
				
				cursor += chunk.Length;

				chunks.Add(new Chunk (
					dataHash,
					cursor - chunk.Length,
					cursor
				));

				rest = rest.Skip(chunkSize).ToArray();
			}

			chunks.Add(
				new Chunk (
					Arweave.Crypto.Hash(rest),
					cursor,
					cursor + rest.Length
				)
			);

			return chunks;
		}

		private static MerkleNode BuildLayers(List<MerkleNode> nodes, int level = 0)
		{
			// If there are only 2 nodes left, this is going to be the root node
			if (nodes.Count < 2)
			{
				MerkleNode root = HashBranch(nodes[0], nodes.ElementAtOrDefault(1));
				
				return root;
			}

			List<MerkleNode> nextLayer = new List<MerkleNode>();

			for (int i = 0; i < nodes.Count; i += 2)
			{
				nextLayer.Add(HashBranch(nodes[i], nodes.ElementAtOrDefault(i + 1)));
			}


			return BuildLayers(nextLayer, level + 1);
		}

		public static MerkleNode HashBranch(MerkleNode left, MerkleNode? right)
		{
			if (right == null)
			{
				return left;
			}

			byte[] id = Hash(
				Hash(left.ID), 
				Hash(right.ID), 
				Hash(IntToBuffer(left.MaxByteRange))
			);

			BranchNode branchNode = new BranchNode(
				id: id, 
				byteRange: left.MaxByteRange, 
				maxByteRange: right.MaxByteRange,
				leftChild: left,
				rightChild: right
			);
			
			return branchNode;
		}

		public static async Task<PathValidationResult> ValidatePath(byte[] id, int dest, int leftBound, int rightBound, byte[] path)
		{
			if (rightBound <= 0)
			{
				return PathValidationResult.Invalid;
			}

			if (dest >= rightBound)
			{
				return await ValidatePath(id, 0, rightBound - 1, rightBound, path);
			}

			if (dest < 0)
			{
				return await ValidatePath(id, 0, 0, rightBound, path);
			}

			if (path.Length == HASH_SIZE + NOTE_SIZE)
			{
				var pathData = path.Take(HASH_SIZE);

				var endOffsetBuffer = path.Take(new Range(pathData.Count(), pathData.Count() + NOTE_SIZE));

				byte[] pathDataHash = Hash(
				  Hash(pathData.ToArray()),
				  Hash(endOffsetBuffer.ToArray())
				);

				if (Enumerable.SequenceEqual(id, pathDataHash)) 
				{
					return new PathValidationResult(
						offset: rightBound - 1,
						leftBound: leftBound,
						rightBound: rightBound,
						chunkSize: rightBound - leftBound
					);
				}

				return PathValidationResult.Invalid;
			}

			byte[] left = path.Take(HASH_SIZE).ToArray();

			int lcount = left.Length;

			byte[] right = path.Take(new Range(lcount, lcount + HASH_SIZE)).ToArray();

			int rcount = right.Count();

			var offsetBuffer = path.Take(new Range(
			  lcount + rcount,
			  lcount + rcount + NOTE_SIZE
			)).ToArray();

			int offset = ByteArrayToInt(offsetBuffer);

			byte[] remainder = path.Skip(
			  lcount + rcount + offsetBuffer.Length
			).ToArray();

			byte[] pathHash = Hash(
			  Hash(left),
			  Hash(right),
			  Hash(offsetBuffer)
			);

			if (Enumerable.SequenceEqual(id, pathHash))
			{
				if (dest < offset)
				{
					return await ValidatePath(
					  left,
					  dest,
					  leftBound,
					  Math.Min(rightBound, offset),
					  remainder
					);
				}
				return await ValidatePath(
				  right,
				  dest,
				  Math.Max(leftBound, offset),
				  rightBound,
				  remainder
				);
			}

			return PathValidationResult.Invalid;

		}

		private static byte[] Hash(params byte[] data) 
		{
			byte[] concat = Utils.CombineArrays(data);

			return Arweave.Crypto.Hash(concat);
		}

		private static byte[] IntToBuffer(int note)
		{
			byte[] buffer = new byte[NOTE_SIZE];

			for (var i = buffer.Length - 1; i >= 0; i--)
			{
				var b = note % 256;
				buffer[i] = (byte)b;
				note = (note - b) / 256;
			}
			return buffer;
		}

		private static int ByteArrayToInt(byte[] byteArray)
		{
			int value = 0;
			for (var i = 0; i < byteArray.Length; i++)
			{
				value *= 256;
				value += byteArray[i];
			}
			return value;
		}
	}
}
