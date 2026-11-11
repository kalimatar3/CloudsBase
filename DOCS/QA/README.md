# QA

Tiêu chí chất lượng đầu ra theo **tính năng**. Owner: QA.

> Mỗi file ở đây là đích **Chất lượng đầu ra** mà file hệ thống trong
> [`GD/Systems/`](../GD/Systems/README.md) trỏ tới. Quy trình: [`DOCS/PROCESS.md`](../PROCESS.md).

Đặt tên `QA_<TÊN>.md`, một file một tính năng. Tính năng nào chưa có file riêng thì file hệ thống trỏ vào
**nhóm testcase tương ứng** của `Phases/PHASE<n>_ACCEPTANCE.md`.

Nghiệm thu **theo phase** không nằm ở đây mà ở [`Phases/`](../Phases/README.md) — cùng chỗ với phạm vi của
phase đó.

| File | Covers |
|---|---|
| _(chưa có)_ | |

## Viết gì trong một file QA tính năng

1. **Xong thì trông thế nào** — mô tả trạng thái đạt, không phải danh sách bước
2. **Testcase tay** — bấm gì, thấy gì
3. **Chỗ xem số liệu** — GameObject nào trong Inspector, hoặc panel debug nào
4. **Chỗ đã biết là dễ sai** — lỗi từng xảy ra, để lần sau kiểm lại

★ Mục 4 là mục đáng giá nhất và cũng là mục hay bị bỏ. Một lỗi đã sửa mà không ai ghi lại thì lần sau
không ai nghĩ tới việc kiểm nó.
