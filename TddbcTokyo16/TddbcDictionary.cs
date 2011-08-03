using System;
using System.Collections.Generic;
using System.Linq;

namespace TddbcTokyo16 {
	public class TddbcDictionary {

		private SortedSet<KeyValueTime> _dic = new SortedSet<KeyValueTime>();

		public void Put(string key, string value) {
			//KeyValueTime existing = this._dic.FirstOrDefault(kvt => string.Equals(kvt.Key, key));
			//if (existing != null) {
			//    existing.Value = value;
			//    existing.Time = SystemClock.Now;
			//    return;
			//}

			//this._dic.Add(new KeyValueTime(key, value, null));
			this.Put(key, value, SystemClock.Now);
		}

		private void Put(string key, string value, DateTime time) {
			KeyValueTime existing = this._dic.FirstOrDefault(kvt => string.Equals(kvt.Key, key));
			if (existing != null) {
				this._dic.Remove(existing);	//一旦削除して、改めて挿入しなおせば、正しくソートされる。

				existing.Value = value;
				existing.Time = time;

				this._dic.Add(existing);
				return;
			}
	
			this._dic.Add(new KeyValueTime(key, value, time));
		}


		public string Get(string key) {
			if (key == null)
				throw new ArgumentNullException();

			return this._dic.First(kvt => kvt.Key == key).Value;
		}


		public IList<KeyValueTime> Dump() {
			return this._dic.ToList();
		}

		public IList<KeyValueTime> Dump(DateTime time) {
			//return this._dic.Where(kvt => kvt.Time.HasValue ? (kvt.Time.Value >= time) : false).ToList();
			//return this._dic.Where(kvt => (kvt.Time.Value >= time)).ToList();
			return this._dic.Where(kvt => (kvt.Time >= time)).ToList();
		}


		public void Delete(string key) {
			if (key == null)
				throw new ArgumentNullException();

			this._dic.RemoveWhere(kvt => kvt.Key == key);
		}

		internal void Delete(int passedMinute, int passedSecond) {
			DateTime limitTime = SystemClock.Now.AddMinutes(-passedMinute).AddSeconds(-passedSecond);
			this._dic.RemoveWhere(kvt => (kvt.Time < limitTime));
		}


		public void MultiPut(IList<KeyValuePair<string, string>> data) {
			foreach (var kv in data) {
				if (kv.Key == null)
					throw new ArgumentNullException();
			}

			DateTime time = SystemClock.Now;
			foreach (var kv in data) {
				this.Put(kv.Key, kv.Value, time);
				time = time.AddMilliseconds(1.0);	//TODO: 次の Put()/MultiPut() の時刻が近いと、これでは前後関係がおかしくなる。
			}
		}


	}
}
