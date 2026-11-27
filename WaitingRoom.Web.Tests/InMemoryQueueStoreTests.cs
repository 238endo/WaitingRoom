using WaitingRoom.Web.Stores;

namespace WaitingRoom.Web.Tests;

[TestClass]
public sealed class InMemoryQueueStoreTests : QueueStoreTests
{
    /// <summary>
    /// 受付データが空のメモリ保存用ストアを返します。
    /// </summary>
    protected override IQueueStore CreateStore() => new InMemoryQueueStore();
}
