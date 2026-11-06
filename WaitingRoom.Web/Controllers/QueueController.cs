using Microsoft.AspNetCore.Mvc;
using WaitingRoom.Web.Models;
using WaitingRoom.Web.Services;

namespace WaitingRoom.Web.Controllers;

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
}
