using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib.Crypto
{
	
	public enum HashAlorithm { SHA256, SHA384 }
	public class CryptoProvider : ICryptoInterface
	{
		public readonly int keyLength = 4096;
		public readonly int publicExponent = 0x10001; //65537
													  //public readonly string hashAlgorithm = "sha256";
		public readonly string encryptionAlgorithm = "aes-256-cbc";

		public JsonWebKey GenerateJWK()
		{
			using (RSA rsa = RSA.Create(keyLength))
			{
				RSAParameters rsaKeyInfo = rsa.ExportParameters(true);

				byte[] pk = rsa.ExportRSAPublicKey();
				byte[] prk = rsa.ExportRSAPrivateKey();


				JsonWebKey jwk = new JsonWebKey
				{
					Kty = "RSA",
					D = Base64UrlEncoder.Encode(rsaKeyInfo.D),
					DP = Base64UrlEncoder.Encode(rsaKeyInfo.DP),
					DQ = Base64UrlEncoder.Encode(rsaKeyInfo.DQ),
					P = Base64UrlEncoder.Encode(rsaKeyInfo.P),
					Q = Base64UrlEncoder.Encode(rsaKeyInfo.Q),
					E = Base64UrlEncoder.Encode(rsaKeyInfo.Exponent),
					N = Base64UrlEncoder.Encode(rsaKeyInfo.Modulus),
					QI = Base64UrlEncoder.Encode(rsaKeyInfo.InverseQ)
				};

				return jwk;
			}
		}

		public byte[] Hash(byte[] data, HashAlorithm hashAlorithm = HashAlorithm.SHA256)
		{
			switch (hashAlorithm)
			{
				case HashAlorithm.SHA256:
					using (SHA256 sha256Hash = SHA256.Create())
					{
						return sha256Hash.ComputeHash(data);
					}
				case HashAlorithm.SHA384:
					using (SHA384 sha384Hash = SHA384.Create())
					{
						return sha384Hash.ComputeHash(data);
					}
				default:
					throw new NotSupportedException($"'{hashAlorithm}' is not supported");
			}
		}

		public async Task<byte[]> HashAsync(Stream stream, HashAlorithm hashAlorithm = HashAlorithm.SHA256)
		{
			switch (hashAlorithm)
			{
				case HashAlorithm.SHA256:
					using (SHA256 sha256Hash = SHA256.Create())
					{
						return await sha256Hash.ComputeHashAsync(stream);
					}
				case HashAlorithm.SHA384:
					using (SHA384 sha384Hash = SHA384.Create())
					{
						return await sha384Hash.ComputeHashAsync(stream);
					}
				default:
					throw new NotSupportedException($"'{hashAlorithm}' is not supported");
			}
		}

		public async Task<byte[]> CombineAndHashAsync(HashAlorithm alorithm = HashAlorithm.SHA256, params byte[][] arrays)
		{
			int totalSize = arrays.Sum(a => a.Length);

			using (MemoryStream ms = new MemoryStream(totalSize))
			{
				foreach (var item in arrays)
				{
					await ms.WriteAsync(item);
				}

				await ms.FlushAsync();

				ms.Position = 0;

				return await HashAsync(ms, alorithm);
			}
		}

		public byte[] Sign(JsonWebKey jwk, byte[] data)
		{
			RSAParameters ppp = new RSAParameters
			{
				D = Base64UrlEncoder.DecodeBytes(jwk.D),
				DP = Base64UrlEncoder.DecodeBytes(jwk.DP),
				DQ = Base64UrlEncoder.DecodeBytes(jwk.DQ),
				P = Base64UrlEncoder.DecodeBytes(jwk.P),
				Q = Base64UrlEncoder.DecodeBytes(jwk.Q),
				Exponent = Base64UrlEncoder.DecodeBytes(jwk.E),
				Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
				InverseQ = Base64UrlEncoder.DecodeBytes(jwk.QI)
			};

			byte[] result;

			using (RSA rsa = RSA.Create())
			{
				rsa.ImportParameters(ppp);
				rsa.KeySize = keyLength;

				result = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
			}

			return result;			
		}


		public bool Verify(string publicModulus, byte[] data, byte[] signature)
		{
			JsonWebKey publicKey = new JsonWebKey{
				Kty		= "RSA",
				E		= "AQAB",
				N		= publicModulus
			};

			var key = new RsaSecurityKey(new RSAParameters
			{
				Modulus = Base64UrlEncoder.DecodeBytes(publicKey.N),
				Exponent = Base64UrlEncoder.DecodeBytes(publicKey.E)
			});

			bool isValid = key.Rsa.VerifyData(
				data, 
				signature,
				HashAlgorithmName.SHA256,
				RSASignaturePadding.Pss
			);

			return isValid;

		}


	}
}
