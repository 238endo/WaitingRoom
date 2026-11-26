using WaitingRoom.Web.Services;
using WaitingRoom.Web.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// アプリの起動時に、設定で選ばれた受付データの保存先を登録する。
switch (builder.Configuration["QueueStore:Provider"])
{
    case "InMemory":
        builder.Services.AddSingleton<IQueueStore, InMemoryQueueStore>();
        break;

    case "Sql":
        // SQL版を選んだときだけ、Azure SQLの接続先を読み取る。
        var connectionString = builder.Configuration.GetConnectionString("WaitingRoom")
            ?? throw new InvalidOperationException("接続先 WaitingRoom が設定されていません。");

        builder.Services.AddSingleton<IQueueStore>(new SqlQueueStore(connectionString));
        break;

    default:
        throw new InvalidOperationException(
            "QueueStore:Provider に InMemory または Sql を設定してください。");
}

builder.Services.AddScoped<QueueService>();

var app = builder.Build();

// 開発環境以外では、利用者向けのエラー画面とHTTPSの設定を有効にする。
if (!app.Environment.IsDevelopment())
{
    // 処理中に例外が起きたら、Homeのエラー画面を表示する。
    app.UseExceptionHandler("/Home/Error");

    // ブラウザーに、次回以降もHTTPSで接続するよう伝える。
    app.UseHsts();
}

// HTTPで来たリクエストをHTTPSへ案内する。
app.UseHttpsRedirection();

// リクエストのURLに合う処理を探す。
app.UseRouting();

// アクセス制限がある処理では、実行してよいか確認する。
app.UseAuthorization();

// CSSやJavaScriptなどの静的ファイルを配信する。
app.MapStaticAssets();

// URLとControllerの処理を結び付ける。トップページはHomeのIndexを使う。
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    // MVCの画面から、最適化された静的ファイルを参照できるようにする。
    .WithStaticAssets();

// アプリを起動し、リクエストの受付を始める。
app.Run();
