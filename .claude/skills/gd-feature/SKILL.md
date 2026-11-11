---
name: gd-feature
description: Cổng quy trình trước khi implement bất cứ tính năng gameplay nào — kiểm tính năng đã được GDD mô tả chưa, mục [TBD] có chặn không, bảng Tham chiếu trỏ tới file nào còn thiếu, và mốc tiên quyết đã xong chưa. Dùng khi bắt đầu một dòng BACKLOG, khi được giao code một hệ thống gameplay, khi dựng hoặc ghép asset, hoặc khi phải quyết định một tính năng có đủ điều kiện để làm chưa. KHÔNG dùng cho việc sửa một lỗi nhỏ trong file đã có.
---

# Cổng quy trình tính năng

**Không implement thứ chưa được mô tả trong GDD.** Chưa có mô tả thì viết mô tả trước.

Nhà của quy trình là [`DOCS/PROCESS.md`](../../../DOCS/PROCESS.md) — file này **không chép** nó. Đây là
phần thực thi: thứ tự chạy, lệnh kiểm được bằng máy, và chỗ dừng.

---

## 0. Việc đã có dòng trong backlog chưa?

Chưa → dùng skill `backlog` trước. Mọi việc phát sinh đều cần một dòng trước khi làm
(`PROCESS.md` mục 1). Ngoại lệ duy nhất: việc nằm trong phạm vi dòng đang làm.

---

## 1. Tìm file hệ thống

```bash
ls DOCS/GD/Systems/
```

Không chắc tính năng thuộc hệ nào → bảng chỉ mục [`DOCS/GD/Systems/README.md`](../../../DOCS/GD/Systems/README.md),
có cả cột *Trạng thái* của từng hệ.

**Không hệ nào khớp → DỪNG, HỎI.** Đó là dấu hiệu 1 của `PROCESS.md` mục 2: GDD không mô tả tính năng này.

---

## 2. Ba lệnh kiểm — chạy hết trước khi viết dòng code đầu tiên

```bash
S=<TEN_HE_THONG>   # tên file hệ thống, không đuôi .md
P=2                # phase đang làm

grep -n "TBD" DOCS/GD/Systems/$S.md
sed -n '/^## [0-9]*\.* *Tham chiếu/,$p' DOCS/GD/Systems/$S.md
grep -nE "^\| M[0-9]+" DOCS/DEV/PHASE$P\_STATUS.md
```

| Lệnh | Đọc ra gì | Không đạt thì |
|---|---|---|
| `grep TBD` | Câu chưa chốt. **Chạm vào thứ phải code** → nó chặn | **DỪNG, HỎI.** Nêu rõ `[TBD]` nào, ở file nào |
| `sed Tham chiếu` | Bảng định tuyến: file DEV / QA / Art của hệ này | Dòng ghi `— chưa có` = file hạ nguồn phải **tạo ở bước 4**, trước khi code |
| `grep M[0-9]` | Trạng thái mốc. Mốc tiên quyết chưa ✅ → việc này chưa tới lượt | **Nói ra**, đừng tự đổi thứ tự hàng đợi |

★ Dự án chưa có `PHASE<n>_STATUS.md` thì lệnh thứ ba không có gì để đọc — bỏ qua nó, **nhưng** vẫn phải
tự hỏi việc này có phụ thuộc việc nào chưa xong không.

---

## 3. Năm dấu hiệu phải DỪNG

Bảng đầy đủ kèm ví dụ: `PROCESS.md` mục 2. Dạng rút gọn để tự soát:

1. GDD không nhắc tới tính năng này
2. Mục `[TBD]` liên quan chưa trả lời
3. Việc **trái** một quyết định đã chốt
4. Hai file nói khác nhau
5. Chạm danh sách BẮT BUỘC — [`DOCS/GD/PROJECT.md`](../../../DOCS/GD/PROJECT.md) mục 2

Khi hỏi: nêu **chỗ mâu thuẫn nằm ở file nào**, và đưa phương án kèm hệ quả. Đừng hỏi "làm thế nào".

★ Dự án tự thêm cổng riêng của mình thì ghi chúng vào `PROJECT.md` mục 2, không vào file này — file này
là phần dùng chung giữa mọi dự án.

---

## 4. Viết doc trước, code sau

Bảng *mảng → file → viết gì*: `PROCESS.md` bước 4. Hai luật khi viết:

- **Mỗi thông tin đúng một nhà** (`PROCESS.md` mục 4). Cần tới thì trỏ link, không chép.
- **File hạ nguồn phải tồn tại TRƯỚC khi code.** Nó là đầu vào của chặng sau — và của một agent DEV
  context sạch, nếu sau này tách role. Doc viết sau khi code xong sẽ mô tả *thứ đã làm*, không phải
  *thứ muốn làm*.

Brief vào GDD: 3–5 dòng, **không** chi tiết vận hành. Thêm dòng vào bảng điều hướng đầu mục 02/03/04,
và một dòng *Nhật ký cập nhật* cuối GDD nếu đây là quyết định thiết kế mới.

★ **Nếu phải vẽ art:** khoá đặc tả **trước** — `Art/<TÊN>.md` phải tồn tại và có bộ số. Sai đặc tả thì
thay art biến thành **làm lại**, không phải đổi file.

---

## 5. Xong thì

- Chuyển dòng backlog sang `CHECKLIST.md` → skill `backlog`
- **Gỡ `[TBD]` đã được trả lời** trong file hệ thống
- Ghi nhật ký GDD nếu quyết định thiết kế có đổi trong lúc làm

---

## Tự soát trước khi báo xong

- [ ] Dòng backlog có tồn tại trước khi tôi bắt đầu
- [ ] Đã chạy cả ba lệnh ở mục 2
- [ ] Không còn `[TBD]` nào chặn phần tôi vừa làm
- [ ] Mọi file ghi `— chưa có` trong bảng *Tham chiếu* mà việc này cần đều đã được tạo
- [ ] Code theo [`CLAUDE.md`](../../../CLAUDE.md) + [`DOCS/DEV/WORKFLOW.md`](../../../DOCS/DEV/WORKFLOW.md)
- [ ] Không chép thông tin nào sang file thứ hai
