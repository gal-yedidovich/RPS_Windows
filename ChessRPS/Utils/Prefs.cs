using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Client.Utils
{
	class Prefs
	{
		public static class KEYS
		{
			public static string
				token = "token",
				playerList = "player_list";
		}


		public static Prefs Instance = new Prefs();

		private Prefs() { }

		Dictionary<string, object> cache = new Dictionary<string, object>();

		public int Token
		{
			get => (int)cache[KEYS.token];
			private set => cache[KEYS.token] = value;
		}

		public object this[string key]
		{
			get => cache[key];
			set => cache[key] = value;
		}

		public T Opt<T>(string key) => (T)cache[key];
	}
}
