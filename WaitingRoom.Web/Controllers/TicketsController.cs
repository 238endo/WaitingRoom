using Microsoft.AspNetCore.Mvc;
using WaitingRoom.Web.Models;
using WaitingRoom.Web.Services;

namespace WaitingRoom.Web.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase
{
    private readonly QueueService _queueService;

    public TicketsController(QueueService queueService)
    {
        _queueService = queueService;
    }

    /// <summary>
    /// 受付番号を発行して待機列に追加し、発行した番号をHTTP 201で返します。
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Issue(CancellationToken cancellationToken)
    {
        var number = await _queueService.IssueTicketAsync(cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new TicketNumberResponse(number));
    }

    /// <summary>
    /// 指定した番号の案内を完了します。成功なら HTTP 204、
    /// 未発行なら HTTP 404、呼び出し中でなければ HTTP 409 を返します。
    /// </summary>
    /// <param name="number">URLで指定した受付番号。例は A001。</param>
    [HttpPost("{number}/complete")]
    public async Task<IActionResult> Complete(
        string number,
        CancellationToken cancellationToken)
    {
        var result = await _queueService.CompleteTicketAsync(number, cancellationToken);
        return result switch
        {
            TicketCompletionResult.Completed => NoContent(),
            TicketCompletionResult.NotFound => NotFound(),
            TicketCompletionResult.NotCalling => Conflict(),
            _ => throw new InvalidOperationException($"未定義の案内完了結果です。{result}")
        };
    }
}
