import * as React from 'react';

type WaitingListProps = {
    waitingNumbers: string[];
    canCallNext: boolean;
    hasCallError: boolean;
    onCallNext: () => Promise<void>;
};

function WaitingList({
    waitingNumbers,
    canCallNext,
    hasCallError,
    onCallNext,
}: WaitingListProps) {
    return (
        <section className="waiting">
            <h2>待機中の番号</h2>
            <ul>
                {/* 待機番号1件につき一覧の行を1つ作り、受付番号を各行の目印にする。 */}
                {waitingNumbers.map(number => <li key={number}>{number}</li>)}
            </ul>

            {/* 親が渡した canCallNext が false のとき、ボタンを無効にする。 */}
            <button type="button" onClick={onCallNext} disabled={!canCallNext}>
                次を呼ぶ
            </button>
            {hasCallError && <p role="alert">次の番号を呼び出せませんでした</p>}
        </section>
    );
}

export default WaitingList;
