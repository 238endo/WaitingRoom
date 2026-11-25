import * as React from 'react';

type WaitingCountProps = {
    count: number | null;
    onRefresh: () => Promise<void>;
};

function WaitingCount({ count, onRefresh }: WaitingCountProps) {
    return (
        <section className="waiting-people">
            <h2>待機人数</h2>
            {/* null は「未取得」。取得結果が 0 なら「0人」と表示する。 */}
            <span>{count === null ? '未取得' : `${count}人`}</span>

            {/* onRefresh は親から受け取った取得関数。「更新」を押したときに呼ぶ。 */}
            <button type="button" onClick={onRefresh}>更新</button>
        </section>
    );
}

export default WaitingCount;
