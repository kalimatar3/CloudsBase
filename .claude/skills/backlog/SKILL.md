---
name: backlog
description: Ghi và chuyển dòng giữa DOCS/BACKLOG.md và DOCS/CHECKLIST.md — thêm dòng cho việc phát sinh, chuyển dòng đã xong sang CHECKLIST với ID tiếp theo, đổi trạng thái một dòng, và giữ hai file chỉ có bảng 3 cột. Dùng khi phát hiện một việc hoặc lỗi giữa lúc đang làm việc khác, khi làm xong một đầu việc, hoặc khi cần ghi tiến độ. Đây là bước bắt buộc trước mọi việc phát sinh, kể cả việc nhỏ.
---

# Ghi backlog và checklist

Hai file, hai vai: [`DOCS/BACKLOG.md`](../../../DOCS/BACKLOG.md) là **ô đầu vào**,
[`DOCS/CHECKLIST.md`](../../../DOCS/CHECKLIST.md) là **tiến độ**. Quy ước ba cột, nhãn ưu tiên và ký hiệu
trạng thái: [`DOCS/PROCESS.md`](../../../DOCS/PROCESS.md) mục 3 — file này không chép lại, chỉ là phần
thực thi.

---

## 1. Khi nào BẮT BUỘC thêm dòng

Bốn trường hợp ở `PROCESS.md` mục 1. Rút gọn: **mọi việc phát sinh**, kể cả việc nhỏ, kể cả lỗi nảy ra
giữa lúc làm việc khác, kể cả một `[TBD]` cần chốt.

- **Nói ra, đừng sửa im lặng.** Thấy lỗi không liên quan tới việc đang làm → thêm dòng, **không** sửa luôn.
- Nghĩ ra cải tiến → thêm dòng. Không tự làm.
- **Ngoại lệ duy nhất:** việc nằm trong phạm vi dòng đang làm.

Thêm một dòng mất mười giây. Việc làm mà không có dòng nào ghi lại thì không ai thấy nó trong tiến độ.

---

## 2. Thêm dòng vào BACKLOG

Ba cột: `| Việc cần làm | Mô tả / kết quả mong muốn | Ưu tiên |`

- Chèn **cuối bảng**, ngay trước dòng rỗng `|  |  |  |` ở cuối file. Dòng rỗng đó giữ nguyên.
- **Không tự đổi thứ tự hàng đợi.** Backlog chạy lần lượt từ trên xuống; dòng 2 phải làm trước dòng 1 vì
  lý do kỹ thuật thì **nói ra** rồi mới đổi.
- Cột mô tả ghi **kết quả mong muốn** và **nhà của thông tin** (file nào sẽ chứa nó), không ghi cách làm.
- Việc đã chạy một phần → mở đầu mô tả bằng `⟳ <ngày>:` và nói rõ phần nào xong, phần nào còn.

Nhãn ưu tiên: `Bắt buộc` · `Nên có` · `Có thì tốt`. Để trống = `Nên có`. Nhà của quy ước là `PROCESS.md`
mục 3. Kiểm nhãn đang dùng trong file:

```bash
grep -oE '\| (Bắt buộc|Nên có|Có thì tốt) \|$' DOCS/BACKLOG.md | sort | uniq -c
```

Ra nhãn lạ nghĩa là file đã lệch khỏi quy ước — **nói ra**, đừng tự đổi hàng loạt.

---

## 3. Chuyển một dòng đã xong sang CHECKLIST

Làm xong một dòng backlog thì nó **chuyển nhà**: thêm vào CHECKLIST, **xoá khỏi BACKLOG trong cùng một
lượt**. Không để nó tồn tại ở hai nơi.

```bash
# ID tiếp theo của phase đang làm — đánh tiếp từ số lớn nhất, không dùng lại số đã bỏ
grep -oE '^\| P2-[0-9]+' DOCS/CHECKLIST.md | grep -oE '[0-9]+$' | sort -n | tail -1
```

Đổi `P2` thành phase đang làm.

Ba cột: `| Đầu việc | Trạng thái | Note |`

- Cột *Đầu việc* **phải** đúng dạng `P<phase>-<số> · <tên>`. Dấu `·` là U+00B7, có khoảng trắng hai bên —
  script xuất Excel khớp đúng regex này, sai dạng là dòng bị bỏ khỏi bản xuất mà không báo lỗi.
- Chèn **cuối mục phase tương ứng**, trước tiêu đề `## PHASE <n+1>`.
- Ký hiệu trạng thái: `PROCESS.md` mục 3. Không bịa ký hiệu mới — script sẽ ném `ValueError: Trạng thái lạ`.
- Cột Note: mốc của phase, **còn thiếu gì**, link doc. Việc còn phần chưa xong → ghi rõ phần đó **và** thêm
  một dòng backlog cho nó, rồi link tới `BACKLOG.md` trong Note.
- Đánh dấu `★` trước tên cho đầu việc là mắt xích chặn tiến độ.

---

## 4. Hai file đó chỉ chứa bảng

Không chú thích, không lưu ý, không dòng tổng kết, không tiêu đề phụ nào khác ngoài `## PHASE <n>`.
Thứ muốn ghi chú thuộc về `PROCESS.md` (quy ước) hoặc `DOCS/DEV/PHASE<n>_STATUS.md` (việc kỹ thuật còn
treo).

---

## 5. Cổng kiểm bằng máy

```bash
python DOCS/tools/backlog_to_xlsx.py
```

Đọc `DOCS/CHECKLIST.md`, ghi `DOCS/CHECKLIST.xlsx`, in ra số phase và số việc. Chạy được **và** số việc
khớp số dòng `P<n>-` là bảng còn hợp lệ. `ValueError: Trạng thái lạ` = ký hiệu trạng thái sai.

Cần `openpyxl` (`pip install openpyxl`) — chưa cài thì lệnh chết ở dòng import và cổng này coi như không có.

Đếm nhanh không cần chạy script, không sinh file:

```bash
grep -cE '^\| P[0-9]+-' DOCS/CHECKLIST.md
grep -oE '^\| P[0-9]+-[0-9]+[^|]*\| *[^ |]+' DOCS/CHECKLIST.md | awk '{print $NF}' | sort | uniq -c
```

⚠ `DOCS/CHECKLIST.xlsx` là bản xuất, sinh lại được — thêm nó vào `.gitignore` trước khi chạy thường xuyên,
không thì mỗi lượt chạy để lại một file chưa theo dõi.
