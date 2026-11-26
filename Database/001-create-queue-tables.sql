CREATE TABLE [dbo].[Tickets] (
    [TicketSequence] INT           NOT NULL,
    [IssuedAtUtc]    DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [Status]         TINYINT       NOT NULL,
    PRIMARY KEY CLUSTERED ([TicketSequence] ASC),
    CONSTRAINT [CK_Tickets_Status] CHECK ([Status]=(2) OR [Status]=(1) OR [Status]=(0))
);

CREATE TABLE [dbo].[QueueState]
(
    [QueueId] INT NOT NULL PRIMARY KEY,
    [LastIssuedNumber] INT NOT NULL,
    CONSTRAINT [CK_QueueState_LastIssuedNumber] CHECK ([LastIssuedNumber] >= 0)
)

-- 窓口1の採番を0から始める。
INSERT INTO dbo.QueueState (QueueId, LastIssuedNumber)
VALUES (1, 0);
