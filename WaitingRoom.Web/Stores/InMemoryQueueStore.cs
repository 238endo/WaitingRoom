using WaitingRoom.Web.Models;

namespace WaitingRoom.Web.Stores;

/// <summary>
/// 受付データを一時的にメモリに保存するクラスです。
/// DBに切り替えるときは、IQueueStoreを実装するDB用クラスを別ファイルに作ります。
/// DBから読むGetSnapshotAsyncは、そのDB用クラスに書きます。
/// Program.csの登録をDB用クラスに切り替えると、このメモリ用クラスは役目を終えます。
/// </summary>
public sealed class InMemoryQueueStore : IQueueStore
{
    private readonly object _queueLock = new();

    // 待機番号を発行順に保持します。初期状態では待機者はいません。
    private readonly Queue<string> _waitingNumbers = new();
    private int _lastIssuedNumber;

    /// <summary>
    /// メモリに保持している受付状況を返します。初期状態では呼び出し番号も待機番号もありません。
    /// </summary>
    public Task<QueueSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        lock (_queueLock)
        {
            // 呼び出し元からキャンセルが通知されていたら、処理を中断します。
            cancellationToken.ThrowIfCancellationRequested();

            // 取得後の発行で、このときの一覧が変わらないようにコピーします。
            var snapshot = new QueueSnapshot(
                null,
                _waitingNumbers.ToArray());

            return Task.FromResult(snapshot);
        }
    }

    /// <summary>
    /// A001から順に受付番号を発行し、待機列の末尾に追加します。
    /// </summary>
    public Task<string> IssueTicketAsync(CancellationToken cancellationToken)
    {
        // 同時に発行を頼まれても、番号の決定と待機列への追加を1件ずつ行います。
        lock (_queueLock)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nextNumber = checked(_lastIssuedNumber + 1);
            var ticketNumber = $"A{nextNumber:D3}";
            _waitingNumbers.Enqueue(ticketNumber);　// データ構造の末尾に追加することで、発行順を保持します。
            _lastIssuedNumber = nextNumber;

            return Task.FromResult(ticketNumber);
        }
    }
}
