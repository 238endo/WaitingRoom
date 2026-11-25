import { defineConfig } from 'vite';

// Vite 公式の設定例と同じ書き方。defineConfig を使うと VS Code に設定の入力候補が出る。
// export default で、このファイルの設定を Vite に渡す。
export default defineConfig({
    // vite build を実行したときに使う設定。
    build: {
        // 「../」で WaitingRoom.Client の親フォルダーに戻り、隣の Web プロジェクトへ進む。
        // ASP.NET Core は wwwroot の下に生成された JavaScript をブラウザーに配信する。
        outDir: '../WaitingRoom.Web/wwwroot/js/react',

        // Rolldown は Vite がファイルをまとめるために使うツール。
        // ここでは画面ごとの入口と、生成する JavaScript の名前を指定する。
        rolldownOptions: {
            input: {
                // 左側が出力名のもと、右側が最初に読み込む .tsx のパス。
                'waiting-room': 'src/WaitingRoom.tsx',
                'reception': 'src/Reception.tsx',
            },
            output: {
                // [name] に input の左側の名前が入り、waiting-room.js と reception.js になる。
                // 各 .cshtml は /js/react/ の下にある、自分の画面用のファイルを読み込む。
                entryFileNames: '[name].js',
            },
        },
    },
});
