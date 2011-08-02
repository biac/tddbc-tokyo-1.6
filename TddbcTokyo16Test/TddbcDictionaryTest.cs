﻿using System;
using System.Collections.Generic;

using NUnit.Framework;
using TddbcTokyo16;

namespace TddbcTokyo16Test {
	[TestFixture()]
	public class TddbcDictionaryTest {

		/*
		 ■ T16MAIN-1: putでkeyとvalueを追加し、dumpで一覧表示、getでkeyに対応するvalueを取得できる
			put で keyとvalueを追加する
			 • dumpで登録されている一覧を表示
			 • getで指定したkeyのvalueを取得
			注1)keyにnullをわたすと例外が発生
			注2)valueのnullは許容する
		 */
		//TODO: keyとvalueの型は? …分からないから、とりあえずstringにしとくか f(^^;


		[Test()]
		public void PutGetTest01_ひとつ登録_ひとつ取得() {
			const string key = "AAA";
			const string value = "Test";

			TddbcDictionary dic = new TddbcDictionary();
			dic.Put(key, value);

			string result = dic.Get(key);
			Assert.That(result, Is.EqualTo(value));
		}

		//[Test()]
		//public void PutGetTest02_keyにnull_例外() {
		//    const string value = "Test";

		//    TddbcDictionary dic = new TddbcDictionary();
		//    Assert.Throws<ArgumentNullException>(new TestDelegate(() => dic.Put(null, value)));

		//    //※ 実装変更無しで GREEN だった。不要。
		//}

		[Test()]
		public void DumpTest01_2つ登録してDump_ただし2つめのvalueはnull() {
			const string key1 = "BBB";
			const string value1 = "Test";
			const string key2 = "AAA";
			const string value2 = null;
	
			TddbcDictionary dic = new TddbcDictionary();
			dic.Put(key1, value1);
			dic.Put(key2, value2);

			IList<KeyValuePair<string, string>> dump = dic.Dump();
			Assert.That(dump[0].Key, Is.EqualTo(key1));
			Assert.That(dump[0].Value, Is.EqualTo(value1));
			Assert.That(dump[1].Key, Is.EqualTo(key2));
			Assert.That(dump[1].Value, Is.Null);


			//実際にコンソールにdumpするには、こうする
			foreach (var kv in dump) {
				Console.WriteLine("{0}: '{1}'", kv.Key, kv.Value ?? "(null)");
			}
			//実行結果:
			//BBB: 'Test'
			//AAA: '(null)'
		}



		/*
		 ■ T16MAIN-2: deleteで指定のkey-valueを削除
			deleteで指定したkeyとvalueを削除する
			 • 存在しないkeyが渡されたら何もしない
			 • nullを渡すと例外が発生する
		 */

		[Test()]
		public void DeleteTest01_ひとつ登録_ひとつ削除() {
			const string key = "AAA";
			const string value = "Test";

			TddbcDictionary dic = new TddbcDictionary();
			dic.Put(key, value);

			dic.Delete(key);
			//TODO: Get(key)で、該当するkeyが無かった時の挙動が不明。Dumpした個数でテストケースを書いておく。
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

		//[Test()]
		//public void DeleteTest03_keyにnull_例外() {
		//    TddbcDictionary dic = new TddbcDictionary();
		//    Assert.Throws<ArgumentNullException>(new TestDelegate(() => dic.Delete(null)));

		//    //※ 実装変更無しで GREEN だった。不要。
		//}



		/*
		 ■ T16MAIN-3: putの引数に既に存在するkeyが指定された場合、valueのみを更新する
			putで既に存在するkeyの場合はvalueを更新する
		 */

		[Test()]
		public void PutGetTest03_2回登録ただし同じkey() {
			const string key = "AAA";
			const string value1 = "Test1";
			const string value2 = "Test2";

			TddbcDictionary dic = new TddbcDictionary();
			dic.Put(key, value1);
			dic.Put(key, value2);

			string result = dic.Get(key);
			Assert.That(result, Is.EqualTo(value2));
		}



		/*
		 ■ T16MAIN-4: keyとvalueのセットを一度に複数追加できる
			複数のkeyとvalueをまとめて追加できるようにする
			同じキーが複数ある場合、一番最後に指定されたものが使用される。
			既に存在するキーがある場合も、今回指定したものが優先される。
			指定した引数の中にnullのキーがある場合、例外を投げ、状態を元に戻す。
		 */

		[Test()]
		public void MultiPutTest01_複数まとめて追加_重複キーあり_既存キーもあり() {
			// 既存データ
			const string key = "AAA";
			const string value = "Test";
			TddbcDictionary dic = new TddbcDictionary();
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
			dic.MultiPut(data);

			Assert.That(dic.Get("Key1"), Is.EqualTo("Value1"));
			Assert.That(dic.Get("Key2"), Is.EqualTo("Value2B"));
			Assert.That(dic.Get("Key3"), Is.EqualTo("Value3"));
			Assert.That(dic.Get("AAA"), Is.EqualTo("Value4"));
		}

		[Test()]
		public void MultiPutTest02_複数まとめて追加_nullキーあり_例外が出てロールバックされる() {
			// 既存データ
			const string key = "AAA";
			const string value = "Test";
			TddbcDictionary dic = new TddbcDictionary();
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


	}
}