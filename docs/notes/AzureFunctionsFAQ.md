# Azure Functions FAQ（日次集計）

待合室アプリの第2版では、前日に発行した受付番号の件数を毎日集計する。 [要件定義の FR-08](../要件定義.md#第2版の追加変更要件)

受付データの保存先（FR-07）は **Azure SQL Database**。Azure Functions の日次集計は、ここに保存した発行履歴を使う。

## Q. DateTime で時刻を見て、午前0時になったら実行すればよいのでは？

その方法でも実装できる。たとえば `DateTime.Now` で現在時刻を読み、ASP.NET Core のバックグラウンド処理にタイマーを置いて、毎日集計を実行できる。時刻を読む処理に加えて、決まった時間に集計を呼び出す仕組みと、同じ日の集計を重複して保存しない仕組みが必要になる。 [Microsoft Learn のバックグラウンド処理](https://learn.microsoft.com/ja-jp/aspnet/core/fundamentals/hosted-services)

## Q. バックエンド API は、ふだん起動しているのに止まることがある？

ある。たとえば App Service を手動で再起動すると、Web アプリの中のタイマーも一緒に止まる。また、App Service は Always On が無効だと、一定時間アクセスがないときにアプリをメモリから降ろす。再びアクセスできる状態と、Web アプリの処理が動き続けている状態には違いがある。 [Microsoft Learn の App Service 設定](https://learn.microsoft.com/ja-jp/azure/app-service/configure-common)

## Q. このアプリで Azure Functions を使うメリットは何？

集計を Web アプリの稼働状態から切り離せる。タイマートリガーに実行時刻を設定すると、Function App がその予定に沿って集計を起動する。受付の Web アプリが再起動中でも、Function App が実行可能でデータベースに接続できれば、集計を実行できる。集計の実行結果も Function App 側で確認できる。 [Microsoft Learn のタイマートリガー](https://learn.microsoft.com/ja-jp/azure/azure-functions/functions-bindings-timer)

## Q. 「別に動かす」とは、何を別にすること？

Visual Studio では、既存のソリューションに C# の Azure Functions プロジェクトを追加する。Azure では、受付画面と API は App Service、集計は別の Function App に公開する。受付アプリが保存した発行履歴を、集計処理が同じ Azure SQL Database から読み取る。

## Q. 日次集計のテストは、日付が変わるまで待つ必要がある？

日付を固定したテストデータと、テスト用の短い実行間隔を使えば、その場で確認できる。

1. **集計する件数を確かめる。** MSTest で集計対象の日付を指定して処理を呼ぶ。対象日に2件、翌日に1件の発行履歴を用意し、集計結果が2件になることを確認する。
2. **タイマーから起動して保存できるか確かめる。** テスト環境でタイマーを1分おきに設定する。Function App の実行ログで起動を確認し、テスト用の Azure SQL Database に対象日と集計件数が保存されていることを確かめる。
3. **公開先の実行設定を確かめる。** 毎日の実行時刻とタイムゾーンを確認し、公開後の初回実行をログで確認する。

## Q. 定刻に実行できなかったときや、失敗したときは？

集計に失敗した場合は、まず Function App の実行ログを確認する。やり直すときは、同じ日付の集計を更新する形にすると、同じ集計が2件できるのを防げる。今回の学習では、時刻で起動する処理を別に公開し、実行と結果を確認するところから始める。
