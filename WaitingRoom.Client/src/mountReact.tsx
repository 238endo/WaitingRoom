import type { ReactNode } from 'react';
import { createRoot } from 'react-dom/client';

// マウントとは、React で作った画面の部品を HTML に表示すること。
// containerId は表示先の id。content は <WaitingRoom /> など、そこに表示する内容。
export function mountReact(containerId: string, content: ReactNode): void {
    // 例として 'waiting-room-root' を渡すと、HTML から同じ id の付いた要素を探す。
    const container = document.getElementById(containerId);

    // 表示先が見つかったときだけ、その要素の中に React の内容を表示する。
    if (container) {
        const root = createRoot(container);
        root.render(content);
    }
}
