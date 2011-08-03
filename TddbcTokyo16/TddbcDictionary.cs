﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TddbcTokyo16 {
	public class TddbcDictionary {

		private SortedSet<KeyValueTime> _dic = new SortedSet<KeyValueTime>();

		public void Put(string key, string value) {
			KeyValueTime existing = this._dic.FirstOrDefault(kvt => string.Equals(kvt.Key, key));
			if (existing != null) {
				existing.Value = value;
				return;
			}

			this._dic.Add(new KeyValueTime(key, value, null));
		}

		public void Put(string key, string value, DateTime time) {
			//throw new NotImplementedException();
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
			return this._dic.Where(kvt => (kvt.Time.Value >= time)).ToList();
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
			foreach (var kv in data) {
				this.Put(kv.Key, kv.Value);
			}
		}


	}
}
