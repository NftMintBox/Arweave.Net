using Arweave.NET.Lib.Crypto;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public interface ISignatureOptions
	{
		public int? saltLength { get; }
	}
	public interface ICryptoInterface
	{
		public JsonWebKey GenerateJWK();

		public byte[] Sign(JsonWebKey jwk, byte[] data);

		public bool Verify(string publicModulus, byte[] data, byte[] signature);

		//public byte[] Encrypt(byte[] data, string key, string? salt);
		//public byte[] Encrypt(byte[] data, byte[] key, string? salt);

		//public byte[] Decrypt(byte[] encrypted, string key, string? salt);
		//public byte[] Decrypt(byte[] encrypted, byte[] key, string? salt);

		public byte[] Hash(byte[] data, HashAlorithm hashAlorithm = HashAlorithm.SHA256);

		public Task<byte[]> CombineAndHashAsync(HashAlorithm hashAlorithm, params byte[][] arrays);
	}
}
