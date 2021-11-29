using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
	public class Api
	{
		public readonly string METHOD_GET = "GET";
		public readonly string METHOD_POST = "POST";

		public ApiConfig Config { get; private set; }

		public Api(ApiConfig apiConfig)
		{
			Config = apiConfig;
		}

		public void ApplyConfig(ApiConfig config)
		{
			Config = MergeDefaults(config);
		}

		private ApiConfig MergeDefaults(ApiConfig config) 
		{

			string protocol = config.Protocol ?? "http";
			string host = config.Host ?? "127.0.0.1";
			int port = config.Port ?? (protocol == "https" ? 443 : 80);

			return new ApiConfig
			{
				Protocol = protocol,
				Host = host,
				Port = port,
				Timeout = config.Timeout,
			};
		}

		internal async Task<string> GetStringResponse(string endPoint, CancellationToken cancellationToken = default)
		{

			string responseContent = "";

			using (HttpResponseMessage response = await Request().GetAsync(endPoint, cancellationToken))
			{
				try
				{
					response.EnsureSuccessStatusCode();
				}
				catch (Exception ex)
				{
					throw new ApiResponseException(ex.Message, response.StatusCode, null);
				}

				responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

			}

			return responseContent;
		}

		public async Task<ApiResponse<T>> Get<T>(string endPoint, CancellationToken cancellationToken = default)
		{
			using (HttpResponseMessage response = await Request().GetAsync(endPoint, cancellationToken))
			{
				try
				{
					response.EnsureSuccessStatusCode();
				}
				catch (Exception ex)
				{
					throw new ApiResponseException(ex.Message, response.StatusCode, null);
				}

				

				string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

				T? result;

				Type t = typeof(T);

				if (t.IsPrimitive || t == typeof(string) || t == typeof(decimal))
				{
					result = (T)Convert.ChangeType(responseContent, typeof(T)); ;
				}
				else
				{
					try
					{
						JsonSerializerOptions options = new JsonSerializerOptions
						{
							PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
							NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
						};

						result = JsonSerializer.Deserialize<T>(responseContent, options);
						
					}
					catch (Exception ex)
					{
						throw new ApiResponseException("Unable to deserialize object", ex, response.StatusCode, responseContent);
					}
				}

				if (result == null)
				{
					throw new ApiResponseException("Unable to deserialize object", response.StatusCode, responseContent);
				}

				return new ApiResponse<T>(result, response.StatusCode, response.ReasonPhrase);
			}
		}

		


		//public async Task<ApiResponse<T>> Post<T>(string endPoint, object? body, CancellationToken cancellationToken = default)
		//{
		//	if (body is string)
		//	{
		//		return await Post<T>(endPoint, body as string, cancellationToken);
		//	}
		//	string json = JsonSerializer.Serialize(body);

		//	using (StringContent content = new StringContent(json, Encoding.UTF8, "application/json"))
		//	{
		//		return await Post<T>(endPoint, content, cancellationToken);	
		//	}
		//}

		public async Task<ApiResponse<T>> Post<T>(string endPoint, byte[] body, CancellationToken cancellationToken = default)
		{
			using (ByteArrayContent content = new ByteArrayContent(body!))
			{
				return await Post<T>(endPoint, content, cancellationToken);
			}
		}


		/// <summary>
		/// Makes post request
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="endPoint"></param>
		/// <param name="body">Serialized Body</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<ApiResponse<T>> Post<T>(string endPoint, string? body, CancellationToken cancellationToken = default)
		{
			byte[] aBody = Array.Empty<byte>();

			if (!string.IsNullOrEmpty(body))
			{
 				aBody =	Encoding.UTF8.GetBytes(body);
			}

			return await Post<T>(endPoint, aBody, cancellationToken);
		}

		internal async Task<ApiResponse<T>>Post<T>(string endPoint, HttpContent content, CancellationToken cancellationToken = default)
		{
			HttpStatusCode statusCode;

			using (HttpResponseMessage response = await Request().PostAsync(endPoint, content, cancellationToken))
			{
				statusCode = response.StatusCode;

				try
				{
					response.EnsureSuccessStatusCode();
				}

				catch (Exception ex)
				{
					throw new ApiResponseException(ex.Message, statusCode, null);
				}

				T? result;
				
				string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

				try
				{
					Type t = typeof(T);

					if (t.IsPrimitive || t == typeof(string) || t == typeof(decimal))
					{
						result = (T)Convert.ChangeType(responseContent, typeof(T)); ;
					}
					else
					{
						JsonSerializerOptions options = new JsonSerializerOptions
						{
							PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
							NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
							DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
						};

						result = JsonSerializer.Deserialize<T>(responseContent, options);
					}
				}
				catch (Exception ex)
				{
					throw new ApiResponseException("Unable to deserialize object", ex, response.StatusCode, content);
				}

				if (result == null)
				{
					throw new ApiResponseException("Unable to deserialize object", response.StatusCode, responseContent);
				}

				return new ApiResponse<T>(result, response.StatusCode, response.ReasonPhrase);

				//using (Stream memoryStream = await response.Content.ReadAsStreamAsync(cancellationToken))
				//{
				//	result = await JsonSerializer.DeserializeAsync<T>(memoryStream, cancellationToken: cancellationToken);
				//}
			}

			//return new ApiResponse<T>(result, statusCode);
		}



		private HttpClient Request()
		{

			HttpClient httpClient = new HttpClient
			{
				BaseAddress = new Uri($"{Config.Protocol}://{Config.Host}:{Config.Port}"),
				Timeout = TimeSpan.FromMilliseconds((int)Config.Timeout),
				MaxResponseContentBufferSize = 1024 * 1024 * 512
			};

			return httpClient;
		}

	}
}
