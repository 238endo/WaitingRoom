using Microsoft.Data.SqlClient;
using WaitingRoom.Web.Models;

namespace WaitingRoom.Web.Stores;

/// <summary>
/// Azure SQL Databaseに保存された受付状況を読み取ります。
/// </summary>
public sealed class SqlQueueStore : IQueueStore
{
    private readonly string _connectionString;

    /// <summary>
    /// 受付データを読み取るためのDBの接続先を受け取ります。
    /// </summary>
    public SqlQueueStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// 呼び出し中の番号と、発行順に並んだ待機番号を返します。
    /// </summary>
    public async Task<QueueSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // 待機中（0）と呼び出し中（1）を、発行した順番で取得する。
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TicketSequence, Status
            FROM dbo.Tickets
            WHERE Status IN (0, 1)
            ORDER BY TicketSequence;
            """;

        string? callingNumber = null;
        var waitingNumbers = new List<string>();

        // 今回は一覧を取得するので、取得した結果を1行ずつ読む。
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var sequence = reader.GetInt32(0);
            var status = reader.GetByte(1);

            // DBに保存された連番の1を、画面に表示する A001 に変換する。
            var ticketNumber = $"A{sequence:D3}";

            if (status == 1)
            {
                callingNumber = ticketNumber;
            }
            else
            {
                waitingNumbers.Add(ticketNumber);
            }
        }

        return new QueueSnapshot(
            callingNumber,
            waitingNumbers);
    }

    /// <summary>
    /// 待機列の先頭を呼び出し中にして、受付番号を返します。
    /// 呼び出し中の番号があるか、待機列が空の場合は null を返します。
    /// </summary>
    public async Task<string?> CallNextAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        // 窓口1の行をロックして、同じ窓口の受付操作が同時に進むのを防ぐ。
        // 呼び出し中の番号がなければ、最も早く発行した待機番号の状態を
        // 「待機中（0）」から「呼び出し中（1）」に変える。
        command.CommandText = """
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.QueueState WITH (UPDLOCK, HOLDLOCK)
            WHERE QueueId = 1
        )
        BEGIN
            THROW 50001, N'窓口1の採番データが見つかりません。', 1;
        END;

        IF NOT EXISTS (SELECT 1 FROM dbo.Tickets WHERE Status = 1)
        BEGIN
            UPDATE dbo.Tickets
            SET Status = 1
            OUTPUT INSERTED.TicketSequence
            WHERE TicketSequence = (
                SELECT TOP (1) TicketSequence
                FROM dbo.Tickets
                WHERE Status = 0
                ORDER BY TicketSequence
            );
        END;
        """;

        // 更新した番号を受け取る。更新対象がなければ値は返らない。
        var result = await command.ExecuteScalarAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return result is int sequence ? $"A{sequence:D3}" : null;
    }

    /// <summary>
    /// 次の受付番号を発行し、待機中としてDBに保存してから、A001形式の番号を返します。
    /// </summary>
    public async Task<string> IssueTicketAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // 採番と受付データの追加を、まとめて確定できるようにする。
        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using var numberCommand = connection.CreateCommand();
        numberCommand.Transaction = transaction;
        numberCommand.CommandText = """
        UPDATE dbo.QueueState
        SET LastIssuedNumber = LastIssuedNumber + 1
        OUTPUT INSERTED.LastIssuedNumber
        WHERE QueueId = 1;
        """;

        // OUTPUT で、更新したあとの連番を受け取る。初回は 0 から 1 になる。
        var result = await numberCommand.ExecuteScalarAsync(cancellationToken);
        if (result is not int nextNumber)
        {
            throw new InvalidOperationException("窓口1の採番データが見つかりません。");
        }

        await using var insertCommand = connection.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = """
        INSERT INTO dbo.Tickets (TicketSequence, Status)
        VALUES (@TicketSequence, 0);
        """;

        // SQL内の @TicketSequence に採番した連番を渡す。状態の 0 は「待機中」。
        insertCommand.Parameters.Add(
            "@TicketSequence",
            System.Data.SqlDbType.Int).Value = nextNumber;
        await insertCommand.ExecuteNonQueryAsync(cancellationToken);

        // 両方のSQLが成功したら確定し、画面で使う形式の番号を返す。
        await transaction.CommitAsync(cancellationToken);
        return $"A{nextNumber:D3}";
    }

    /// <summary>
    /// 呼び出し中の番号を案内完了にして、処理結果を返します。
    /// </summary>
    /// <param name="number">案内を完了する受付番号。例は A001。</param>
    public async Task<TicketCompletionResult> CompleteTicketAsync(
        string number,
        CancellationToken cancellationToken)
    {
        // 画面から届く A001 を、DBで検索するための連番 1 に変換する。
        // A1 のように発行時と違う表記は、未発行の番号として扱う。
        if (string.IsNullOrEmpty(number)
            || number[0] != 'A'
            || !int.TryParse(number.AsSpan(1), out var sequence)
            || sequence <= 0
            || !string.Equals(number, $"A{sequence:D3}", StringComparison.Ordinal))
        {
            return TicketCompletionResult.NotFound;
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        // 「次を呼ぶ」と同じ窓口行をロックしてから、指定された番号の状態を確認する。
        // 呼び出し中（1）なら案内完了（2）に変更し、判定用に変更前の状態を返す。
        command.CommandText = """
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.QueueState WITH (UPDLOCK, HOLDLOCK)
            WHERE QueueId = 1
        )
        BEGIN
            THROW 50001, N'窓口1の採番データが見つかりません。', 1;
        END;

        DECLARE @PreviousStatus tinyint;

        SELECT @PreviousStatus = Status
        FROM dbo.Tickets
        WHERE TicketSequence = @TicketSequence;

        IF @PreviousStatus = 1
        BEGIN
            UPDATE dbo.Tickets
            SET Status = 2
            WHERE TicketSequence = @TicketSequence;
        END;

        SELECT @PreviousStatus;
        """;

        command.Parameters.Add(
            "@TicketSequence",
            System.Data.SqlDbType.Int).Value = sequence;

        var previousStatus = await command.ExecuteScalarAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // 該当する行がなければ、SQLの NULL が返る。
        return previousStatus switch
        {
            (byte)1 => TicketCompletionResult.Completed,
            byte => TicketCompletionResult.NotCalling,
            _ => TicketCompletionResult.NotFound
        };
    }
}