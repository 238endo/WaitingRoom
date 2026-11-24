# MSTest メモ

MSTest は、C# で書いたテストを実行して合否を判定する仕組み。今回のテストでは、受付の初期状態を確認する。 [InMemoryQueueStoreTests.cs](../../WaitingRoom.Web.Tests/InMemoryQueueStoreTests.cs)

## テストコードの読み方

| 記述 | 役割 |
|---|---|
| `[TestClass]` | このクラスにテストが入っていることを MSTest に伝える。 |
| `[TestMethod]` | このメソッドをテストとして実行するよう MSTest に伝える。 |
| `Assert` | 実行結果が期待どおりかを確かめ、違っていたらテストを不合格にする。 |

テストコードは、空の受付データを作る（準備）、`GetSnapshotAsync` を呼ぶ（実行）、返された受付状況を `Assert` で確かめる（確認）の順に読む。

## 今回の Assert

```csharp
// CallingNumber が null なら、呼び出し中の番号はない。
Assert.IsNull(snapshot.CallingNumber);

// 一覧を渡すと、待機番号が 0 件かを確認できる。
Assert.IsEmpty(snapshot.WaitingNumbers);
```

- `IsNull` は値が `null` なら成功する。たとえば `"A001"` が入っていたら失敗する。
- `IsEmpty` は一覧を受け取り、要素が0件なら成功する。`snapshot.WaitingNumbers.Count` は件数を表す整数なので、件数を渡したい場合は `Assert.AreEqual(0, snapshot.WaitingNumbers.Count)` と書ける。`AreEqual` の引数は **期待値、実際の値** の順。
- 最初の `Assert` が失敗すると、その時点でテストは不合格になり、次の `Assert` には進まない。両方が成功すれば合格。

[Microsoft Learn の Assert 解説](https://learn.microsoft.com/ja-jp/dotnet/core/testing/unit-testing-mstest-writing-tests-assertions)

## Visual Studio で実行する

1. [WaitingRoom.slnx](../../WaitingRoom.slnx) を開き、**「テスト」→「テスト エクスプローラー」** を選ぶ。
2. `GetSnapshotAsync_初期状態では待機列が空` を右クリックし、**「実行」** を選ぶ。一覧に表示されない場合は、先にソリューションをビルドする。
3. 緑のチェックなら合格。赤い印が出たらテスト名を選び、詳細欄の失敗メッセージを読む。

このテストはテスト専用の受付データを使う。Web アプリや Azure を起動する操作は不要。
