// MEMO: 
// 1. querySelector() は、指定した条件に一致する最初のHTML要素を探す。
// 2. id は、ページ内の1つの要素を指定するための名前です。
// 3. class は、同じ見た目にしたいボタンなど、複数の要素で共有できる名前です。
// 4. idを探すときは"#○○"、classを探すときは".○○"です。
// 5. export {};` は「このファイルはモジュールとして扱う。外に公開するものはゼロ」
// 6. ES(ECMAScript)モジュールは、import・exportを使う仕組みです。

const issueButton = document.querySelector('#issue-ticket');
const ticketNumber = document.querySelector('#ticket-number');

if(issueButton && ticketNumber) {
    issueButton.addEventListener('click',() => {
        ticketNumber.textContent = 'A001';
    });
}