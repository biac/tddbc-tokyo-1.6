using System;

namespace TddbcTokyo16 {
	internal class SystemClock {
		public static DateTime Now {
			get {
#if DEBUG		
				if (_testTime.HasValue)
					return _testTime.Value;
#endif
				return DateTime.Now;
			}
		}

	
#if DEBUG		
		static DateTime? _testTime;

		internal static void TestSetTime(DateTime time) {
			_testTime = time;
		}

		internal static void TestClearTime() {
			_testTime = null;
		}
#endif
	}
}
