# &lt;TÊN GAME&gt; — Game Design Document

> **File này là bản TỔNG QUAN.** Mỗi hệ thống chỉ có một brief 3–5 dòng kèm link xuống
> [`Systems/`](Systems/README.md). Chi tiết vận hành, bảng số, ngoại lệ — tất cả ở file hệ thống.
>
> Vì sao: GDD là bản đồ. Nhét chi tiết vào đây làm nó phình lên và mất tác dụng bản đồ, rồi không ai
> đọc nó nữa. Quy trình thêm hoặc sửa một mục: [`DOCS/PROCESS.md`](../PROCESS.md).

★ **Khung rỗng.** Mỗi mục dưới đây ghi *viết gì vào đó*, chưa có nội dung. Điền mục 01 trước; mục 02–04
dày lên dần theo từng hệ thống được chốt.

---

## 01. TỔNG QUAN DỰ ÁN

**Nhà của:** pitch, nền tảng, tóm tắt dự án.

| Cần có | Ghi chú |
|---|---|
| Tóm tắt | Game gì, chơi thế nào, trong 3–5 câu. Người lạ đọc xong phải hình dung được một phiên chơi |
| Pitch | Một câu bán được game. Thứ làm nó khác với game cùng thể loại |
| Nền tảng & đối tượng | PC/mobile/console, điều khiển, thời lượng một phiên |
| Điều khiển & trình bày | Tỉ lệ khung hình, góc nhìn, có nút trên màn hình hay không |

---

## 02. CƠ CHẾ GAMEPLAY

**Nhà của:** brief mỗi cơ chế. Chi tiết ở `Systems/<TÊN>.md`.

| Mục | Hệ thống | Chi tiết |
|---|---|---|
| _(chưa có)_ | | |

Mỗi mục con theo đúng dạng này:

```markdown
### 2.1 Tên hệ thống

Hai đến bốn câu: nó là gì, người chơi làm gì, và vì sao nó thuộc về game này. Không viết chi tiết
vận hành, không bảng số — những thứ đó thuộc file hệ thống.

**Chi tiết:** [Systems/TEN.md](Systems/TEN.md) · **Code:** DEV/SYSTEM_TEN.md · **Art:** Art/TEN.md
```

★ Mục nào chưa chốt thì ghi thẳng `— [TBD]` vào tiêu đề, và viết câu hỏi còn treo vào file hệ thống của
nó. `PROCESS.md` bước 2 đọc đúng chỗ đó để quyết định có dừng lại hỏi hay không.

---

## 03. THẾ GIỚI & NHÂN VẬT

**Nhà của:** brief về thế giới, nhân vật, cư dân, khu vực. Chi tiết ở `Systems/`.

| Mục | Hệ thống | Chi tiết |
|---|---|---|
| _(chưa có)_ | | |

★ Bố cục bản đồ **không** thuộc mục này — nhà của nó là `Phases/PHASE<n>_MAP.md`. Ở đây chỉ nói thế giới
*là gì*, không nói nó *nằm ở đâu trên bản đồ*.

---

## 04. CỐT TRUYỆN & TIẾN TRÌNH

**Nhà của:** brief về cách kể, cấu trúc tiến trình, điều kiện mở khoá.

| Mục | Hệ thống | Chi tiết |
|---|---|---|
| _(chưa có)_ | | |

---

## 05. PHONG CÁCH NGHỆ THUẬT

**Nhà của:** phong cách art ở dạng bảng **nên / không nên**. Đặc tả kỹ thuật từng loại asset thì ở
[`Art/`](../Art/README.md).

| Làm | KHÔNG làm |
|---|---|
| _(chưa có)_ | |

★ Cột **KHÔNG làm** là cột có tác dụng thật: nó là thứ duy nhất chặn được một asset đã vẽ xong nhưng
sai hướng. Viết nó cụ thể tới mức một người mới nhìn vào asset là chấm được đạt hay không.

---

## 06. ROADMAP PHÁT TRIỂN

Chỉ giữ **bảng phase và câu hỏi của mỗi phase**. Phạm vi và nghiệm thu chi tiết ở
[`Phases/`](../Phases/README.md) — xem roadmap mặc định 6 phase ở đó.

| Phase | Tên phase | Câu hỏi cần trả lời | Trạng thái |
|---|---|---|---|
| _(chưa có — xem Phases/README.md)_ | | | |

---

## Nhật ký cập nhật

Một dòng cho **mỗi quyết định thiết kế mới hoặc bị đổi**, mới nhất ở trên. Không ghi việc sửa chính tả
hay sắp lại bố cục.

| Ngày | Đổi gì | Vì sao |
|---|---|---|
| _(chưa có)_ | | |

★ Cột *Vì sao* là cột làm nhật ký này đáng giữ. Sáu tháng sau, câu hỏi luôn là *"vì sao hồi đó lại chốt
thế"* — không phải *"hồi đó chốt gì"*.
