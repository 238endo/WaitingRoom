import * as React from 'react';

// props は、親の Reception から受け取る表示用の値と操作用の関数。
type TicketIssueProps = {
    ticketNumber: string | null;
    hasIssueError: boolean;
    isIssuing: boolean;
    isQueueLoading: boolean;
    onIssue: () => Promise<void>;
};

function TicketIssue({ ticketNumber, hasIssueError, isIssuing, isQueueLoading, onIssue }: TicketIssueProps) {
    return (
        <section className="issue">
            {/* onIssue は親の発行処理。ボタンを押したときに React が呼ぶ。 */}
            <button type="button" onClick={onIssue} disabled={isQueueLoading || isIssuing}>
                {isIssuing ? '発行中…' : '受付番号を発行'}
            </button>
            {ticketNumber !== null && <p>受付番号 {ticketNumber}</p>}
            {hasIssueError && (
                <p role="alert">発行結果を確認できませんでした。再発行の前に待機一覧を確認してください。</p>
            )}
        </section>
    );
}

export default TicketIssue;
