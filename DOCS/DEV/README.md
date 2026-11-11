# DEV

Kiến trúc và quyết định kỹ thuật, ngoài những gì đã có ở [`CLAUDE.md`](../../CLAUDE.md) gốc repo — ghi chú
kiến trúc từng hệ thống, hợp đồng tích hợp, ràng buộc kỹ thuật từ GD. Owner: DEV.

> Mỗi `SYSTEM_*.md` ở đây là đích **Code** mà file hệ thống tương ứng trong [`GD/Systems/`](../GD/Systems/README.md)
> trỏ tới. Thiết kế *vì sao* ở GD, *làm thế nào* ở đây.
> Quy trình: [`DOCS/PROCESS.md`](../PROCESS.md).

Index of files in this folder — add a line per doc as it's written:

| File | Covers |
|---|---|
| [WORKFLOW.md](WORKFLOW.md) | ★ Quy ước làm việc: prefab, Addressables (`ConfigService` vs `AssetService`), con số cảm giác phải nằm trong config, input action map, test bằng Odin lúc chạy, nghiệm thu một mốc |

## Đặt tên file trong folder này

| Loại | Tên | Viết gì |
|---|---|---|
| Kiến trúc một hệ thống | `SYSTEM_<TÊN>.md` | Cấu trúc, file, API, tín hiệu, và **lý do của các quyết định kỹ thuật có thể muốn đổi** |
| Lộ trình kỹ thuật một phase | `PHASE<n>_PLAN.md` | Mốc, phạm vi hợp nhất backlog + GDD, điểm kỹ thuật phải chốt trước khi code |
| Việc kỹ thuật còn treo | `PHASE<n>_STATUS.md` | Trạng thái theo mốc, việc còn treo, thứ đã làm ngoài backlog |

★ Thứ **không** thuộc folder này: trạng thái từng đầu việc (ở [`CHECKLIST.md`](../CHECKLIST.md)), và quy
ước code chung của framework (ở `CLAUDE.md` gốc repo).
