using Arweave.NET.Lib;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arweave.NET
{
	public class Tag
	{

		[JsonPropertyName("name")]
		[JsonConverter(typeof(JsonBase64UrlToStringConverter))]
		public string Name { get; private set; }

		[JsonPropertyName("value")]
		[JsonConverter(typeof(JsonBase64UrlToStringConverter))]
		public string Value { get; private set; }


		//[JsonPropertyName("name")]
		//public string _name
		//{
		//	get => Base64UrlEncoder.Encode(Name); 
		//	//set =>Name = Base64UrlEncoder.Decode(value);
		//}

		//[JsonPropertyName("value")]
		//public string _value
		//{
		//	get => Base64UrlEncoder.Encode(Value);
		//	set => Value = Base64UrlEncoder.Decode(value);
		//}


		public Tag(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}
}
