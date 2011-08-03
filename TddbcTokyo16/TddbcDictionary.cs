using System;
using System.Collections.Generic;
using System.Linq;

namespace TddbcTokyo16 {
	public class TddbcDictionary {

		//private Dictionary<string, string> _dic = new Dictionary<string, string>();
		private SortedSet<KeyValueTime> _dic = new SortedSet<KeyValueTime>();

		public void Put(string key, string value) {
			//this._dic[key] = value;

			KeyValueTime existing = this._dic.FirstOrDefault(kvt => string.Equals(kvt.Key, key));
			if (existing != null) {
				existing.Value = value;
				return;
			}

			this._dic.Add(new KeyValueTime(key, value, null));
		}

		public string Get(string key) {
			//return this._dic[key];

			if (key == null)
				throw new ArgumentNullException();

			return this._dic.First(kvt => kvt.Key == key).Value;
		}

		public IList<KeyValueTime> Dump() {
			//return this._dic.ToList<KeyValuePair<string, string>>();
			return this._dic.ToList();
		}

		public void Delete(string key) {
			//this._dic.Remove(key);

			if (key == null)
				throw new ArgumentNullException();

			this._dic.RemoveWhere(kvt => kvt.Key == key);
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
