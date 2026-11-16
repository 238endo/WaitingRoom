using WaitingRoom.Web.Stores;

namespace WaitingRoom.Web.Tests;

[TestClass]
public sealed class InMemoryQueueStoreTests
{
    /// <summary>
    /// 初期状態では呼び出し中の番号がなく、待機列が空であることを確認します。
    /// </summary>
    [TestMethod]
    public async Task GetSnapshotAsync_初期状態では待機列が空()
    {
        // 準備。番号を一度も発行していない、新しい受付データを用意する。
        var store = new InMemoryQueueStore();

        // 実行。現在の受付状況を取得する。None はキャンセルを指定しないという意味。
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);

        // IsNull は、渡した値が null かを確認する。ここでの null は「呼び出し中の番号なし」。
        Assert.IsNull(snapshot.CallingNumber);

        // IsEmpty は一覧を受け取り、要素が 0 件かを確認する。
        // Assert が失敗した時点でテストは不合格になる。両方の確認が成功すれば合格。
        Assert.IsEmpty(snapshot.WaitingNumbers);
    }
}
