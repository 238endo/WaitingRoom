import * as React from 'react';

type WaitingCountProps = {
    count: number | null;
    isLoading: boolean;
    hasFetchError: boolean;
    onRefresh: () => Promise<void>;
};

function WaitingCount({ count, isLoading, hasFetchError, onRefresh }: WaitingCountProps) {
    return (
        <section className="waiting-people">
            <h2>待機人数</h2>
            {/* null は「未取得」。取得結果が 0 なら「0人」と表示する。 */}
            <span>{count === null ? '未取得' : `${count}人`}</span>

            {/* onRefresh は親から受け取った取得関数。「更新」を押したときに呼ぶ。 */}
            <button type="button" onClick={onRefresh} disabled={isLoading}>更新</button>
            {count === null && !hasFetchError && <p role="status">受付状況を取得中…</p>}
            {hasFetchError && <p role="alert">最新の受付状況を取得できませんでした</p>}
        </section>
    );
}

export default WaitingCount;
