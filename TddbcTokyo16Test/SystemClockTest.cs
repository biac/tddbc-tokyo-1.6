using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using TddbcTokyo16;

namespace TddbcTokyo16Test {
	[TestFixture()]
	public class SystemClockTest {

		[Test()]
		public void NowTest01() {
			DateTime now = SystemClock.Now;
			Assert.That(Math.Abs(DateTime.Now.Subtract(now).TotalMilliseconds), Is.LessThan(1.0));
		}

#if DEBUG
		[Test()]
		public void NowTest02_テスト用のセッター() {
			DateTime time = new DateTime(2011, 8, 3, 18, 45, 0);
			SystemClock.TestSetTime(time);

			Assert.That(SystemClock.Now, Is.EqualTo(time));

			// テスト終了時にクリアすること!
			SystemClock.TestClearTime();
			Assert.That(SystemClock.Now, Is.Not.EqualTo(time));
		}
#endif

	}
}
