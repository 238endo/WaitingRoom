using Microsoft.Data.SqlClient;
using WaitingRoom.Web.Stores;

namespace WaitingRoom.Web.Tests;

/// <summary>
/// SQL保存のテスト用に、このPCでデータベースを作り、テスト後に片付けます。
/// このDBを使って実行する9件のテストは QueueStoreTests.cs にあります。
/// </summary>
[TestClass]
// MSTestはSQL版のテストを1件ずつ実行する。複数のDB作成・削除が重ならないようにするため。
[DoNotParallelize]
public sealed class SqlQueueStoreTests : QueueStoreTests
{
    // このPCのLocalDBに接続する。master は、テスト用DBを作成・削除する操作の接続先。
    // Pooling=False にすると、使い終わった接続が再利用のために残らず、DBを削除できる。
    private const string LocalDbConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;Pooling=False";

    // 毎回違う名前を付けるので、同じPCにある開発用DBとは別のDBになる。
    private readonly string _databaseName = $"WaitingRoomTests_{Guid.NewGuid():N}";
    // テスト用DBを作ったあとに、そのDBへの接続先をここに保存する。
    private string? _testConnectionString;

    /// <summary>
    /// 各テストの前に、そのテスト専用のDBと空の受付テーブルを作ります。
    /// </summary>
    [TestInitialize] // MSTestが各テストの直前に呼ぶ。
    public async Task PrepareDatabaseAsync()
    {
        // LocalDBはWindowsで使うため、それ以外ではテストを判定できないことを伝える。
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("WindowsのSQL Server Express LocalDBが必要です。");
        }

        // まず master に接続し、今回のテストだけが使うDBを作る。
        await RunSqlAsync(LocalDbConnectionString, $"CREATE DATABASE [{_databaseName}];");

        // 接続先のDB名を master から、さっき作ったテスト用DBに変える。
        var connection = new SqlConnectionStringBuilder(LocalDbConnectionString)
        {
            InitialCatalog = _databaseName
        };
        _testConnectionString = connection.ConnectionString;

        // ビルド時にコピーされたSQLを読み、テスト用DBの中に受付テーブルを作る。
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Database", "001-create-queue-tables.sql");
        await RunSqlAsync(_testConnectionString, await File.ReadAllTextAsync(schemaPath));
    }

    /// <summary>
    /// 共通のテストが使うSQL保存用ストアを返します。
    /// </summary>
    // 共通テストの CreateStore() がここに来る。! は、直前の準備で値が入ることをコンパイラーに伝える。
    protected override IQueueStore CreateStore() => new SqlQueueStore(_testConnectionString!);

    /// <summary>
    /// テストの後に、そのテストで作ったDBを片付けます。
    /// </summary>
    [TestCleanup] // MSTestが各テストの終了後に呼ぶ。
    public async Task RemoveDatabaseAsync()
    {
        // 作成できたときだけ master に接続し、今回のテストで付けた名前のDBを削除する。
        if (_testConnectionString is not null)
        {
            await RunSqlAsync(LocalDbConnectionString, $"DROP DATABASE [{_databaseName}];");
        }
    }

    /// <summary>
    /// テスト用DBの準備と片付けに使うSQLを実行します。
    /// </summary>
    private static async Task RunSqlAsync(
        string connectionString,
        string sql)
    {
        // 接続を開き、終わったら await using で自動的に閉じる。
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // 呼び出し元から受け取ったCREATEやDROPなどのSQLを送る。
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}
