﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using TddbcTokyo16;

namespace TddbcTokyo16Test {
	[TestFixture()]
	public class KeyValueTimeTest {

		// T16MAIN-5 の仕様変更(下記)で新しく必要となる Value Object。
		// key, value, time をセットで保持できる入れ物。KeyValuPair を拡張したカタチ。
		// とりあえず必要なもの
		//	プロパティ: Key, Value, Time
		//	コンストラクター:  KeyValuTime(key, value, time)
		/*
		 ■ T16MAIN-5: putの引数でkey,value,date(時刻情報)を渡し、dumpを時間順に出力するように仕様変更
			putの引数でkey,value,date(時刻情報)を渡せるようにする。
			また、dump関数は時刻が新しい方から古い方へ順にkey、valueを出力するように変更する。
			引数に複数指定して追加する関数の場合、後ろにあるものほど新しいとみなす。
		 */

		[Test()]
		public void Test01_コンストラクトして3つのプロパティを読み出し() { 
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt = new KeyValueTime("AAA", "Value1", dt);

			string key = kvt.Key;
			Assert.That(key, Is.EqualTo("AAA"));
			string val = kvt.Value;
			Assert.That(val, Is.EqualTo("Value1"));
			DateTime time = kvt.Time;
			Assert.That(time, Is.EqualTo(dt));
		}

		//[Test()]
		//public void Test02_Timeにはnullもセット可能() {
		//    KeyValueTime kvt = new KeyValueTime("AAA", "Value1", null);

		//    Assert.That(kvt.Time, Is.Null);
		//}
		// ※ T16MAIN-8 の仕様変更で、null はセット出来なくなった。



		// IComparable の実装: SortedSet に突っ込みたいので。
		// ※ 型が合わないときは ArgumentException

		[Test()]
		public void CompareToTest01_Key_Valueともnullではなく等しい() {
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt1 = new KeyValueTime("AAA", "Value1", dt);
			KeyValueTime kvt2 = new KeyValueTime("AAA", "Value1", dt);

			Assert.That(kvt1.CompareTo(kvt2), Is.EqualTo(0));
		}

		[Test()]
		public void CompareToTest02_Key_Valueともnullではない_Timeだけ違う() {
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt1 = new KeyValueTime("AAA", "Value1", dt);
			KeyValueTime kvt2 = new KeyValueTime("AAA", "Value1", dt.AddMilliseconds(-1.0)); //古い → 前に来る

			Assert.That(kvt1.CompareTo(kvt2), Is.GreaterThan(0));
			Assert.That(kvt2.CompareTo(kvt1), Is.LessThan(0));
		}

		[Test()]
		public void CompareToTest03_Key_Valueともnullではない_ValueとTimeが違う_Timeが優先() {
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt1 = new KeyValueTime("AAA", "Value1", dt);	//Valueの比較だけならこっちが前だけど…
			KeyValueTime kvt2 = new KeyValueTime("AAA", "Value2", dt.AddSeconds(-1.0)); //古い → 前に来る

			Assert.That(kvt1.CompareTo(kvt2), Is.GreaterThan(0));
			Assert.That(kvt2.CompareTo(kvt1), Is.LessThan(0));
		}

		[Test()]
		public void CompareToTest04_Key_Valueともnullではない_KeyとTimeが違う_Timeが優先() {
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt1 = new KeyValueTime("AAA", "Value1", dt);	//Keyの比較だけならこっちが前だけど…
			KeyValueTime kvt2 = new KeyValueTime("BBB", "Value1", dt.AddSeconds(-1.0)); //古い → 前に来る

			Assert.That(kvt1.CompareTo(kvt2), Is.GreaterThan(0));
			Assert.That(kvt2.CompareTo(kvt1), Is.LessThan(0));
		}

		[Test()]
		public void CompareToTest05_Key_Valueともnullではない_Timeが同じ_Valueでの比較になる() {
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt1 = new KeyValueTime("AAA", "ValueB", dt);	
			KeyValueTime kvt2 = new KeyValueTime("BBB", "ValueA", dt);	//Valueの比較で、こっちが前に来る 

			Assert.That(kvt1.CompareTo(kvt2), Is.GreaterThan(0));
			Assert.That(kvt2.CompareTo(kvt1), Is.LessThan(0));
		}

		[Test()]
		public void CompareToTest06_Key_Valueともnullではない_TimeもValueも同じ_Keyでの比較になる() {
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt1 = new KeyValueTime("BBB", "Value", dt);
			KeyValueTime kvt2 = new KeyValueTime("AAA", "Value", dt);	//Keyの比較で、こっちが前に来る 

			Assert.That(kvt1.CompareTo(kvt2), Is.GreaterThan(0));
			Assert.That(kvt2.CompareTo(kvt1), Is.LessThan(0));
		}

		[Test()]
		public void CompareToTest07_nullとの比較は例外() {
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt1 = new KeyValueTime("BBB", "Value", dt);

			Assert.Throws<ArgumentNullException>(new TestDelegate(() => kvt1.CompareTo(null)));
		}

		[Test()]
		public void CompareToTest08_互換性の無いオブジェクトとの比較は例外() {
			DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
			KeyValueTime kvt1 = new KeyValueTime("BBB", "Value", dt);
			KeyValuePair<string, string> kvp = new KeyValuePair<string, string>("CCC", "Value2");

			Assert.Throws<ArgumentException>(new TestDelegate(() => kvt1.CompareTo(kvp)));
		}


		//[Test()]
		//public void CompareToTest09_片方のValueがnull_Timeが同じ_Valueでの比較になる() {
		//    DateTime dt = new DateTime(2011, 8, 3, 13, 55, 0);
		//    KeyValueTime kvt1 = new KeyValueTime("AAA", "ValueB", dt);
		//    KeyValueTime kvt2 = new KeyValueTime("BBB", null, dt);	//Valueの比較で、こっちが前に来る 

		//    Assert.That(kvt1.CompareTo(kvt2), Is.GreaterThan(0));
		//    Assert.That(kvt2.CompareTo(kvt1), Is.LessThan(0));

		//    // ※ RED にならない。内部的に string.Compare() を呼び出していて、null も OK。Key も同じく。
		//}

	}
}
