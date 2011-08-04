using System;
using System.Collections.Generic;
using System.Linq;

namespace TddbcTokyo16 {

	public class TddbcKeyValueStore {

		private SortedSet<KeyValueTime> _dic = new SortedSet<KeyValueTime>();


		public void Put(string key, string value) {
			this.Put(key, value, SystemClock.Now);
		}

		private void Put(string key, string value, DateTime time) {
			this._dic.RemoveWhere(kvt => (kvt.Key == key));
			this._dic.Add(new KeyValueTime(key, value, time));
		}

		public void MultiPut(IList<KeyValuePair<string, string>> data) {
			if(data.Count(kv => (kv.Key == null)) > 0)
				throw new ArgumentNullException();

			DateTime time = SystemClock.Now;
			foreach (var kv in data) {
				this.Put(kv.Key, kv.Value, time);
				time = time.AddMilliseconds(1.0);
				//TODO: 次の Put()/MultiPut() の時刻が近いと、これでは前後関係がおかしくなる。
			}
		}


		public string Get(string key) {
			if (key == null)
				throw new ArgumentNullException();

			KeyValueTime data = this._dic.FirstOrDefault(kvt => (kvt.Key == key));
			if (data == null)
				throw new KeyNotFoundException();

			return data.Value;
		}


		public IList<KeyValueTime> Dump() {
			return this._dic.Reverse().ToList();
		}

		public IList<KeyValueTime> Dump(DateTime time) {
			return this._dic.Reverse().Where(kvt => (kvt.Time >= time)).ToList();
		}


		public void Delete(string key) {
			if (key == null)
				throw new ArgumentNullException();

			this._dic.RemoveWhere(kvt => (kvt.Key == key));
		}

		public void Delete(int passedMinutes, int passedSeconds) {
			DateTime limitTime = SystemClock.Now.AddMinutes(-passedMinutes).AddSeconds(-passedSeconds);
			this._dic.RemoveWhere(kvt => (kvt.Time < limitTime));
		}
	}
}