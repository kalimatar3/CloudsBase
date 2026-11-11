# CloudsBase

Framework Unity (URP) dùng lại được giữa các game, cộng với **bộ quy trình và tài liệu** đi kèm. Dự án mới
clone repo này về là có sẵn: framework ở `Assets/Clouds/`, quy ước code ở [`CLAUDE.md`](CLAUDE.md), và quy
trình làm việc ở [`DOCS/`](DOCS/README.md).

**`DOCS/` ở đây là khung rỗng.** Mọi file trong đó chỉ chứa **hướng dẫn viết gì vào đâu** — không có thiết
kế, không có phase, không có số của một game cụ thể. Điền dần khi dự án chạy.

## Bắt đầu một dự án mới từ đây

| Bước | Làm gì |
|---|---|
| 1 | Clone repo, đổi tên thư mục và product name trong `ProjectSettings` |
| 2 | Điền [`DOCS/GD/PROJECT.md`](DOCS/GD/PROJECT.md) — nhất là **mục 2, danh sách ràng buộc BẮT BUỘC**. Đây là thứ chặn agent làm sai ngay từ dòng backlog đầu tiên |
| 3 | Điền [`DOCS/GD/GDD.md`](DOCS/GD/GDD.md) mục 01 (pitch, nền tảng), rồi brief từng hệ thống vào mục 02–04 |
| 4 | Tạo `DOCS/Phases/PHASE1.md` theo cấu trúc 3 khối ở [`Phases/README.md`](DOCS/Phases/README.md) |
| 5 | Điền việc vào [`DOCS/BACKLOG.md`](DOCS/BACKLOG.md) và chạy lần lượt từ trên xuống |

## Bắt đầu đọc từ đâu

| Bạn là | Đọc |
|---|---|
| **Sắp làm một tính năng** | ★ [`DOCS/PROCESS.md`](DOCS/PROCESS.md) — quy trình 5 bước. Không implement thứ chưa mô tả trong GDD |
| Người mới / AI agent | [`DOCS/GD/PROJECT.md`](DOCS/GD/PROJECT.md) — tóm tắt một màn hình + ràng buộc BẮT BUỘC |
| Sắp viết code | [`CLAUDE.md`](CLAUDE.md) (framework + quy ước) rồi [`DOCS/DEV/WORKFLOW.md`](DOCS/DEV/WORKFLOW.md) |
| Chỉ mục toàn bộ tài liệu | [`DOCS/README.md`](DOCS/README.md) |

## Chạy game

Luôn bấm Play từ `Assets/Game.Scenes/Bootstrap.unity` — nó nạp config rồi tự sang scene sau. Play thẳng từ
scene gameplay thì thứ gì cần config sẽ thiếu. Chi tiết và lý do:
[`DOCS/DEV/WORKFLOW.md`](DOCS/DEV/WORKFLOW.md) mục 1.

## Framework có gì

Chi tiết từng hệ thống ở [`CLAUDE.md`](CLAUDE.md). Tóm tắt:

| Mảng | Thứ chính |
|---|---|
| Vòng đời component | `MyBehaviour`, `Singleton<T>`, một điểm vào `Bootstrap` |
| Messaging | `SignalBus` — pub/sub toàn cục và theo phạm vi |
| UI navigation | `ScreenService` / `PopupService` / `ActivityService`, stack + lịch sử, `UIRootManager` |
| Animation UI | `UIAnimationContainer`, `UIAnimationSequencer`, factory DOTween / PrimeTween |
| Dữ liệu | `LoadSaveService`, `Repository<T>`, `DataService` |
| Asset | `ConfigService` (theo kiểu, nạp sẵn) · `AssetService` (theo key, nạp khi cần) · `PoolService` |
| Máy trạng thái | `State`, `Statemachine<T>` |
| Khác | Physics helper, Timeline track, Editor tool |

## Skill cho agent

`.claude/skills/` có hai skill dùng được ngay:

| Skill | Dùng khi |
|---|---|
| `gd-feature` | Trước khi implement bất cứ tính năng gameplay nào — cổng kiểm GDD đã mô tả chưa |
| `backlog` | Ghi việc phát sinh vào `BACKLOG.md`, chuyển dòng đã xong sang `CHECKLIST.md` |
