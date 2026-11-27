import * as React from 'react';
import CurrentCall from './components/reception/CurrentCall';
import TicketIssue from './components/reception/TicketIssue';
import WaitingList from './components/reception/WaitingList';
import { mountReact } from './mountReact';

type QueueSnapshot = {
    callingNumber: string | null;
    waitingNumbers: string[];
};

function Reception() {
    // null は「この画面でまだ番号を発行していない」という意味。
    const [ticketNumber, setTicketNumber] = React.useState<string | null>(null);
    const [hasIssueError, setHasIssueError] = React.useState(false);
    const [isIssuing, setIsIssuing] = React.useState(false);

    // null は未取得。取得後の空の待機列とは区別する。
    const [queue, setQueue] = React.useState<QueueSnapshot | null>(null);
    const [queueStatus, setQueueStatus] = React.useState<'loading' | 'loaded' | 'error'>('loading');
    const latestQueueRequest = React.useRef(0);
    const [hasCallError, setHasCallError] = React.useState(false);
    const [hasCompleteError, setHasCompleteError] = React.useState(false);
    const callingNumber = queue?.callingNumber ?? null;
    const isQueueLoading = queueStatus === 'loading';

    const refreshWaitingList = React.useCallback(async () => {
        // 取得が重なったときは、最後に開始した通信の結果を表示する。
        const requestId = ++latestQueueRequest.current;
        setQueueStatus('loading');
        try {
            const response = await fetch('/api/queue');
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const snapshot: QueueSnapshot = await response.json();
            if (requestId === latestQueueRequest.current) {
                setQueue(snapshot);
                setQueueStatus('loaded');
            }
        } catch (error) {
            if (requestId === latestQueueRequest.current) {
                setQueueStatus('error');
            }
            console.error('[受付] 待機一覧の取得に失敗', error);
        }
    }, []);

    React.useEffect(() => {
        // 受付画面を開いたとき、すでに待機している番号も表示する。
        void refreshWaitingList();
    }, [refreshWaitingList]);

    async function issueTicket() {
        if (isQueueLoading || isIssuing) {
            return;
        }

        setIsIssuing(true);
        setHasIssueError(false);
        try {
            // POST で、サーバーに新しい受付番号の発行を依頼する。
            const response = await fetch('/api/tickets', { method: 'POST' });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            // API が返した番号を state に入れると、React が表示を更新する。
            const ticket: { number: string } = await response.json();
            setTicketNumber(ticket.number);
            // 発行した番号が待機列に入ったので、サーバーから一覧を取り直す。
            void refreshWaitingList();
        } catch (error) {
            setHasIssueError(true);
            console.error('[受付] 受付番号の発行に失敗', error);
            // 応答が届かなくても発行済みの場合があるので、一覧を読み直す。
            void refreshWaitingList();
        } finally {
            setIsIssuing(false);
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

            {/* 表示に使う値と、ボタンを押したときに呼ぶ関数を各部品へ渡す。 */}
            <TicketIssue
                ticketNumber={ticketNumber}
                hasIssueError={hasIssueError}
                isIssuing={isIssuing}
                isQueueLoading={isQueueLoading}
                onIssue={issueTicket}
            />
            {isQueueLoading && <p role="status">受付状況を取得中…</p>}
            {queueStatus === 'error' && (
                <div>
                    <button type="button" onClick={refreshWaitingList}>再取得</button>
                    <p role="alert">{queue ? '受付状況を更新できませんでした。' : '受付状況を読み込めませんでした。'}</p>
                </div>
            )}
            {queue && (
                <>
                    <CurrentCall
                        callingNumber={callingNumber}
                        hasCompleteError={hasCompleteError}
                        onComplete={completeTicket}
                    />
                    <WaitingList
                        waitingNumbers={queue.waitingNumbers}
                        canCallNext={queue.waitingNumbers.length > 0 && callingNumber === null}
                        hasCallError={hasCallError}
                        onCallNext={callNext}
                    />
                </>
            )}
        </div>
    );
}

// Reception.cshtml の reception-root に、受付の部品を表示する。
mountReact('reception-root', <Reception />);
