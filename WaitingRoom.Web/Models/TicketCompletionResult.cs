namespace WaitingRoom.Web.Models;

/// <summary>
/// 指定した受付番号の案内を完了できたかを表します。
/// </summary>
public enum TicketCompletionResult
{
    /// <summary>
    /// 指定した受付番号の案内を完了しました。
    /// </summary>
    Completed,
    /// <summary>
    /// 指定した受付番号は発行されていません。
    /// </summary>
    NotFound,
    /// <summary>
    /// 指定した受付番号は発行済みですが、現在は呼び出し中ではありません。
    /// </summary>
    NotCalling
}
