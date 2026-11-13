using Microsoft.AspNetCore.Mvc;

namespace WaitingRoom.Web.Areas.Admin.Controllers;

/// <summary>
/// 同名のコントローラーとアクションを Area で区別する学習用の例です。
/// </summary>
[Area("Admin")]
public class HomeController : Controller
{
    /// <summary>
    /// Admin Area の URL から、共通で使うプライバシー画面のビューを返します。
    /// </summary>
    [HttpGet("/Admin/Home/Privacy")]
    public IActionResult Privacy()
    {
        // 通常の Privacy と同じビューを使うため、ファイルの場所を明示する。
        return View("~/Views/Home/Privacy.cshtml");
    }
}
