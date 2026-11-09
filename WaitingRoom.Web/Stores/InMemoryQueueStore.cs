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
    // 案内完了後も、発行済みの番号かどうかを判定できるように保持します。
    private readonly HashSet<string> _issuedNumbers = new(StringComparer.Ordinal);
    private int _lastIssuedNumber;
    private string? _callingNumber;

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
                _callingNumber,
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
            _issuedNumbers.Add(ticketNumber);
            _lastIssuedNumber = nextNumber;

            return Task.FromResult(ticketNumber);
        }
    }

    /// <summary>
    /// 待機列の先頭を呼び出し中にして、その番号を返します。
    /// 呼び出し中の番号があるか、待機列が空の場合は null を返します。
    /// </summary>
    public Task<string?> CallNextAsync(CancellationToken cancellationToken)
    {
        // 同時に操作されても、呼び出せるかの判定と更新を1件ずつ行います。
        lock (_queueLock)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_callingNumber is not null || !_waitingNumbers.TryDequeue(out var nextNumber))
            {
                return Task.FromResult<string?>(null);
            }

            _callingNumber = nextNumber;
            return Task.FromResult<string?>(nextNumber);
        }
    }

    /// <summary>
    /// 呼び出し中の番号の案内を完了し、完了できたかを返します。
    /// </summary>
    /// <param name="number">案内を完了する受付番号。例は A001。</param>
    public Task<TicketCompletionResult> CompleteTicketAsync(
        string number,
        CancellationToken cancellationToken)
    {
        lock (_queueLock)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_issuedNumbers.Contains(number))
            {
                return Task.FromResult(TicketCompletionResult.NotFound);
            }

            if (_callingNumber != number)
            {
                return Task.FromResult(TicketCompletionResult.NotCalling);
            }

            _callingNumber = null;
            return Task.FromResult(TicketCompletionResult.Completed);
        }
    }
}
