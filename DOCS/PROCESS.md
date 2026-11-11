# QUY TRÌNH — làm bất cứ việc gì trong dự án này

> **Đọc file này trước khi code.** Quy tắc một câu: **không implement thứ chưa được mô tả trong GDD.**
> Chưa có mô tả thì viết mô tả trước — đó là một phần của việc, không phải thủ tục giấy tờ.
>
> Vì sao: tài liệu viết sau khi code xong sẽ mô tả **thứ đã làm**, không phải **thứ muốn làm**. Hai thứ đó
> khác nhau, và chỉ cái thứ hai mới chặn được việc làm sai.

---

## 1. Việc phát sinh → viết vào backlog TRƯỚC

**Mọi việc phát sinh đều phải có một dòng trong [`BACKLOG.md`](BACKLOG.md) trước khi làm** — kể cả việc nảy
ra giữa lúc đang làm việc khác, và kể cả việc nhỏ.

| Phát sinh | Làm gì |
|---|---|
| Đang làm A thì phát hiện phải làm B trước | Thêm dòng B vào backlog, **nói ra**, rồi làm B |
| Phát hiện một lỗi / thiếu sót không liên quan tới việc đang làm | Thêm dòng vào backlog, **không** sửa luôn |
| Nghĩ ra một cải tiến | Thêm dòng vào backlog. Không tự làm |
| Một `[TBD]` cần chốt | Thêm dòng vào backlog |

Vì sao: việc làm mà không có dòng nào ghi lại thì không ai thấy nó trong tiến độ, và nó biến mất khỏi
backlog ngay lúc nó phát sinh. Thêm dòng mất mười giây.

Chỉ bỏ qua bước này với việc **nằm trong phạm vi của dòng đang làm** — ví dụ dòng *code hệ thống X* thì
không cần thêm dòng riêng cho *tạo file `DEV/SYSTEM_X.md`*, vì file đó là bước 4 của chính dòng đó.

---

## 2. Năm bước

### Bước 1 — Đọc GDD trước

Tính năng này đã được mô tả chưa?

1. Mở [`GD/GDD.md`](GD/GDD.md), tìm trong mục **02** (cơ chế gameplay) / **03** (thế giới & nhân vật) /
   **04** (cốt truyện & tiến trình).
2. Thấy brief rồi thì theo link xuống file trong [`GD/Systems/`](GD/Systems/README.md) và đọc hết file đó —
   nhất là mục **★ Còn để ngỏ [TBD]**.
3. Đọc luôn mục **Tham chiếu** cuối file để biết có file DEV / QA / Art đi kèm chưa.

### Bước 2 — Chưa có, sai, hoặc mâu thuẫn → HỎI

**Dừng lại và hỏi** khi gặp bất kỳ dấu hiệu nào:

| Dấu hiệu | Nghĩa là gì |
|---|---|
| GDD không nhắc gì tới tính năng này | Không có mô tả nào để làm theo. Đoán ý là tự nghĩ ra thiết kế |
| Mục liên quan đang là `[TBD]` | Câu hỏi thiết kế chưa được trả lời, mà code sẽ trả lời nó thay — sai chỗ |
| Việc được giao **trái** với một quyết định đã chốt | Một trong hai phải đổi, và đổi quyết định là việc của người, không phải của agent |
| Hai file nói khác nhau | Một trong hai sai. Làm theo cái nào cũng là đánh cược |
| Chạm vào danh sách BẮT BUỘC của [`GD/PROJECT.md`](GD/PROJECT.md) mục 2 | Đó là danh sách những thứ chỉ người chủ dự án mới được đổi |

Khi hỏi: nêu rõ **chỗ mâu thuẫn nằm ở file nào**, và đưa phương án kèm hệ quả — đừng chỉ hỏi "làm thế nào".

### Bước 3 — Viết brief vào GDD, đúng chỗ

3–5 dòng vào mục con phù hợp, **theo đúng dạng các mục khác đang dùng**:

```markdown
### 2.7 Tên hệ thống

Hai đến bốn câu: nó là gì, người chơi làm gì, và vì sao nó thuộc về game này. Không viết chi tiết
vận hành, không bảng số — những thứ đó thuộc file hệ thống.

**Chi tiết:** [Systems/TEN.md](Systems/TEN.md) · **Code:** DEV/SYSTEM_TEN.md · **Art:** Art/TEN.md
```

Thêm dòng vào bảng điều hướng ở đầu mục 02/03/04, và ghi một dòng vào **Nhật ký cập nhật** cuối GDD nếu đây
là quyết định thiết kế mới.

**Không viết chi tiết vào GDD.** GDD là bản đồ; chi tiết làm nó phình lên và làm mất tác dụng bản đồ.

### Bước 4 — Viết chi tiết vào file liên quan

| Mảng | File | Viết gì |
|---|---|---|
| Thiết kế | `GD/Systems/<TÊN>.md` | Theo khung chuẩn ở [`Systems/README.md`](GD/Systems/README.md): một câu · vì sao · vận hành · bộ số · ăn khớp · [TBD] · tham chiếu |
| Code | `DEV/SYSTEM_<TÊN>.md` | Cấu trúc, file, API, tín hiệu, **lý do của các quyết định kỹ thuật có thể muốn đổi** |
| Chất lượng | `QA/QA_<TÊN>.md` | Xong thì trông thế nào, testcase tay, chỗ xem số liệu |
| Art | `Art/<TÊN>.md` | Bộ số phải khoá trước khi vẽ · **CẦN gì** · **KHÔNG cần gì** |

Không phải việc nào cũng cần cả bốn. Nhưng mục *Tham chiếu* của file hệ thống phải ghi rõ cái nào chưa có:
`— chưa có, tạo khi làm` chứ không bỏ trống dòng.

★ **Nếu phải vẽ art:** khoá đặc tả **trước**. Canvas · pivot · số khung · tỉ lệ so với ô lưới · mọi bán
kính và khoảng cách mà gameplay đọc. Sai đặc tả thì thay art biến thành làm lại thay vì đổi file.

### Bước 5 — Rồi mới implement

Code theo [`CLAUDE.md`](../CLAUDE.md) (framework, quy ước) và [`DEV/WORKFLOW.md`](DEV/WORKFLOW.md) (prefab,
Addressables, config, test bằng Odin). Xong thì:

- Chuyển dòng backlog thành một dòng ở [`CHECKLIST.md`](CHECKLIST.md)
- Gỡ `[TBD]` đã được trả lời trong file hệ thống
- Ghi nhật ký GDD **nếu** quyết định thiết kế có đổi trong lúc làm (thường có)

---

## 3. Luật chạy backlog

[`BACKLOG.md`](BACKLOG.md) là ô đầu vào. Chạy **lần lượt từ trên xuống**, mỗi dòng đi qua bước 1–5.

| Tình huống | Làm gì |
|---|---|
| Dòng rõ ràng, GDD đã mô tả | Làm luôn, từ bước 4 hoặc 5 |
| Dòng rõ ràng, GDD chưa mô tả | Bước 3 → 4 → 5. Brief tự viết, nhưng **báo lại** nội dung brief |
| Dòng **thiếu thông tin** | **Hỏi trước.** Không đoán ý |
| Dòng **trái với GDD** | **Hỏi trước**, nêu rõ chỗ trái và file nào ghi |
| Dòng chạm danh sách BẮT BUỘC | **Hỏi trước**, kể cả khi backlog ghi rõ ràng |
| Làm xong một dòng | Chuyển nó thành dòng trong [`CHECKLIST.md`](CHECKLIST.md), xoá khỏi backlog |

Không tự ý đổi thứ tự hàng đợi. Nếu dòng 2 phải làm trước dòng 1 vì lý do kỹ thuật, **nói ra** rồi mới đổi.

### Hai file đó chỉ có bảng, không có chú thích

[`BACKLOG.md`](BACKLOG.md) và [`CHECKLIST.md`](CHECKLIST.md) **chỉ chứa bảng 3 cột**, không thêm ghi chú, lưu
ý hay bảng tổng kết nào. Quy ước của chúng nằm ở đây:

| | BACKLOG.md | CHECKLIST.md |
|---|---|---|
| Ba cột | Việc cần làm · Mô tả / kết quả mong muốn · Ưu tiên | Đầu việc · Trạng thái · Note |
| Ai điền | Người chủ dự án | Agent, sau khi làm |
| Ưu tiên | Bắt buộc · Nên có · Có thì tốt. Để trống = Nên có | — |
| Trạng thái | — | ✅ xong · 🟡 đang làm · ⬜ chưa làm · ⛔ bị chặn · ✖ đã bỏ |
| Note | — | Mốc, còn thiếu gì, link doc. **Dòng ✅ để trống** |
| ID | — | Tiền tố `P<phase>-<số>` trong cột Đầu việc, đánh tiếp từ số lớn nhất, không dùng lại số đã bỏ |

Cột *Đầu việc* của CHECKLIST phải đúng dạng `P<phase>-<số> · <tên>`, dấu `·` là U+00B7 có khoảng trắng hai
bên. Giữ đúng một dạng là điều kiện để đếm và soát được bằng `grep` — mỗi dòng lệch dạng là một dòng
không ai đếm tới.

★ **Markdown là bản duy nhất.** Không xuất ra Excel, không sinh file song song. Hai bản của cùng một bảng
thì sớm muộn lệch nhau, và bản nào đúng thì không ai biết.

### Soát bằng grep

Đếm số việc, và đếm theo từng ký hiệu trạng thái:

```bash
grep -cE '^\| P[0-9]+-' DOCS/CHECKLIST.md
```

```bash
grep -oE '^\| P[0-9]+-[0-9]+[^|]*\| *[^ |]+' DOCS/CHECKLIST.md | awk '{print $NF}' | sort | uniq -c
```

Lệnh thứ hai ra ký hiệu lạ nghĩa là có dòng dùng trạng thái không có trong bảng trên. Kiểm số cột:

```bash
awk -F'|' '/^\| P[0-9]+-/ && NF != 5 {print FILENAME":"NR}' DOCS/CHECKLIST.md DOCS/BACKLOG.md
```

In ra dòng nào nghĩa là dòng đó không đúng 3 cột — thường vì có dấu `|` chưa được escape trong một ô.

---

## 4. Nhà của từng loại thông tin

Mỗi thông tin có **đúng một nhà**. File khác cần tới thì trỏ link, **không chép lại**. Đây là thứ chặn tài
liệu trùng lặp quay lại.

| Thông tin | Nhà |
|---|---|
| Pitch, nền tảng, tóm tắt dự án | [`GD/GDD.md`](GD/GDD.md) mục 01 |
| Brief mỗi hệ thống (3–5 dòng) | `GD/GDD.md` mục 02–04 |
| **Thiết kế chi tiết một hệ thống** | `GD/Systems/<TÊN>.md` |
| Bản đồ, góc nhìn, bố cục | `Phases/PHASE<n>_MAP.md` |
| Phong cách art (nên / không nên) | `GD/GDD.md` mục 05 |
| Đặc tả kỹ thuật art | `Art/<TÊN>.md` |
| Phạm vi & nghiệm thu từng phase | [`Phases/PHASE<n>.md`](Phases/README.md) |
| Nguyên tắc xuyên suốt, liên kết giữa các phase | [`Phases/README.md`](Phases/README.md) |
| **Trạng thái từng đầu việc** | [`CHECKLIST.md`](CHECKLIST.md) |
| Việc mới chưa bắt đầu | [`BACKLOG.md`](BACKLOG.md) |
| Lộ trình kỹ thuật một phase | `DEV/PHASE<n>_PLAN.md` |
| Việc kỹ thuật còn treo, thứ làm ngoài backlog | `DEV/PHASE<n>_STATUS.md` |
| Kiến trúc & cách code một hệ thống | `DEV/SYSTEM_<TÊN>.md` |
| Quy ước prefab / Addressables / config / test | [`DEV/WORKFLOW.md`](DEV/WORKFLOW.md) |
| Framework, namespace, quy ước code | [`CLAUDE.md`](../CLAUDE.md) ở gốc repo |
| Testcase nghiệm thu | [`QA/`](QA/README.md) |
| Ràng buộc BẮT BUỘC cho agent | [`GD/PROJECT.md`](GD/PROJECT.md) mục 2 |

---

## 5. Ví dụ đi hết quy trình

**Việc:** một dòng backlog ghi *"code hệ thống X"*.

| Bước | Kết quả |
|---|---|
| 1 | GDD mục 02 có brief cho X → mở `Systems/X.md` |
| 2 | Mục *Còn để ngỏ* của file đó có **2 thứ chặn**: một khái niệm trong dữ liệu phiên chưa có, và một hệ phụ thuộc chưa làm → **báo trước khi bắt đầu**, vì hệ kia phải làm trước |
| 3 | Brief đã có trong GDD, không cần viết thêm |
| 4 | Tạo `DEV/SYSTEM_X.md` (mục *Tham chiếu* của `X.md` đã ghi sẵn là chưa có) · thêm testcase vào QA · `Art/X.md` nếu cần vẽ |
| 5 | Code → chuyển dòng sang CHECKLIST → gỡ `[TBD]` đã trả lời trong `X.md` |

Điểm đáng chú ý: **bước 2 bắt được hai thứ chặn trước khi viết dòng code nào.** Đó là toàn bộ lý do quy
trình này tồn tại.
