using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TddbcTokyo16 {

	public class KeyValueTime : IComparable {

		public KeyValueTime(string key, string value, DateTime? time) {
			if (key == null)
				throw new ArgumentNullException();

			this.Key = key;
			this.Value = value;
			this.Time = time;
		}

		public string Key { get; set; }

		public string Value { get; set; }

		public DateTime? Time { get; set; }



		// IComparable

		public int CompareTo(object obj) {
			KeyValueTime kvt = obj as KeyValueTime;

			if(kvt.Time != this.Time)
				return DateTime.Compare(kvt.Time.Value, this.Time.Value);

			if(kvt.Value != this.Value)
				return string.Compare(this.Value, kvt.Value);

			return string.Compare(this.Key, kvt.Key);
		}

	}
}
