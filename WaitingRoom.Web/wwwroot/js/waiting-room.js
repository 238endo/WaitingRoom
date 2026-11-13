// WaitingRoom.cshtml にある「更新」ボタンを id で探す。
const refreshButton = document.querySelector('#refresh-queue');
// 最新の受付状況を取得して、呼び出し番号と待機人数を表示する。
async function refreshWaitingRoom() {
    // method を省略すると GET になり、現在の受付状況を取得できる。
    const response = await fetch('/api/queue');
    const queue = await response.json();
    // 呼び出し番号
    const callingNumberElement = document.querySelector('#calling-number');
    // 案内メッセージ
    const callingMsgElement = document.querySelector('#calling-message');
    // 待機人数の表示
    const waitingCountElement = document.querySelector('#waiting-count');
    if (callingNumberElement) {
        // 呼び出し番号が null なら、番号の代わりにメッセージを表示する。
        callingNumberElement.textContent =
            queue.callingNumber ?? '呼び出し中の番号はありません';
    }
    if (callingMsgElement) {
        // 番号がなければ true で隠し、番号があれば false で表示する。
        callingMsgElement.hidden = queue.callingNumber === null;
    }
    if (waitingCountElement) {
        // 配列の length が待機人数。2件なら「2人」と表示する。
        waitingCountElement.textContent = `${queue.waitingNumbers.length}人`;
    }
}
// 関数を渡しておくと、「更新」を押したときに実行される。
if (refreshButton) {
    refreshButton.addEventListener('click', refreshWaitingRoom);
}
// ページを開いたときにも、同じ関数をその場で実行する。
await refreshWaitingRoom();
export {};
//# sourceMappingURL=waiting-room.js.map