namespace WaitingRoom.Web.Models;

/// <summary>
/// 受付番号の発行や呼び出しが成功したときに返すレスポンスです。
/// </summary>
/// <param name="Number">発行または呼び出しした受付番号。例は A001。</param>
public sealed record TicketNumberResponse(string Number);
