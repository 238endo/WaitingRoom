using WaitingRoom.Web.Models;

namespace WaitingRoom.Web.Stores;

public interface IQueueStore
{
    /// <summary>
    /// 保存先から、呼び出し中の番号と発行順の待機番号をまとめて取得します。
    /// </summary>
    Task<QueueSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 新しい受付番号を発行して待機列に追加し、その番号を返します。
    /// </summary>
    Task<string> IssueTicketAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 待機列の先頭を呼び出し中にして、その番号を返します。
    /// 呼び出し中の番号があるか、待機列が空の場合は null を返します。
    /// </summary>
    Task<string?> CallNextAsync(CancellationToken cancellationToken);
}
