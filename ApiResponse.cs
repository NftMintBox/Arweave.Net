using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public class ApiResponse<T>
	{
		public T Data { get; internal set; }

		public HttpStatusCode Status { get; private set; }

		public string? StatusText { get; private set; }
		
		public ApiResponse(T data, HttpStatusCode statusCode, string? statusText = null)
		{
			Data = data;
			Status = statusCode;	
			StatusText = statusText;
		}

		public bool IsOK { get => Status == HttpStatusCode.OK; }
	}

	public class ApiResponseException: Exception
	{
		public HttpStatusCode Status { get; private set; }
		public object? RawData { get; internal set; }

		internal ApiResponseException(string message, HttpStatusCode statusCode, object? data) : base(message)
		{
			RawData = data;
			Status = statusCode;
		}

		internal ApiResponseException(string message, Exception inner, HttpStatusCode statusCode, object? data) : base(message, inner)
		{
			RawData = data;
			Status = statusCode;
		}

	}
}
