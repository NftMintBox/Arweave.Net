using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arweave.NET.Lib
{
    /// <summary>
    /// String - Base64UrlString - String
    /// </summary>
	internal class JsonBase64UrlToStringConverter : JsonConverter<string>
	{
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(string) == typeToConvert;
        }

        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
            string? value = reader.GetString();

            if (value == null) return null;

            return Base64UrlEncoder.Decode(value);
		}

		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{
            writer.WriteStringValue(Base64UrlEncoder.Encode(value));
        }
	}

    /// <summary>
    /// byte[] - Base64UrlString - byte[]
    /// </summary>
    internal class JsonBase64UrlToByteConverter : JsonConverter<byte[]>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(byte[]) == typeToConvert;
        }

        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();

            if (value == null) return null;

            return Base64UrlEncoder.DecodeBytes(value);
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Base64UrlEncoder.Encode(value));
        }
    }


}
