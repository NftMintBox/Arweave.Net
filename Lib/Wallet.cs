using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public interface IJWKPublicInterface
	{
		public string kty { get; }
		public string e { get; }
		public string n { get; }
}

	public interface IJWKInterface: IJWKPublicInterface
	{
		public string? d { get; }
		public string? p { get; }
		public string? q { get; }
		public string? dp { get; }
		public string? dq { get; }
		public string? qi { get; }

	}

}
