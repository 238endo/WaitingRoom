using WaitingRoom.Web.Models;

namespace WaitingRoom.Web.Stores;

public interface IQueueStore
{
    /// <summary>
    /// 保存先から、呼び出し中の番号と発行順の待機番号をまとめて取得します。
    /// </summary>
    Task<QueueSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}
