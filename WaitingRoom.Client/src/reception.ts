// MEMO: 
// 1. querySelector() は、指定した条件に一致する最初のHTML要素を探す。
// 2. id は、ページ内の1つの要素を指定するための名前です。
// 3. class は、同じ見た目にしたいボタンなど、複数の要素で共有できる名前です。
// 4. idを探すときは"#○○"、classを探すときは".○○"です。
// 5. export {};` は「このファイルはモジュールとして扱う。外に公開するものはゼロ」
// 6. ES(ECMAScript)モジュールは、import・exportを使う仕組みです。
// 7. <ul> は 箇条書きのリストを作る HTML 要素です。中に <li>（リストの1項目）を書きます。
// 8. Promise<>は"C#のTaskに相当する"と考えて大丈夫です。例: Task<string> = Promise<string> || Task = Promise<void>

// 受付番号発行ボタン
const issueButton = document.querySelector('#issue-ticket');
// 受付番号を表示する段落要素(<p>タグ)
const ticketNumber = document.querySelector('#ticket-number');
// Reception.cshtml に追加した <ul id="waiting-numbers"> を探す。
const waitingList = document.querySelector('#waiting-numbers');
// HTMLButtonElement を指定すると、ボタンの disabled プロパティを使える。
const callNextButton = document.querySelector<HTMLButtonElement>('#call-next');

// 最新の待機番号を取得し、画面の一覧を描き直す。
async function refreshWaitingList(): Promise<void> {
    // サーバーから最新の受付状況を取得する。
    const response = await fetch('/api/queue');
    const queue: {
        callingNumber: string | null;
        waitingNumbers: string[];
    } = await response.json();

    // 呼び出し中の番号を表示し、次の番号を呼べるかを反映する。
    const callingNumberElement = document.querySelector('#calling-number');
    if (callingNumberElement) {
        callingNumberElement.textContent =
            queue.callingNumber ?? '呼び出し中の番号はありません';
    }

    if (callNextButton) {
        callNextButton.disabled =
            queue.waitingNumbers.length === 0 || queue.callingNumber !== null;
    }

    // 以前の項目を取り除き、取得した待機番号で一覧を作り直す。
    if (waitingList) {
        waitingList.replaceChildren();
        for (const waitingNumber of queue.waitingNumbers) {
            const item = document.createElement('li');
            item.textContent = waitingNumber;
            waitingList.append(item);
        }
    }
}

// 「受付発行」ボタンと「受付番号を表示する」段落要素が見つかったときに、クリック処理を登録する。
if(issueButton && ticketNumber) {
    
    issueButton.addEventListener('click', async () => {
    try {
        const response = await fetch('/api/tickets', { method: 'POST' });
        console.log(response.status);

        // response.ok は、HTTP ステータスが 200〜299 のとき true になる。
        if (!response.ok) {
            console.error('番号発行に失敗しました。HTTP ステータス:', response.status);

            // return で今回のクリック処理を終え、下の JSON 読み取りに進まない。
            return;
        }

        // 番号発行 API の応答には number があり、その値は "A001" のような文字列。
        // 「オブジェクト型の型注釈を付けた。」と言います。
        const ticket: { number: string } = await response.json();

        // API が返したオブジェクトの中身をコンソールで確認する。
        console.log(ticket);

        // fetch で別の API を呼ぶと、返ってきた JSON のキーに応じて ticket のプロパティ名も変わる。
        // たとえば GET /api/queue の応答には callingNumber と waitingNumbers が入る。
        // TypeScript で型注釈を付ける場合も、その API の応答の形に合わせる。
        ticketNumber.textContent = ticket.number;

        // 番号を発行した後で待機一覧を更新し、描画が終わるのを待つ。
        await refreshWaitingList();
    }catch(error){
        // fetch が通信エラーになった場合などは、ここで原因をコンソールに出す。
        console.error('番号発行の処理中にエラーが起きました。', error);
    }
});
}

// 「次を呼ぶ」ボタンが見つかったときに、クリック処理を登録する。
if (callNextButton) {
    callNextButton.addEventListener('click', async () => {
        // 待機列の先頭を呼び出すように依頼し、サーバーの応答を待つ。
        const response = await fetch('/api/queue/call-next', { method: 'POST' });

        // 呼び出しが成立しなかったときは、ステータスを出して処理を終える。
        if (!response.ok) {
            console.error('呼び出しに失敗しました。HTTP ステータス:', response.status);
            return;
        }

        // 呼び出しに成功した後で、呼び出し中の番号と待機一覧を更新する。
        await refreshWaitingList();
    });
}
