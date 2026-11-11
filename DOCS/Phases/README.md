# Các phase phát triển

> Đây là phần **Roadmap** của [GDD](../GD/GDD.md) (mục 06), tách thành folder riêng để không lẫn với các
> doc thiết kế. GDD chỉ giữ bảng phase và câu hỏi của mỗi phase; phạm vi và nghiệm thu chi tiết nằm trong
> các file ở đây.
>
> **Tiến độ đầu việc:** [`CHECKLIST.md`](../CHECKLIST.md) · **việc mới vào qua:** [`BACKLOG.md`](../BACKLOG.md)

★ **Chưa có file phase nào.** Bảng dưới là roadmap mặc định — tên phase và câu hỏi của nó dùng lại được
cho hầu hết game, nhưng **mục tiêu và phạm vi thì mỗi dự án tự chốt**. Tạo `PHASE<n>.md` khi bắt đầu phase
đó, theo cấu trúc 3 khối ở dưới.

| File | Phase | Trạng thái |
|---|---|---|
| _(chưa có)_ | | |

Doc riêng của một phase, tạo khi cần:

| Tên file | Covers |
|---|---|
| `PHASE<n>_MAP.md` | Góc nhìn, bản đồ, hướng mở rộng, bố cục. **Nguồn duy nhất** về bố cục — file hệ thống trỏ vào đây, không chép lại |
| `PHASE<n>_ACCEPTANCE.md` | Testcase nghiệm thu bằng tay, theo nhóm tính năng |

---

### Roadmap mặc định — 6 phase

| Phase | Tên phase | Mục tiêu tóm tắt | Câu hỏi cần trả lời |
|---|---|---|---|
| PHASE 1 | Prototype cơ chế lõi | Kiểm chứng cơ chế lõi bằng bản dựng xám, không art, không nội dung | Cơ chế có giữ được hứng thú khi chỉ có phản hồi tối thiểu? |
| PHASE 2 | Vertical slice — vòng lặp khép kín | Dựng vòng lặp gameplay hoàn chỉnh, art tạm đúng đặc tả | Các nửa của gameplay có nuôi nhau không? |
| PHASE 3 | Polish & xác lập chất lượng | Nâng phạm vi Phase 2 lên chất lượng phát hành, không thêm nội dung mới | Game trông và cảm nhận thế nào khi hoàn chỉnh, và mất bao lâu để sản xuất một đơn vị nội dung ở chất lượng đó? |
| PHASE 4 | Sản xuất nội dung | Nhân nội dung từ một lát cắt chất lượng lên toàn bộ quy mô sản phẩm | Nội dung có đủ đa dạng để giữ chân người chơi suốt thời lượng mục tiêu? |
| PHASE 5 | Cân bằng & hoàn thiện | Biến một game chạy được thành một game chơi hay | Đường cong độ khó và đường cong kinh tế có tạo ra nhịp độ tốt xuyên suốt? |
| PHASE 6 | Phát hành & hậu phát hành | Đưa sản phẩm ra thị trường và ổn định sau ra mắt | Không còn câu hỏi thiết kế — đây là phase thực thi |

Ghi trạng thái mỗi phase là **ĐÃ CHỐT** (đã thảo luận và thống nhất) hoặc **BẢN NHÁP** (đề xuất có cơ sở,
cần bàn lại chi tiết khi tới lúc). Phase xa thường là bản nháp, và nhiều mục sẽ thay đổi sau khi có số
liệu thực tế từ các phase trước — đó là bình thường, không phải thiếu sót.

### Liên kết giữa các phase

Mỗi chuyển tiếp cần **điều kiện chuyển** (đo được, không phải cảm giác) và danh sách **thứ được mang
sang** — thường là tham số đã hiệu chỉnh, đặc tả đã khoá, hoặc số liệu năng suất. Điền bảng này khi chốt
phase đầu tiên:

| Chuyển tiếp | Điều kiện chuyển | Thứ được mang sang phase sau |
|---|---|---|
| _(chưa có)_ | | |

★ Cột thứ hai là cột hay bị bỏ trống, và là cột đáng giá nhất: **số liệu năng suất thực tế** (bao nhiêu
ngày cho một nhân vật, một zone) chỉ đo được ở Phase 3, nhưng nó là đầu vào duy nhất để lập kế hoạch
Phase 4. Không ghi lại thì Phase 4 phải đoán.

### Cấu trúc chuẩn của mỗi phase

| Khối | Nội dung |
|---|---|
| Khối 1 — Tổng quan | Mục tiêu · Câu hỏi cần trả lời · Điều kiện tiên quyết |
| Khối 2 — Phạm vi | Danh sách hạng mục được làm trong phase |
| Khối 3 — Nghiệm thu & đầu ra | Tiêu chí hoàn thành · Sản phẩm bàn giao · Điều kiện dừng |

Mọi phase dùng chung cấu trúc 3 khối này để dễ đối chiếu và không bỏ sót hạng mục.

---

### Phụ lục — Nguyên tắc xuyên suốt mọi phase

| Nguyên tắc | Diễn giải |
|---|---|
| Một phase, một câu hỏi | Phase nào cũng phải trả lời được một câu hỏi cụ thể. Nếu một hạng mục không giúp trả lời câu hỏi đó, nó thuộc về phase khác. |
| Không nhảy phase | Phase sau chỉ bắt đầu khi phase trước đạt tiêu chí nghiệm thu. Toàn bộ sản phẩm dựa lên cơ chế lõi; xây tiếp trên nền chưa được xác nhận là rủi ro lớn nhất của dự án. |
| Chất lượng tạm bợ ở giai đoạn kiểm chứng | Ở các phase kiểm chứng, mọi thứ chỉ cần đủ để trả lời câu hỏi. Đầu tư chất lượng sớm vừa tốn kém vừa làm sai lệch kết quả đánh giá. |
| Danh sách loại trừ là hàng rào | Mọi bổ sung ngoài phạm vi phải chứng minh được là cần thiết cho câu hỏi của phase. Không chứng minh được thì đẩy sang phase sau. |
| Đặc tả trước, tài nguyên sau | Với art và audio: chốt và khoá đặc tả kỹ thuật trước khi sản xuất bất kỳ tài nguyên nào, kể cả tài nguyên tạm. |
| Polish trước, nhân bản sau | Nâng một lát cắt hẹp lên chất lượng phát hành trước khi mở rộng nội dung. Nhân nội dung ở chất lượng tạm sẽ khiến chi phí polish nhân lên theo số lượng nội dung. |
