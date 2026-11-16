# Azure SQL Database メモ

## Azure SQL Database とは？

Azure 上で使える、Microsoft が運用するデータベースサービス。SQL Server と同じ系統の技術を使っていて、どちらでもテーブルを作り、SQL でデータを登録・検索・更新できる。 [Microsoft Learn｜Azure SQL Database の概要](https://learn.microsoft.com/ja-jp/azure/azure-sql/database/sql-database-paas-overview?view=azuresql)

## IIS と SQL Server を同じ PC に置く場合との違い

IIS は Web アプリや API を公開する役割で、SQL Server はデータを保存する役割を持つ。同じ PC で動かせるので、手元だけで開発やテストをするなら、この構成で十分なこともある。

| 比較する点 | 同じ PC に IIS と SQL Server | IIS はそのままで、保存先を Azure SQL Database にする |
|---|---|---|
| 更新・バックアップ | 自分たちで SQL Server の更新やバックアップを設定・管理する。 | Microsoft がデータベース基盤の更新と自動バックアップを担う。自分たちは接続権限やデータの管理を行う。 |
| IIS の PC が止まったら | Web アプリとデータベースの両方に接続できなくなる。 | IIS の Web アプリは止まる。データベースは別の場所で稼働を続けられる。 |
| 接続方法 | Web アプリから同じ PC の SQL Server に接続する。 | Web アプリからネットワーク経由で Azure に接続する。Azure 側で接続と認証の設定が必要になる。 |

Azure SQL Database を選ぶメリットは、データベース用の PC や OS の保守を自分たちで行う負担が減ること。利用にあたっては、料金やネットワーク接続も考慮する。 [Microsoft Learn｜Azure SQL Database の概要](https://learn.microsoft.com/ja-jp/azure/azure-sql/database/sql-database-paas-overview?view=azuresql)

## 待合室アプリでは何に使う？

第2版の FR-07 では、App Service 上の Web アプリが受付番号・発行日時・受付状況を Azure SQL Database に保存する。Web アプリを再起動しても、保存した待機列を読み直して受付を続けられる。FR-08 では、日次集計を担当する Azure Functions も同じデータベースから発行履歴を読む。

再起動後もデータを残すこと自体は、同じ PC の SQL Server でもできる。今回 Azure SQL Database を使う理由は、公開済みの Web アプリをクラウド上のデータベースにつなぎ、接続や運用を学ぶため。
