# React メモ

## React とは？

React は、JavaScript や TypeScript で、ブラウザーに表示する画面を作るためのライブラリ。画面を「呼び出し番号の表示欄」や「待機人数の表示欄」などの部品に分けて作れる。この部品を**コンポーネント**と呼ぶ。

React では、画面が覚えておく値を**状態（state）**と呼ぶ。たとえば、呼び出し中の番号を `A001` から `A002` に更新すると、React が新しい値を使って表示を更新する。今の TypeScript では、表示欄を探して `textContent` に新しい番号を入れている。React では「ここに呼び出し番号を表示する」と書き、番号を状態として更新する。 [React 公式ドキュメントのクイックスタート](https://ja.react.dev/learn#updating-the-screen)

## 待合室アプリでは何に使う？

第2版の FR-06 で、待合室画面の呼び出し番号と待機人数の表示に使う。

1. TypeScript で、既存の C# の受付状況 API を定期的に呼ぶ。
2. 取得した呼び出し番号と待機人数を、React の状態に入れる。
3. React が、受け取った値に合わせて画面の表示を更新する。

## React を使わなくても、別の画面を自動更新できる？

できる。たとえば、待機人数が0人の状態で、受付画面と待合室画面を別々のタブで開いておく。

1. 受付画面で「受付番号を発行」を押す。`POST /api/tickets` が成功すると、サーバーの待機列に1件追加される。
2. 待合室画面の TypeScript がタイマーで `GET /api/queue` を定期的に呼び、同じサーバーから最新の待機列を取得する。
3. 次の取得で待機列が1件になっていれば、TypeScript が表示欄の `textContent` を書き換え、「待機人数 1人」と表示する。

React を使う場合も、最新データを取得する流れは同じ。取得した人数を React の状態に入れると、React が表示を更新する。

## Vite（ヴィート）とは？

Vite は、画面のコードをブラウザーで読み込める形にする**ビルドツール**。今回の待合室画面では、`WaitingRoom.tsx` を JavaScript に変換し、React など実行に必要なコードとまとめる。生成した `waiting-room.js` を `WaitingRoom.cshtml` から読み込む。 [Vite 公式ガイド](https://vite.dev/guide/)

- **Vite** は、開発時や公開前に、ブラウザーで使う JavaScript を作る。
- **React** は、ブラウザーでその JavaScript が実行されたときに、待機人数などを表示する。

React と組み合わせるビルドツールは選べる。今回は、待合室の画面コードと React をまとめて既存の MVC 画面から読み込めるよう、Vite を使う設計にしている。 [React 公式｜基本からReactアプリを作る](https://react.dev/learn/build-a-react-app-from-scratch)
