# Art

Đặc tả kỹ thuật art, quy ước đặt tên và import, định hướng art. Owner: Art.

> Mỗi file ở đây là đích **Art** mà file hệ thống trong [`GD/Systems/`](../GD/Systems/README.md) trỏ tới.
> Phong cách chung (bảng nên / không nên): [`GD/GDD.md`](../GD/GDD.md) mục 05.
> Quy trình: [`DOCS/PROCESS.md`](../PROCESS.md).

| File | Covers |
|---|---|
| _(chưa có)_ | |

## Quy trình sản xuất asset 3D — chưa viết

Base **chưa mang theo** quy trình sản xuất 3D nào. Bộ công cụ của dự án trước (script Blender đo tỉ lệ,
retopo, so silhouette với concept) đã được đánh giá là **không đủ tốt để dùng lại**, nên nó cố ý không
lên đây.

Hướng đã chốt cho bản xây lại: **dựng khối ở Meshy AI**, hoàn thiện bằng **Blender và Unity ở mức cơ
bản** — không dựng lại tầng script đo đạc. Nhà của nó khi viết: `PIPELINE_3D.md` trong thư mục này.

⚠ **Khi nối Meshy vào repo:** khai báo MCP server trong `.mcp.json` nhưng API key phải để dạng
`"MESHY_API_KEY": "${MESHY_API_KEY}"` và đặt giá trị thật ở biến môi trường. **Không** viết key vào
`.mcp.json` — file đó nằm trong git.

## Khung chuẩn của một file đặc tả art

```
# Art <TÊN> — đặc tả, cần gì và KHÔNG cần gì
> Một câu. Thiết kế (tính cách, vai) ở GD/Systems/<TÊN>.md; phong cách chung ở GDD mục 05.

## 1. Bộ số phải khoá trước khi vẽ   ← bảng: giá trị · nguồn / ghi chú
## 2. CẦN gì                         ← danh sách tài nguyên phải có
## 3. KHÔNG cần gì                   ← danh sách rõ ràng, để không vẽ thừa
## 4. Quy ước đặt tên và import
## Tham chiếu
```

## Hai luật của mảng art

**1. Đặc tả trước, tài nguyên sau.** Khoá hết bộ số ở mục 1 **trước khi vẽ khung hình đầu tiên** — canvas,
pivot, số khung, tỉ lệ so với ô lưới, mọi bán kính và khoảng cách mà gameplay đọc. Sai đặc tả thì thay art
biến thành **làm lại**, không phải đổi file.

**2. Mục *KHÔNG cần gì* quan trọng ngang mục *CẦN gì*.** Nó là thứ chặn việc vẽ thừa — thứ đắt nhất trong
mảng art, vì nó tiêu thời gian mà không ai phát hiện ra là lãng phí cho tới lúc ghép vào game.
