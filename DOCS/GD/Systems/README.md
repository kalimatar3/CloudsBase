# Systems — chi tiết từng hệ thống

Mỗi hệ thống của game có đúng **một** file ở đây. [GDD](../GDD.md) chỉ giữ brief 3–5 dòng và link xuống;
chi tiết thiết kế nằm trong các file này. Sửa thiết kế một tính năng thì mở đúng một file.

**Mỗi file tự liên kết sang ba mảng còn lại** ở mục *Tham chiếu* cuối file: `DEV/` (code) ·
`QA/` (chất lượng đầu ra) · `Art/` (đặc tả art). Chưa có file nào trong ba mảng đó thì ghi
`— chưa có, tạo khi làm` chứ không bỏ trống, để lúc làm biết còn thiếu gì.

Quy trình thêm hoặc sửa một hệ thống: [`DOCS/PROCESS.md`](../../PROCESS.md).

---

## Chỉ mục

Một bảng cho mỗi mục của GDD. Thêm một dòng mỗi khi tạo file hệ thống mới; cột *Một câu* phải khớp đúng
câu mở đầu của file đó, cột *Trạng thái* ghi `ĐÃ CHỐT` / `BẢN NHÁP` / `[TBD]` kèm phần nào còn thiếu.

### Cơ chế gameplay — GDD mục 02

| File | Một câu | Trạng thái |
|---|---|---|
| _(chưa có)_ | | |

### Thế giới & Nhân vật — GDD mục 03

| File | Một câu | Trạng thái |
|---|---|---|
| _(chưa có)_ | | |

### Cốt truyện & Tiến trình — GDD mục 04

| File | Một câu | Trạng thái |
|---|---|---|
| _(chưa có)_ | | |

---

## Khung chuẩn của một file hệ thống

```
# Hệ thống: <TÊN>
> Một câu nó là gì. Thuộc GDD mục <x.y>. Trạng thái: ĐÃ CHỐT / BẢN NHÁP / [TBD]

## 1. Một câu
## 2. Vì sao hệ thống này thuộc về game này     ← lý do thiết kế, không phải cách code
## 3. Vận hành                                  ← chi tiết hết mức: luật, trạng thái, ngoại lệ
## 4. Bộ số / tham số cân bằng
## 5. Ăn khớp với hệ khác                       ← link ngang sang Systems/ khác
## ★ Còn để ngỏ — [TBD]                         ← câu phải trả lời trước khi code
## Tham chiếu                                   ← bảng DEV / QA / Art
```

Hai mục dễ bị viết sai:

- **Mục 2 không phải mục 3.** Mục 2 trả lời *vì sao game này cần hệ thống này* — bỏ nó đi thì game mất gì.
  Mục 3 mới là luật vận hành. Trộn hai mục lại thì về sau không ai biết luật nào được phép đổi.
- **Mục ★ Còn để ngỏ là cổng chặn, không phải ghi chú.** Mỗi `[TBD]` ở đó là một câu hỏi thiết kế mà code
  sẽ phải trả lời thay nếu không chốt trước. `PROCESS.md` bước 2 đọc đúng mục này để quyết định có dừng
  lại hỏi hay không.

---

## [TBD] đang chặn tiến độ

Bảng gom mọi `[TBD]` đang chặn một đầu việc cụ thể, để không phải mở từng file đi tìm. Một `[TBD]` chưa
chặn gì thì **không** cần có dòng ở đây — nó ở trong file hệ thống của nó là đủ.

| [TBD] | Chặn gì | Ở đâu |
|---|---|---|
| _(chưa có)_ | | |
