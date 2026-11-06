using WaitingRoom.Web.Models;

namespace WaitingRoom.Web.Stores;

public sealed class InMemoryQueueStore : IQueueStore
{
    private readonly QueueSnapshot _snapshot = new(null, Array.Empty<string>());

    /// <summary>
    /// メモリに保持している受付状況を返します。初期状態では呼び出し番号も待機番号もありません。
    /// </summary>
    public Task<QueueSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_snapshot);
    }
}
