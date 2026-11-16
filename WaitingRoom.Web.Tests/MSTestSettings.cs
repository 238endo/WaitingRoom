// このファイルには、WaitingRoom.Web.Tests 全体に適用する MSTest の設定を書く。
// assembly: は設定の対象がテストプロジェクト全体であることを示す。
// MethodLevel を指定すると、同じクラスにあるテストもメソッド単位で並行して実行できる。
// 実行順に依存しないよう、各テストは自分専用の受付データを用意する。
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
