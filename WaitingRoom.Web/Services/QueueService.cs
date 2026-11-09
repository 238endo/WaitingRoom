using WaitingRoom.Web.Models;
using WaitingRoom.Web.Stores;

namespace WaitingRoom.Web.Services;

public sealed class QueueService
{
    private readonly IQueueStore _queueStore;

    public QueueService(IQueueStore queueStore)
    {
        _queueStore = queueStore;
    }

    /// <summary>
    /// 設定された保存先から、現在の受付状況を取得します。
    /// </summary>
    public Task<QueueSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        return _queueStore.GetSnapshotAsync(cancellationToken);
    }

    /// <summary>
    /// Storeに受付番号の発行を依頼し、発行された番号を返します。
    /// </summary>
    public Task<string> IssueTicketAsync(CancellationToken cancellationToken)
    {
        return _queueStore.IssueTicketAsync(cancellationToken);
    }

    /// <summary>
    /// ストアに次の番号の呼び出しを頼み、呼び出した番号を返します。
    /// 呼び出せない場合は null を返します。
    /// </summary>
    public Task<string?> CallNextAsync(CancellationToken cancellationToken)
    {
        return _queueStore.CallNextAsync(cancellationToken);
    }

    /// <summary>
    /// ストアに指定した番号の案内完了を頼み、結果を返します。
    /// </summary>
    /// <param name="number">案内を完了する受付番号。例は A001。</param>
    public Task<TicketCompletionResult> CompleteTicketAsync(
        string number,
        CancellationToken cancellationToken)
    {
        return _queueStore.CompleteTicketAsync(number, cancellationToken);
    }
}
