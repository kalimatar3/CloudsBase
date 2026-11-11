# GD (Game Design)

Thiết kế game: ý định thiết kế, đặc tả tính năng, hệ thống, cân bằng. Owner: GD.

> Quy trình thêm hoặc sửa một tính năng: [`DOCS/PROCESS.md`](../PROCESS.md).

| File | Covers |
|---|---|
| [GDD.md](GDD.md) | ★ **GDD chính thức — bản TỔNG QUAN.** Mỗi hệ thống một brief 3–5 dòng + link xuống `Systems/`. Pitch, nền tảng, phong cách art, bảng phase, nhật ký cập nhật |
| [Systems/](Systems/README.md) | ★ **Chi tiết từng hệ thống** — một hệ thống một file. Sửa thiết kế một tính năng thì mở đúng file ở đây |
| [PROJECT.md](PROJECT.md) | Brief cho agent — tóm tắt một màn hình, ràng buộc BẮT BUỘC, danh sách [TBD], điều hướng. Không chứa nội dung thiết kế |

## Sửa ở đâu

| Việc | File |
|---|---|
| Thêm một hệ thống mới | Brief vào `GDD.md` mục 02/03/04 **và** file mới trong `Systems/` |
| Đổi chi tiết một hệ thống đã có | Chỉ `Systems/<TÊN>.md` |
| Đổi bố cục bản đồ | `../Phases/PHASE<n>_MAP.md` — **không** ở folder này |
| Đổi phạm vi một phase | [`../Phases/PHASE<n>.md`](../Phases/README.md) |

★ Cả ba file ở đây hiện là **khung rỗng**. Điền theo thứ tự: `PROJECT.md` (ràng buộc) → `GDD.md` mục 01
(pitch) → brief vào mục 02–04 → file trong `Systems/`.
