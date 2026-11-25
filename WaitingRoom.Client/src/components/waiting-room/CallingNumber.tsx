import * as React from 'react';

// props は、親からこの部品に渡される値。C# のメソッドの引数に近い。
type CallingNumberProps = {
    callingNumber: string | null; // 例は "A001"。呼び出し中の番号がなければ null。
    isLoaded: boolean;             // 受付状況を取得できたら true。
};

function CallingNumber({ callingNumber, isLoaded }: CallingNumberProps) {
    return (
        <section className="waiting-number">
            <h2>呼び出し中の番号</h2>

            {/* 取得前は「未取得」。取得後は番号か、番号がないことを表示する。 */}
            <p>{isLoaded
                ? (callingNumber ?? '呼び出し中の番号はありません')
                : '未取得'}</p>

            {callingNumber !== null && <p>受付へお越しください</p>}
        </section>
    );
}

// 上で定義した関数を、このファイルの代表として公開する。
// WaitingRoom.tsx では { } を付けずに読み込む。
export default CallingNumber;
