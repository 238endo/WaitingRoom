using Microsoft.AspNetCore.Mvc;
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
        return StatusCode(StatusCodes.Status201Created, new { number });
    }
}
