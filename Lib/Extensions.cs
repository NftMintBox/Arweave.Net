using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		//public static byte[] ToByteArray(this int note, int noteSize)
		//{
		//	byte[] buffer = new byte[noteSize];

		//	for (var i = buffer.Length - 1; i >= 0; i--)
		//	{
		//		var b = note % 256;
		//		buffer[i] = (byte)b;
		//		note = (note - b) / 256;
		//	}
		//	return buffer;
		//}

		public static byte[] ToBase64UrlEncodedByteArray(this string? val)
		{
			if (string.IsNullOrEmpty(val)) return Array.Empty<byte>();

			return Encoding.UTF8.GetBytes(Base64UrlEncoder.Encode(val));
		}
	}
}
