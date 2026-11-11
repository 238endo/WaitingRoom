using Microsoft.AspNetCore.Mvc;

namespace WaitingRoom.Web.Controllers;

/// <summary>
/// 画面を開くリクエストを受け取り、ブラウザーにHTMLを返すコントローラーです。
/// 受付画面や待合室画面の表示を担当します。
/// </summary>
public class PagesController : Controller
{
    /// <summary>
    /// 受付係が使う受付画面を表示します。
    /// </summary>
    public IActionResult Reception()
    {
        // Views/Pages/Reception.cshtml を使って
        // 画面のHTMLを返すよう、ASP.NET Coreに伝えます。

        // View() を引数なしで呼ぶと、ASP.NET Core MVC は
        // 「Views/<コントローラー名>/<アクション名>.cshtml」の順でビューを探します。
        //
        // フォルダ名の Pages は、クラス名 PagesController から
        // 末尾の Controller を外した名前です。
        // ファイル名の Reception は、このアクション Reception() の名前です。
        //
        // そのため、Views/Pages/Reception.cshtml が選ばれ、
        // このファイルから生成された HTML がブラウザーに返ります。
        return View();
    }
}