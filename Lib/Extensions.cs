using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Arweave.NET.Lib
{
	internal static class Extensions
	{
		#region DateTime
		public static double ToEpoch(this DateTime dt)
		{
			TimeSpan t = (dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0));
			return t.TotalSeconds;
		}

		public static double ToEpochMilliseconds(this DateTime dt)
		{
			TimeSpan t = (dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0));
			return t.TotalMilliseconds;
		}

		public static DateTime FromEpoch(this double EpochTime)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(EpochTime);
		}

		public static DateTime FromEpoch(this long EpochTime)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(EpochTime);
		}

		#endregion

		public static byte[] ToBase64UrlEncodedByteArray(this string? val)
		{
			if (string.IsNullOrEmpty(val)) return Array.Empty<byte>();

			return Encoding.UTF8.GetBytes(Base64UrlEncoder.Encode(val));
		}

		#region JsonWebKey

		internal static RSAParameters ToRSAParameters(this JsonWebKey jwk)
		{
			RSAParameters rsaParameters = new RSAParameters
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

			return rsaParameters;
		}



		#endregion
	}
}
