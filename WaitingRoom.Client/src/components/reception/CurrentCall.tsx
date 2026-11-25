import * as React from 'react';

type CurrentCallProps = {
    callingNumber: string | null;
    hasCompleteError: boolean;
    onComplete: () => Promise<void>;
};

function CurrentCall({ callingNumber, hasCompleteError, onComplete }: CurrentCallProps) {
    return (
        <section className="calling">
            <h2>呼び出し中</h2>
            <p>{callingNumber ?? '呼び出し中の番号はありません'}</p>

            {/* 呼び出し中の番号がない間は、案内完了ボタンを無効にする。 */}
            <button type="button" onClick={onComplete} disabled={callingNumber === null}>
                案内完了
            </button>
            {hasCompleteError && <p role="alert">案内を完了できませんでした</p>}
        </section>
    );
}

export default CurrentCall;
