using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public class ApiConfig
	{
		public string? Host { get; set; }
		public int? Port { get; set; }

		public int Timeout { get; set; } = 20000;
		public string? Protocol { get; set; }

		public static ApiConfig Public
		{
			get
			{
				return new ApiConfig
				{
					Host = "arweave.net",
					Port = 443,
					Protocol = "https",
					Timeout = 20000,
				};
			}
		}
	}
}
