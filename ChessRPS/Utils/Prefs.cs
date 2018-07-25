using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Client.Utils
{
	static class Prefs
	{
		private static readonly string filePath = "";
		private static readonly JObject json = ReadJson();
		private static void WriteJson() => File.WriteAllText(filePath, json.ToString());

		public static IPAddress ServerIP => IPAddress.Parse("84.109.106.163");
		public static int Token { get; set; }
		public static string Name
		{
			get => (string)json["name"];
			set
			{
				json["name"] = value;
				WriteJson();
			}
		}

		private static JObject ReadJson()
		{
			if (File.Exists(filePath))
			{
				File.WriteAllText(filePath, "{}");
				return new JObject();
			}
			string data = File.ReadAllText(filePath);
			return JObject.Parse(data);
		}
	}
}
