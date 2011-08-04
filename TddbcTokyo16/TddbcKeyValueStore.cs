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
			//KeyValueTime existing = this._dic.FirstOrDefault(kvt => string.Equals(kvt.Key, key));
			//if (existing != null) {
			//    this._dic.Remove(existing);	//一旦削除して、改めて挿入しなおせば、正しくソートされる。

			//    existing.Value = value;
			//    existing.Time = time;

			//    this._dic.Add(existing);
			//    return;
			//}
			this._dic.RemoveWhere(kvt => (kvt.Key == key));
	
			this._dic.Add(new KeyValueTime(key, value, time));
		}

		public void MultiPut(IList<KeyValuePair<string, string>> data) {
			//foreach (var kv in data) {
			//    if (kv.Key == null)
			//        throw new ArgumentNullException();
			//}
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

			//return this._dic.First(kvt => kvt.Key == key).Value;	//TODO: 一致する key が無いと例外(?)
			KeyValueTime data = this._dic.FirstOrDefault(kvt => (kvt.Key == key));
			if (data == null)
				throw new KeyNotFoundException();

			return data.Value;
		}


		public IList<KeyValueTime> Dump() {
			return this._dic.ToList();
		}

		public IList<KeyValueTime> Dump(DateTime time) {
			return this._dic.Where(kvt => (kvt.Time >= time)).ToList();
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
