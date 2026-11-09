namespace WaitingRoom.Web.Models;

/// <summary>
/// 呼び出し中の番号と待機番号を、ある時点の受付状況としてまとめます。
/// </summary>
/// <param name="CallingNumber">呼び出し中の受付番号。例は A001。呼び出し中の番号がなければ null。</param>
/// <param name="WaitingNumbers">待機中の受付番号を、発行順に並べた一覧。待機者がいなければ空の一覧。</param>
public sealed record QueueSnapshot(
    string? CallingNumber,
    IReadOnlyList<string> WaitingNumbers);
