import * as React from 'react';
import { mountReact } from './mountReact';

/* MEMO
ts→tsxにあたって、ファイルの先頭が大文字の理由は、コンポーネントが大文字必須であるため、そのコンポーネント名とファイル名で統一させるため。
今回のプロジェクトから、tsx→tsファイルを使うため、tscコマンドを使ってビルドは行いません。

*/

function WaitingRoom() {
    // null は「まだ取得できていない」。0 は「取得した結果、待機者が0人」。
    const [waitingCount, setWaitingCount] = React.useState<number | null>(null);
    const [hasFetchError, setHasFetchError] = React.useState(false);

    // 呼び出し中なら 'A001' などの文字列。番号がないときは null。
    const [callingNumber, setCallingNumber] = React.useState<string | null>(null);
    
    // 今、受付状況を取りに行っているかを覚える。
    const isFetching = React.useRef(false);

    // C# の Func<Task> のように、あとで呼ぶ非同期の取得関数を変数に持っておく。
    // useCallback は、人数が変わって再表示されても、同じ関数を使い回せるようにする。
    // 覚えているのは関数。呼ぶたびに API から最新の人数を取り直す。
    const refreshWaitingCount = React.useCallback(async () => {
        
        // すでに通信中なら、画面には進行中の通信の結果を反映する。
        if (isFetching.current) {
            return;
        }

        isFetching.current = true;
        try {
            const response = await fetch('/api/queue');

            // 500などのエラー応答も、通信の失敗と同じ catch に進める。
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const queue: {
                callingNumber: string | null;
                waitingNumbers: string[];
            } = await response.json();

            // 取得に成功したら、番号と人数を更新してエラー表示を消す。
            setCallingNumber(queue.callingNumber);
            setWaitingCount(queue.waitingNumbers.length);
            setHasFetchError(false);
        } catch (error) {
            // エラー表示だけを更新するので、前回の番号と人数は画面に残る。
            setHasFetchError(true);
            console.error('[待合室] 受付状況の取得に失敗', error);
        } finally {
            // 成功しても失敗しても、次の更新で通信できるように戻す。
            isFetching.current = false;
        }
    }, []);

    React.useEffect(() => {
        // WinForms の Shown に近い。画面を最初に表示したあと、すぐに1回取得する。
        console.log('[待合室] useEffect: 初回取得とタイマーを開始');
        void refreshWaitingCount();

        // WinForms の Timer.Tick に近い。5,000ミリ秒（5秒）ごとに呼ばれる。
        const timerId = window.setInterval(() => {
            console.log('[待合室] タイマー: 5秒ごとの取得');
            void refreshWaitingCount();
        }, 5000);

        // 待合室画面を離れたときなどに、このタイマーを止める。
        return () => { window.clearInterval(timerId); }
    }, [refreshWaitingCount]);

    return (
        <>
            {/* 待合室画面の見出し。初版と同じクラス名で既存の CSS を使う。 */}
            <h1 className="waiting-room-title">待合室画面</h1>

            {/* 取得に失敗したときだけ、エラーを表示する。 */}
            {hasFetchError && (
                <p role="alert">最新の受付状況を取得できませんでした</p>
            )}

            <section className="waiting-number">
                <h2>呼び出し中の番号</h2>
                {/* 取得前は「未取得」。取得済みで番号がなければ、その旨を表示する。 */}
                <p>{waitingCount === null
                    ? '未取得'
                    : (callingNumber ?? '呼び出し中の番号はありません')}</p>

                {callingNumber !== null && <p>受付へお越しください</p>}
            </section>

            <section className="waiting-people">
                <h2>待機人数</h2>
                {/* null は「未取得」。取得結果が 0 なら「0人」と表示する。 */}
                <span>{waitingCount === null ? '未取得' : `${waitingCount}人`}</span>
                <button type="button" onClick={refreshWaitingCount}>更新</button>
            </section>
        </>
    );
}

// WaitingRoom.cshtml の waiting-room-root に、待合室の部品を表示する。
mountReact('waiting-room-root', <WaitingRoom />);
