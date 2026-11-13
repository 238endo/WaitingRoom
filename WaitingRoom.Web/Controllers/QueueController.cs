using Microsoft.AspNetCore.Mvc;
using WaitingRoom.Web.Models;
using WaitingRoom.Web.Services;

namespace WaitingRoom.Web.Controllers;

/// <summary>
/// 受付画面と待合室画面からの待機状況の取得や、
/// 受付画面からの「次を呼ぶ」のリクエストを受け取ります。
/// 受付状況などのデータや、処理が成功したかを返します。
/// </summary>
[ApiController]
[Route("api/queue")]
public sealed class QueueController : ControllerBase
{
    private readonly QueueService _queueService;

    public QueueController(QueueService queueService)
    {
        _queueService = queueService;
    }

    /// <summary>
    /// 現在の呼び出し番号と待機番号の一覧を取得し、HTTP 200で返します。
    /// </summary>
    [HttpGet]
    // 更新時に古い受付状況を使い回さないよう、応答をキャッシュに保存させない。
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ActionResult<QueueSnapshot>> Get(CancellationToken cancellationToken)
    {
        var snapshot = await _queueService.GetSnapshotAsync(cancellationToken);
        return Ok(snapshot);
    }

    /// <summary>
    /// 次の待機番号を呼び出し、番号をHTTP 200で返します。
    /// 呼び出し中の番号があるか、待機列が空の場合はHTTP 409を返します。
    /// </summary>
    [HttpPost("call-next")]
    public async Task<IActionResult> CallNext(CancellationToken cancellationToken)
    {
        var number = await _queueService.CallNextAsync(cancellationToken);
        if (number is null)
        {
            return Conflict();
        }

        return Ok(new TicketNumberResponse(number));
    }
}
