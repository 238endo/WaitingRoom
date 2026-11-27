using WaitingRoom.Web.Stores;
using WaitingRoom.Web.Models;

namespace WaitingRoom.Web.Tests;

/// <summary>
/// 番号発行や呼び出しなど、メモリ保存とSQL保存で共通の受付ルール9件を確認します。
/// </summary>
[TestClass]
public abstract class QueueStoreTests
{
    /// <summary>
    /// 各テストで使う、空の受付データにつながるストアを返します。
    /// </summary>
    protected abstract IQueueStore CreateStore();

    /// <summary>
    /// 初期状態では呼び出し中の番号がなく、待機列が空であることを確認します。
    /// </summary>
    [TestMethod]
    public async Task GetSnapshotAsync_初期状態では待機列が空()
    {
        // 準備。番号を一度も発行していない、新しい受付データを用意する。
        var store = CreateStore();

        // 実行。現在の受付状況を取得する。None はキャンセルを指定しないという意味。
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);

        // IsNull は、渡した値が null かを確認する。ここでの null は「呼び出し中の番号なし」。
        Assert.IsNull(snapshot.CallingNumber);

        // IsEmpty は一覧を受け取り、要素が 0 件かを確認する。
        // Assert が失敗した時点でテストは不合格になる。両方の確認が成功すれば合格。
        Assert.IsEmpty(snapshot.WaitingNumbers);
    }

    /// <summary>
    /// 先に発行した番号が呼ばれ、後に発行した番号が待機列に残ることを確認します。
    /// </summary>
    [TestMethod]
    public async Task CallNextAsync_先に発行した番号を呼び出す()
    {
        // 準備。空の受付データを作り、発行した2件の番号を順に控える。
        var store = CreateStore();
        var firstNumber = await store.IssueTicketAsync(CancellationToken.None);
        var secondNumber = await store.IssueTicketAsync(CancellationToken.None);

        // 実行。「次を呼ぶ」を1回行い、そのあとの受付状況を取得する。
        var calledNumber = await store.CallNextAsync(CancellationToken.None);
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);

        // 先に発行した番号が返り、受付状況でも呼び出し中になっているか確認する。
        Assert.AreEqual(firstNumber, calledNumber);
        Assert.AreEqual(firstNumber, snapshot.CallingNumber);

        // 待機列が1件であることを確かめてから、その番号を確認する。
        Assert.HasCount(1, snapshot.WaitingNumbers);
        Assert.AreEqual(secondNumber, snapshot.WaitingNumbers[0]);
    }

    /// <summary>
    /// 待機列が空なら呼び出す番号を返さず、受付状況も変えないことを確認します。
    /// </summary>
    [TestMethod]
    public async Task CallNextAsync_待機列が空なら番号を呼び出さない()
    {
        // 準備。番号を発行していない受付データを用意する。
        var store = CreateStore();

        // 実行。待機列が空のまま、次の番号を呼ぶ。
        var calledNumber = await store.CallNextAsync(CancellationToken.None);

        // 確認。戻り値も呼び出し中の番号もなく、待機列は空のまま。
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);
        Assert.IsNull(calledNumber);
        Assert.IsNull(snapshot.CallingNumber);
        Assert.IsEmpty(snapshot.WaitingNumbers);
    }

    /// <summary>
    /// 呼び出し中に次を呼んでも、現在の呼び出し番号と待機列が変わらないことを確認します。
    /// </summary>
    [TestMethod]
    public async Task CallNextAsync_呼び出し中なら次の番号を呼び出さない()
    {
        // 準備。1件目を呼び出し中、2件目を待機中にする。
        var store = CreateStore();
        var firstNumber = await store.IssueTicketAsync(CancellationToken.None);
        var secondNumber = await store.IssueTicketAsync(CancellationToken.None);
        await store.CallNextAsync(CancellationToken.None);

        // 実行。1件目の案内を完了する前に、もう一度次の番号を呼ぶ。
        var calledNumber = await store.CallNextAsync(CancellationToken.None);

        // 確認。新しい番号は返らず、2件の状態は変わらない。
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);
        Assert.IsNull(calledNumber);
        Assert.AreEqual(firstNumber, snapshot.CallingNumber);
        Assert.HasCount(1, snapshot.WaitingNumbers);
        Assert.AreEqual(secondNumber, snapshot.WaitingNumbers[0]);
    }

    /// <summary>
    /// A001 から順に番号を発行し、その順で待機列に追加することを確認します。
    /// </summary>
    [TestMethod]
    public async Task IssueTicketAsync_A001から順に発行して待機列に追加する()
    {
        // 準備。まだ番号を発行していない受付データを用意する。
        var store = CreateStore();

        // 実行。番号を2件発行し、発行後の受付状況を取得する。
        var firstNumber = await store.IssueTicketAsync(CancellationToken.None);
        var secondNumber = await store.IssueTicketAsync(CancellationToken.None);
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);

        // 発行番号そのものを確かめるため、仕様で決まっている番号を期待値にする。
        Assert.AreEqual("A001", firstNumber);
        Assert.AreEqual("A002", secondNumber);

        // 件数を確認してから、待機列の1件目と2件目を順に確認する。
        Assert.HasCount(2, snapshot.WaitingNumbers);
        Assert.AreEqual("A001", snapshot.WaitingNumbers[0]);
        Assert.AreEqual("A002", snapshot.WaitingNumbers[1]);
    }

    /// <summary>
    /// 案内完了で呼び出し中の番号がなくなり、待機中の番号を次に呼べることを確認します。
    /// </summary>
    [TestMethod]
    public async Task CompleteTicketAsync_呼び出し中の番号の案内を完了する()
    {
        // 準備。番号を2件発行し、1件目を呼び出し中にする。
        var store = CreateStore();
        var firstNumber = await store.IssueTicketAsync(CancellationToken.None);
        var secondNumber = await store.IssueTicketAsync(CancellationToken.None);
        await store.CallNextAsync(CancellationToken.None);

        // 実行。1件目の案内を完了し、そのあとの受付状況を取得する。
        var result = await store.CompleteTicketAsync(
            firstNumber,
            CancellationToken.None);
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);

        // 戻り値で案内完了の結果を確かめ、受付状況で残っている番号を確認する。
        Assert.AreEqual(TicketCompletionResult.Completed, result);
        Assert.IsNull(snapshot.CallingNumber);
        Assert.HasCount(1, snapshot.WaitingNumbers);
        Assert.AreEqual(secondNumber, snapshot.WaitingNumbers[0]);

        // 案内完了を確認してから次を呼び、2件目が呼び出し中になることを確かめる。
        var nextNumber = await store.CallNextAsync(CancellationToken.None);
        var snapshotAfterNextCall = await store.GetSnapshotAsync(CancellationToken.None);
        Assert.AreEqual(secondNumber, nextNumber);
        Assert.AreEqual(secondNumber, snapshotAfterNextCall.CallingNumber);
        Assert.IsEmpty(snapshotAfterNextCall.WaitingNumbers);
    }

    /// <summary>
    /// 待機中の番号は案内完了できず、呼び出し中と待機中の番号が残ることを確認します。
    /// </summary>
    [TestMethod]
    public async Task CompleteTicketAsync_待機中の番号は案内完了にしない()
    {
        // 準備。1件目は呼び出し中、2件目は待機中にする。
        var store = CreateStore();
        var firstNumber = await store.IssueTicketAsync(CancellationToken.None);
        var secondNumber = await store.IssueTicketAsync(CancellationToken.None);
        await store.CallNextAsync(CancellationToken.None);

        // 実行。待機中の2件目を指定して案内完了を試す。
        var result = await store.CompleteTicketAsync(
            secondNumber,
            CancellationToken.None);

        // 確認。2件目は呼び出し中ではないと判定され、受付状況は変わらない。
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);
        Assert.AreEqual(TicketCompletionResult.NotCalling, result);
        Assert.AreEqual(firstNumber, snapshot.CallingNumber);
        Assert.HasCount(1, snapshot.WaitingNumbers);
        Assert.AreEqual(secondNumber, snapshot.WaitingNumbers[0]);
    }

    /// <summary>
    /// 未発行の番号は見つからず、受付状況が空のままであることを確認します。
    /// </summary>
    [TestMethod]
    public async Task CompleteTicketAsync_未発行の番号は見つからない()
    {
        // 準備。番号を一度も発行していない受付データを用意する。
        var store = CreateStore();

        // 実行。まだ発行していない A001 の案内完了を試す。
        var result = await store.CompleteTicketAsync(
            "A001",
            CancellationToken.None);

        // 確認。番号が見つからず、呼び出し中の番号と待機番号はないまま。
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);
        Assert.AreEqual(TicketCompletionResult.NotFound, result);
        Assert.IsNull(snapshot.CallingNumber);
        Assert.IsEmpty(snapshot.WaitingNumbers);
    }

    /// <summary>
    /// 案内完了済みの番号を再度指定しても、案内完了を受け付けないことを確認します。
    /// </summary>
    [TestMethod]
    public async Task CompleteTicketAsync_案内完了済みの番号は再度完了にしない()
    {
        // 準備。1件発行し、呼び出しと案内完了を済ませる。
        var store = CreateStore();
        var ticketNumber = await store.IssueTicketAsync(CancellationToken.None);
        await store.CallNextAsync(CancellationToken.None);
        await store.CompleteTicketAsync(
            ticketNumber,
            CancellationToken.None);

        // 実行。同じ番号の案内完了をもう一度試す。
        var result = await store.CompleteTicketAsync(
            ticketNumber,
            CancellationToken.None);

        // 確認。発行済みの番号だが呼び出し中ではないと判定し、受付状況は変わらない。
        var snapshot = await store.GetSnapshotAsync(CancellationToken.None);
        Assert.AreEqual(TicketCompletionResult.NotCalling, result);
        Assert.IsNull(snapshot.CallingNumber);
        Assert.IsEmpty(snapshot.WaitingNumbers);
    }
}
