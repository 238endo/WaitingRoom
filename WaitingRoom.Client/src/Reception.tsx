import * as React from 'react';
import { mountReact } from './mountReact';

function Reception() {
    // null は「この画面でまだ番号を発行していない」という意味。
    const [ticketNumber, setTicketNumber] = React.useState<string | null>(null);
    const [hasIssueError, setHasIssueError] = React.useState(false);

    // 待機中の受付番号を配列で持つ。最初は空の配列。
    const [waitingNumbers, setWaitingNumbers] = React.useState<string[]>([]);

    // null は、呼び出し中の番号がない状態。
    const [callingNumber, setCallingNumber] = React.useState<string | null>(null);
    const [hasCallError, setHasCallError] = React.useState(false);
    const [hasCompleteError, setHasCompleteError] = React.useState(false);

    const refreshWaitingList = React.useCallback(async () => {
        try {
            const response = await fetch('/api/queue');
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const queue: {
                callingNumber: string | null;
                waitingNumbers: string[];
            } = await response.json();

            // 同じ API の応答から、呼び出し中の番号と待機列を更新する。
            setCallingNumber(queue.callingNumber);
            setWaitingNumbers(queue.waitingNumbers);
        } catch (error) {
            console.error('[受付] 待機一覧の取得に失敗', error);
        }
    }, []);

    React.useEffect(() => {
        // 受付画面を開いたとき、すでに待機している番号も表示する。
        void refreshWaitingList();
    }, [refreshWaitingList]);

    async function issueTicket() {
        try {
            // POST で、サーバーに新しい受付番号の発行を依頼する。
            const response = await fetch('/api/tickets', { method: 'POST' });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            // API が返した番号を state に入れると、React が表示を更新する。
            const ticket: { number: string } = await response.json();
            setTicketNumber(ticket.number);
            setHasIssueError(false);
            // 発行した番号が待機列に入ったので、サーバーから一覧を取り直す。
            await refreshWaitingList();
        } catch (error) {
            setHasIssueError(true);
            console.error('[受付] 受付番号の発行に失敗', error);
        }
    }

    async function callNext() {
        try {
            // どの番号を呼ぶかはサーバーが決めるので、番号を渡さず POST する。
            const response = await fetch('/api/queue/call-next', { method: 'POST' });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            setHasCallError(false);
            await refreshWaitingList();
        } catch (error) {
            setHasCallError(true);
            console.error('[受付] 次の番号の呼び出しに失敗', error);
        }
    }

    async function completeTicket() {
    // 呼び出し中の番号があるときだけ、その番号を完了にする。
        if (callingNumber === null) {
            return;
        }

        try {
            const response = await fetch(`/api/tickets/${callingNumber}/complete`, {
                method: 'POST',
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            // 204 は応答の本文が空。受付状況は GET で取り直して表示する。
            setHasCompleteError(false);
            await refreshWaitingList();
        } catch (error) {
            setHasCompleteError(true);
            console.error('[受付] 案内完了に失敗', error);
        }
    }

    return (
        <div className="reception">
            <h1 className="reception-title">受付画面</h1>

            {/* 初版と同じクラス名で、受付画面用の CSS を使う。 */}
            <section className="issue">
                <button type="button" onClick={issueTicket}>受付番号を発行</button>
                {ticketNumber !== null && <p>受付番号 {ticketNumber}</p>}
                {hasIssueError && <p role="alert">受付番号を発行できませんでした</p>}
            </section>

            <section className="calling">
                <h2>呼び出し中</h2>
                <p>{callingNumber ?? '呼び出し中の番号はありません'}</p>
                <button type="button" onClick={completeTicket} disabled={callingNumber === null}>
                    案内完了
                </button>
                {hasCompleteError && <p role="alert">案内を完了できませんでした</p>}
            </section>

            <section className="waiting">
                <h2>待機中の番号</h2>
                <ul>
                    {/* 待機番号1件につき、一覧の行を1つ作る。 */}
                    {waitingNumbers.map(number => <li key={number}>{number}</li>)}
                </ul>
                <button
                    type="button"
                    onClick={callNext}
                    disabled={waitingNumbers.length === 0 || callingNumber !== null}
                >
                    次を呼ぶ
                </button>
                {hasCallError && <p role="alert">次の番号を呼び出せませんでした</p>}
            </section>
        </div>
    );
}

// Reception.cshtml の reception-root に、受付の部品を表示する。
mountReact('reception-root', <Reception />);
