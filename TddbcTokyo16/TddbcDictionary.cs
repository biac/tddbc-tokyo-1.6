using System;
using System.Collections.Generic;
using System.Linq;

namespace TddbcTokyo16 {
	public class TddbcDictionary {

		private Dictionary<string, string> _dic = new Dictionary<string, string>();

		public void Put(string key, string value) {
			this._dic[key] = value;
		}

		public string Get(string key) {
			return this._dic[key];
		}

		public IList<KeyValuePair<string, string>> Dump() {
			return this._dic.ToList<KeyValuePair<string, string>>();
		}

		public void Delete(string key) {
			this._dic.Remove(key);
		}

		public void MultiPut(IList<KeyValuePair<string, string>> data) {
			foreach (var kv in data) {
				if (kv.Key == null)
					throw new ArgumentNullException();
			}
			foreach (var kv in data) {
				this.Put(kv.Key, kv.Value);
			}
		}
	}
}
