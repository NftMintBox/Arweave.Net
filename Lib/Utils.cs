using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public static class Utils
	{
		public static byte[] CombineArrays(params byte[][] arrays)
		{
			if (arrays.Length == 1) return arrays[0];

			byte[] rv = new byte[arrays.Sum(a => a.Length)];
			int offset = 0;
			foreach (byte[] array in arrays)
			{
				Buffer.BlockCopy(array, 0, rv, offset, array.Length);
				offset += array.Length;
			}
			return rv;
		}
	}
}
