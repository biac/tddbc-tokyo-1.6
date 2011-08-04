﻿using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using TddbcTokyo16;

namespace TddbcTokyo16Test {
	[TestFixture()]
	public class TddbcKeyValueStoreTest {

		/*
		 ■ T16MAIN-1: putでkeyとvalueを追加し、dumpで一覧表示、getでkeyに対応するvalueを取得できる
			put で keyとvalueを追加する
			 • dumpで登録されている一覧を表示
			 • getで指定したkeyのvalueを取得
			注1)keyにnullをわたすと例外が発生
			注2)valueのnullは許容する
			※ T16MAIN-8: Put() したときの時刻も保存される。

		 ■ T16MAIN-2: deleteで指定のkey-valueを削除
			deleteで指定したkeyとvalueを削除する
			 • 存在しないkeyが渡されたら何もしない
			 • nullを渡すと例外が発生する

		 ■ T16MAIN-3: putの引数に既に存在するkeyが指定された場合、valueのみを更新する
			putで既に存在するkeyの場合はvalueを更新する

		 ■ T16MAIN-4: keyとvalueのセットを一度に複数追加できる
			複数のkeyとvalueをまとめて追加できるようにする
			同じキーが複数ある場合、一番最後に指定されたものが使用される。
			既に存在するキーがある場合も、今回指定したものが優先される。
			指定した引数の中にnullのキーがある場合、例外を投げ、状態を元に戻す。
		 
		 ■ T16MAIN-5: putの引数でkey,value,date(時刻情報)を渡し、dumpを時間順に出力するように仕様変更
			putの引数でkey,value,date(時刻情報)を渡せるようにする。
			※ T16MAIN-8: Put() の引数は key, value だけのまま。date(時刻情報) としては、システム時刻を使う。
			また、dump関数は時刻が新しい方から古い方へ順にkey、valueを出力するように変更する。
			引数に複数指定して追加する関数の場合、後ろにあるものほど新しいとみなす。

		 ■ T16MAIN-6: dumpの引数に時刻を指定できるようにする。dump関数は時刻が指定された場合、指定時刻以降のデータのみを表示する
			dumpの引数に時刻（秒単位）を指定できるようにする。
			dump関数は時刻が指定された場合、指定時刻以降のデータのみを表示する

		 ■ T16MAIN-7: deleteの引数に分・秒を指定できる。deleteは分を指定された場合、「現在時刻-引数の分・秒」よりも古いデータをすべて削除する
			deleteの引数に分・秒を指定できる。deleteは分・秒を指定された場合、データの時刻情報が「現在時刻-引数の分・秒」よりも古いデータをすべて削除する
		 */

		//TODO: keyとvalueの型は? …分からないから、とりあえずstringにしとくか f(^^;


		[TearDown()]
		public void TearDown() {
#if DEBUG
			SystemClock.TestClearTime();
#endif
		}


#if DEBUG
		[Test()]
		public void PutGetTest01_ひとつ登録_ひとつ取得() {
			DateTime time = new DateTime(2011, 8, 3, 17, 40, 0);
			SystemClock.TestSetTime(time);

			const string key = "AAA";
			const string value = "Test";

			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			dic.Put(key, value);

			string result = dic.Get(key);
			Assert.That(result, Is.EqualTo(value));
			Assert.That(dic.Dump().First().Time, Is.EqualTo(time));
		}
#endif

		[Test()]
		public void PutGetTest02_keyにnull_例外() {
			const string value = "Test";

			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			Assert.Throws<ArgumentNullException>(new TestDelegate(() => dic.Put(null, value)));
		}

		[Test()]
		public void PutGetTest03_ひとつ登録_nullで取得() {
			const string key = "AAA";
			const string value = "Test";

			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			dic.Put(key, value);

			Assert.Throws<ArgumentNullException>(new TestDelegate(() => dic.Get(null)));
		}



#if DEBUG
		[Test()]
		public void MultiPutTest01_複数まとめて追加_重複キーあり_既存キーもあり() {
			DateTime time1 = new DateTime(2011, 8, 3, 17, 40, 0);
			DateTime time2 = time1.AddSeconds(3.0);

			// 既存データ
			const string key = "AAA";
			const string value = "Test";
			TddbcKeyValueStore dic = new TddbcKeyValueStore();

			SystemClock.TestSetTime(time1);
			dic.Put(key, value);

			// まとめて追加
			IList<KeyValuePair<string, string>> data
				= new List<KeyValuePair<string, string>>() {
					new KeyValuePair<string, string>("Key1", "Value1"),
					new KeyValuePair<string, string>("Key2", "Value2A"),
					new KeyValuePair<string, string>("Key2", "Value2B"),	//重複キー
					new KeyValuePair<string, string>("Key3", "Value3"),
					new KeyValuePair<string, string>("AAA", "Value4"),	//既存キーと同じ
				};
			SystemClock.TestSetTime(time2);
			dic.MultiPut(data);	//データ1件ごとに、1mSecずつずれる

			Assert.That(dic.Dump().Count, Is.EqualTo(4));

			// Dump() すると逆順になる (時刻の新しいものから)
			IList<KeyValueTime> dump = dic.Dump();
			Assert.That(dump[0].Key, Is.EqualTo("AAA"));
			Assert.That(dump[0].Value, Is.EqualTo("Value4"));
			Assert.That(dump[1].Key, Is.EqualTo("Key3"));
			Assert.That(dump[1].Value, Is.EqualTo("Value3"));
			Assert.That(dump[2].Key, Is.EqualTo("Key2"));
			Assert.That(dump[2].Value, Is.EqualTo("Value2B"));
			Assert.That(dump[3].Key, Is.EqualTo("Key1"));
			Assert.That(dump[3].Value, Is.EqualTo("Value1"));
		}
#endif

		[Test()]
		public void MultiPutTest02_複数まとめて追加_nullキーあり_例外が出てロールバックされる() {
			// 既存データ
			const string key = "AAA";
			const string value = "Test";
			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			dic.Put(key, value);

			// まとめて追加
			IList<KeyValuePair<string, string>> data
				= new List<KeyValuePair<string, string>>() {
					new KeyValuePair<string, string>("Key1", "Value1"),
					new KeyValuePair<string, string>("Key2", "Value2A"),
					new KeyValuePair<string, string>("Key2", "Value2B"),	//重複キー
					new KeyValuePair<string, string>("Key3", "Value3"),
					new KeyValuePair<string, string>("AAA", "Value4"),	//既存キーと同じ
					new KeyValuePair<string, string>(null, "Value5"),	//ここで例外
					new KeyValuePair<string, string>("Key6", "Value6"),
				};
			Assert.Throws<ArgumentNullException>(new TestDelegate(() => dic.MultiPut(data)));

			Assert.That(dic.Dump().Count, Is.EqualTo(1));
			Assert.That(dic.Get("AAA"), Is.EqualTo("Test"));
		}



		[Test()]
		public void GetTest01_存在しないキーを指定() {
			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			Assert.Throws<KeyNotFoundException>(new TestDelegate(() => dic.Get("NoKey")));
		}



#if DEBUG
		[Test()]
		public void DumpTest01_2つ登録してDump_ただし2つめのvalueはnull() {
			DateTime time1 = new DateTime(2011, 8, 3, 17, 40, 0);
			DateTime time2 = time1.AddSeconds(1.0);

			const string key1 = "BBB";
			const string value1 = "Test";
			const string key2 = "AAA";
			const string value2 = null;
	
			TddbcKeyValueStore dic = new TddbcKeyValueStore();

			SystemClock.TestSetTime(time1);
			dic.Put(key1, value1);

			SystemClock.TestSetTime(time2);
			dic.Put(key2, value2);

			IList<KeyValueTime> dump = dic.Dump();
			// Dump 順は、登録時の時刻の逆順。
			Assert.That(dump[0].Key, Is.EqualTo(key2));
			Assert.That(dump[0].Value, Is.Null);
			Assert.That(dump[0].Time, Is.EqualTo(time2));
			Assert.That(dump[1].Key, Is.EqualTo(key1));
			Assert.That(dump[1].Value, Is.EqualTo(value1));
			Assert.That(dump[1].Time, Is.EqualTo(time1));


			//実際にコンソールにdumpするには、こうする
			foreach (var kv in dump) {
				Console.WriteLine("{0}: '{1}' at {2}", 
									kv.Key, 
									kv.Value ?? "(null)", 
									kv.Time.ToString("HH:mm:ss"));
			}
			//実行結果:
			//AAA: '(null)' at 17:40:01
			//BBB: 'Test' at 17:40:00
		}
#endif

#if DEBUG
		[Test()]
		public void Dump02Test01_2つ登録してDump_時刻指定でひとつしか出ない() {
			const string key1 = "AAA";
			const string value1 = "Test1";
			DateTime time1 = new DateTime(2011, 8, 3, 17, 40, 0);
			const string key2 = "BBB";
			const string value2 = "Test2";
			DateTime time2 = time1.AddSeconds(1.0); //こっちが新しい → Dump(time2) で、これだけが出て来る

			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			SystemClock.TestSetTime(time1);
			dic.Put(key1, value1);
			SystemClock.TestSetTime(time2);
			dic.Put(key2, value2);

			IList<KeyValueTime> dump = dic.Dump(time2);
			Assert.That(dump.Count, Is.EqualTo(1));
			Assert.That(dump[0].Key, Is.EqualTo(key2));
			Assert.That(dump[0].Value, Is.EqualTo(value2));
			Assert.That(dump[0].Time, Is.EqualTo(time2));
		}
#endif



		[Test()]
		public void DeleteTest01_ひとつ登録_ひとつ削除() {
			const string key = "AAA";
			const string value = "Test";

			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			dic.Put(key, value);

			dic.Delete(key);
			Assert.That(dic.Dump().Count, Is.EqualTo(0));
		}

		//[Test()]
		//public void DeleteTest02_ひとつ登録_存在しないkeyで削除() {
		//    const string key = "AAA";
		//    const string value = "Test";

		//    TddbcDictionary dic = new TddbcDictionary();
		//    dic.Put(key, value);

		//    dic.Delete("NoWhere");
		//    Assert.That(dic.Get(key), Is.EqualTo(value));

		//    //※ 実装変更無しで GREEN だった。不要。
		//}

		[Test()]
		public void DeleteTest03_keyにnull_例外() {
			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			Assert.Throws<ArgumentNullException>(new TestDelegate(() => dic.Delete(null)));
		}


#if DEBUG
		[Test()]
		public void Delete2Test01_秒を指定() {
			const string key1 = "AAA";
			const string value1 = "Test1";
			DateTime time1 = new DateTime(2011, 8, 3, 17, 40, 0);
			const string key2 = "BBB";
			const string value2 = "Test2";
			DateTime time2 = time1.AddSeconds(1.0); 

			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			SystemClock.TestSetTime(time1);
			dic.Put(key1, value1);
			SystemClock.TestSetTime(time2);
			dic.Put(key2, value2);

			// 現在時刻を time2 より 2秒後にセット → 現在時刻から見ると、time2 は 2秒前、time1 は 3秒前
			SystemClock.TestSetTime(time2.AddSeconds(2.0));

			dic.Delete(0, 3);
			Assert.That(dic.Dump().Count, Is.EqualTo(2));	//1件も削除されない

			dic.Delete(0, 2);
			IList<KeyValueTime> dump = dic.Dump();
			Assert.That(dump.Count, Is.EqualTo(1));	//time1 だけ削除される
			Assert.That(dump[0].Key, Is.EqualTo(key2));	//time2 の方のデータは残っている

			dic.Delete(0, 1);
			Assert.That(dic.Dump().Count, Is.EqualTo(0));	//time2 も削除される
		}
#endif

#if DEBUG
		[Test()]
		public void Delete2Test02_分を指定() {
			const string key1 = "AAA";
			const string value1 = "Test1";
			DateTime time1 = new DateTime(2011, 8, 3, 17, 40, 0);
			const string key2 = "BBB";
			const string value2 = "Test2";
			DateTime time2 = time1.AddSeconds(1.0);

			TddbcKeyValueStore dic = new TddbcKeyValueStore();
			SystemClock.TestSetTime(time1);
			dic.Put(key1, value1);
			SystemClock.TestSetTime(time2);
			dic.Put(key2, value2);

			// 現在時刻を time2 より 1分後にセット → 現在時刻から見ると、time2 は 60秒前、time1 は 61秒前
			SystemClock.TestSetTime(time2.AddMinutes(1.0));

			dic.Delete(2, 0);
			Assert.That(dic.Dump().Count, Is.EqualTo(2));	//1件も削除されない

			dic.Delete(1, 0);
			IList<KeyValueTime> dump = dic.Dump();
			Assert.That(dump.Count, Is.EqualTo(1));	//time1 だけ削除される
			Assert.That(dump[0].Key, Is.EqualTo(key2));	//time2 の方のデータは残っている

			dic.Delete(0, 0);
			Assert.That(dic.Dump().Count, Is.EqualTo(0));	//time2 も削除される
		}
#endif

	}
}
