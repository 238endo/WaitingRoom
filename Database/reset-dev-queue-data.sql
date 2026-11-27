-- 0 のままなら初期化しない。実際に初期化するときだけ 1 に変更する。
DECLARE @ConfirmReset bit = 0;

-- 現在接続しているデータベース名を確認する。
IF DB_NAME() <> N'waitingroom-dev'
BEGIN
    THROW 50001, N'接続先を waitingroom-dev に切り替えてください。', 1;
END;

-- 実行許可がなければ、ここで処理を終了する。
IF @ConfirmReset <> 1
BEGIN
    SELECT N'初期化は実行されませんでした。' AS ResetStatus;
    RETURN;
END;

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- 採番を 0 に戻す。この行は番号発行の処理でも使うため、先に更新する。
    UPDATE dbo.QueueState
    SET LastIssuedNumber = 0
    WHERE QueueId = 1;

    -- 初期行がなければ、受付データを削除する前に停止する。
    IF @@ROWCOUNT <> 1
    BEGIN
        THROW 50002, N'QueueState に窓口1の初期行がありません。', 1;
    END;

    -- 発行済みの受付番号をすべて削除する。
    DELETE FROM dbo.Tickets;

    -- 採番のリセットと受付データの削除をまとめて確定する。
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    -- 途中で失敗したら、確定前の変更を元に戻す。
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;

-- 初期化できた場合は TicketCount = 0、LastIssuedNumber = 0 が目安。
SELECT
    (SELECT COUNT(*) FROM dbo.Tickets) AS TicketCount,
    LastIssuedNumber
FROM dbo.QueueState
WHERE QueueId = 1;
