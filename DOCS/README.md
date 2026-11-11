# DOCS

Tài liệu của **game** đang dựng trên framework Clouds — thiết kế, phase, kỹ thuật, QA, art. Thứ không suy
ra được từ code. Quy ước framework và code nằm ở [`CLAUDE.md`](../CLAUDE.md) gốc repo, không ở đây.

> ★ **Trước khi làm bất cứ tính năng nào, đọc [`PROCESS.md`](PROCESS.md).** Quy tắc một câu: *không
> implement thứ chưa được mô tả trong GDD.* Chưa có mô tả thì viết mô tả trước.

**Thư mục này là khung, chưa có nội dung.** Mọi file ở đây hiện chỉ chứa **hướng dẫn viết gì vào đâu** —
không có thiết kế, không có phase, không có số của một game cụ thể. Dự án mới điền dần: `BACKLOG.md` nhận
việc, `CHECKLIST.md` ghi tiến độ, `GD/` và `DEV/` dày lên theo từng hệ thống.

## Vào đâu

| Bạn cần | Đọc |
|---|---|
| **Quy trình làm việc** | [PROCESS.md](PROCESS.md) — 5 bước, luật chạy backlog, nhà của từng loại thông tin |
| Tóm tắt 1 màn hình + ràng buộc BẮT BUỘC | [GD/PROJECT.md](GD/PROJECT.md) |
| **Thiết kế game (tổng quan)** | [GD/GDD.md](GD/GDD.md) — bản đồ, mỗi hệ thống một brief + link |
| **Thiết kế một hệ thống (chi tiết)** | [GD/Systems/](GD/Systems/README.md) — một hệ thống một file |
| **Tiến độ: phase này xong tới đâu** | [CHECKLIST.md](CHECKLIST.md) — 3 cột |
| **Điền việc mới** | [BACKLOG.md](BACKLOG.md) — hàng đợi đầu vào |
| Tiêu chí chất lượng theo tính năng | [QA/](QA/README.md) |
| Phạm vi & nghiệm thu từng phase | [Phases/](Phases/README.md) |
| Kiến trúc & cách code | [DEV/](DEV/README.md) |
| Đặc tả art | [Art/](Art/README.md) |

## Cấu trúc

```
DOCS/
├── PROCESS.md     ★ quy trình — đọc trước khi code
├── CHECKLIST.md     tiến độ, 3 cột (đầu việc · trạng thái · note)
├── BACKLOG.md       hàng đợi đầu vào — người chủ dự án điền, agent chạy lần lượt
├── GD/              thiết kế game
│   ├── GDD.md       ★ TỔNG QUAN — brief mỗi hệ thống + link xuống
│   ├── Systems/     chi tiết từng hệ thống, một file một hệ thống
│   └── PROJECT.md   brief + ràng buộc cho agent
├── Phases/          mọi thứ thuộc một phase cụ thể
│   ├── PHASE<n>.md             phạm vi & nghiệm thu
│   ├── PHASE<n>_MAP.md         bản đồ, góc nhìn, bố cục
│   └── PHASE<n>_ACCEPTANCE.md  testcase nghiệm thu
├── DEV/             kiến trúc, lộ trình kỹ thuật, quy ước làm việc
├── QA/              tiêu chí chất lượng theo tính năng
├── Art/             đặc tả kỹ thuật art
└── tools/           backlog_to_xlsx.py — xuất CHECKLIST.md ra Excel
```

## Nguyên tắc: mỗi thông tin có đúng MỘT nhà

File khác cần tới thì **trỏ link, không chép lại**. Bảng đầy đủ *thông tin nào ở nhà nào* ở
[PROCESS.md](PROCESS.md) mục 4.

Ba chỗ dễ chép trùng nhất:

| Đừng chép | Nhà của nó |
|---|---|
| Chi tiết vận hành một hệ thống vào GDD | `GD/Systems/<TÊN>.md` — GDD chỉ giữ brief 3–5 dòng |
| Trạng thái đầu việc vào doc khác | [CHECKLIST.md](CHECKLIST.md) |
| Bản đồ / bố cục vào file hệ thống | `Phases/PHASE<n>_MAP.md` |

Mỗi thư mục con có `README.md` làm chỉ mục riêng. Đọc chỉ mục này để tìm đúng thư mục, rồi chỉ mục của thư
mục đó để tìm đúng file — đừng đọc cả cây.
