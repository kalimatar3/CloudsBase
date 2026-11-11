"""Xuất DOCS/CHECKLIST.md ra DOCS/CHECKLIST.xlsx.

File .md là bản chính; file xlsx chỉ là bản xuất để mở bằng Excel / Google Sheets.
Sửa trong xlsx sẽ KHÔNG về được .md — sửa .md rồi chạy lại:

    python DOCS/tools/backlog_to_xlsx.py

Cần openpyxl (pip install openpyxl).
"""
import re
from datetime import date
from pathlib import Path

from openpyxl import Workbook
from openpyxl.formatting.rule import FormulaRule
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from openpyxl.worksheet.datavalidation import DataValidation

DOCS = Path(__file__).resolve().parent.parent
SRC = DOCS / "CHECKLIST.md"
OUT = DOCS / "CHECKLIST.xlsx"

STATUSES = ["Pending", "Progressing", "Done", "Blocked", "Cancelled"]
# CHECKLIST.md ghi trạng thái bằng icon — đây là bảng đổi sang chữ cho Excel
ICONS = {"✅": "Done", "🟡": "Progressing", "⬜": "Pending", "⛔": "Blocked", "✖": "Cancelled"}

COLUMNS = [  # (tiêu đề trong xlsx, tiêu đề cột trong bảng md, độ rộng)
    ("Phase", None, 7),
    ("ID", "__id", 8),            # tách từ tiền tố "P2-08 · " của cột Đầu việc
    ("Đầu việc", "Đầu việc", 60),
    ("Trạng thái", "Trạng thái", 13),
    ("Note", "Note", 100),
]
NCOL = len(COLUMNS)
ST_COL = "D"  # cột Trạng thái trong sheet Backlog
LAST_COL = "E"
# Dòng trống có sẵn danh sách chọn + tô màu, để thêm việc ngay trong Excel
SPARE_ROWS = 100

FONT = "Arial"
GREEN = "3F6F45"
THIN = Side(style="thin", color="C9CFC2")


def plain(md: str) -> str:
    """Bỏ cú pháp markdown, giữ chữ."""
    s = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", md)
    s = re.sub(r"~~(.+?)~~", r"\1", s)
    s = re.sub(r"\*\*(.+?)\*\*", r"\1", s)
    s = re.sub(r"(?<!\w)\*(.+?)\*(?!\w)", r"\1", s)
    return s.replace("`", "").strip()


def parse(text: str):
    """Trả về danh sách phase (số, tên) và các dòng việc (dict theo tiêu đề cột md)."""
    phases, tasks = [], []
    current, header = None, None
    for line in text.splitlines():
        # Tên phase sau dấu "—" là tuỳ chọn: "## PHASE 1" cũng nhận, để
        # CHECKLIST.md rỗng của một dự án mới vẫn qua được cổng kiểm này.
        m = re.match(r"^## PHASE (\d+)(?: — (.+))?$", line)
        if m:
            # Tên phase dừng trước phần đếm " · 10/25 ✅"
            current = int(m.group(1))
            phases.append((current, (m.group(2) or "").split(" · ")[0].strip()))
            header = None
            continue
        if line.startswith("## "):
            current = None
            continue
        if current is None or not line.startswith("|"):
            continue
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if cells[0] == "Đầu việc":
            header = cells
            continue
        m = re.match(r"^(P\d+-\d+)\s*·\s*(.+)$", cells[0]) if header else None
        if m:
            row = dict(zip(header, cells))
            row["Phase"] = current
            row["__id"] = m.group(1)
            row["Đầu việc"] = m.group(2)
            tasks.append(row)
    return phases, tasks


def status_word(cell: str) -> str:
    for icon, word in ICONS.items():
        if icon in cell:
            return word
    for s in STATUSES:  # vẫn nhận dạng chữ, để không vỡ nếu ai đó gõ tay
        if s in cell:
            return s
    raise ValueError(f"Trạng thái lạ: {cell!r}")


def style_header(ws, row: int, ncols: int):
    for c in range(1, ncols + 1):
        cell = ws.cell(row=row, column=c)
        cell.font = Font(name=FONT, bold=True, color="FFFFFF")
        cell.fill = PatternFill("solid", fgColor=GREEN)
        cell.alignment = Alignment(vertical="center", wrap_text=True)
        cell.border = Border(bottom=THIN)


def build_backlog_sheet(ws, tasks):
    for i, (title, _, width) in enumerate(COLUMNS, start=1):
        ws.cell(row=1, column=i, value=title)
        ws.column_dimensions[ws.cell(row=1, column=i).column_letter].width = width
    style_header(ws, 1, len(COLUMNS))

    for r, t in enumerate(tasks, start=2):
        for c, (title, md_col, _) in enumerate(COLUMNS, start=1):
            if md_col is None:
                value = t["Phase"]
            elif md_col == "Trạng thái":
                value = status_word(t[md_col])
            else:
                value = plain(t.get(md_col, ""))
                if value == "—":
                    value = ""
            cell = ws.cell(row=r, column=c, value=value)
            cell.font = Font(name=FONT, size=10)
            cell.alignment = Alignment(vertical="top", wrap_text=title in ("Đầu việc", "Note"))
            cell.border = Border(bottom=THIN)

    last = len(tasks) + 1 + SPARE_ROWS
    for r in range(len(tasks) + 2, last + 1):
        for c in range(1, len(COLUMNS) + 1):
            ws.cell(row=r, column=c).font = Font(name=FONT, size=10)

    dv_status = DataValidation(type="list", formula1='"' + ",".join(STATUSES) + '"', allow_blank=True)
    ws.add_data_validation(dv_status)
    dv_status.add(f"{ST_COL}2:{ST_COL}{last}")

    # Cả dòng đổi màu theo trạng thái
    area = f"A2:{LAST_COL}{last}"
    rules = [
        ("Done", PatternFill("solid", fgColor="D9E7D7"), None),
        ("Progressing", PatternFill("solid", fgColor="FFF2CC"), None),
        ("Blocked", PatternFill("solid", fgColor="F4CCCC"), None),
        ("Cancelled", None, Font(color="999999", strike=True)),
    ]
    for word, fill, font in rules:
        ws.conditional_formatting.add(area, FormulaRule(formula=[f'${ST_COL}2="{word}"'], fill=fill, font=font))

    ws.freeze_panes = "C2"
    ws.auto_filter.ref = f"A1:{LAST_COL}{len(tasks) + 1}"


def build_summary_sheet(ws, phases):
    # Tên dự án lấy từ tên thư mục gốc repo, để base không mang tên của dự án nào.
    ws["A1"] = f"{DOCS.parent.name.upper()} — TIẾN ĐỘ THEO PHASE"
    ws["A1"].font = Font(name=FONT, bold=True, size=14)
    ws["A2"] = (f"Xuất từ DOCS/CHECKLIST.md ngày {date.today():%d/%m/%Y}. File .md là bản chính — sửa ở đó rồi chạy "
                "DOCS/tools/backlog_to_xlsx.py để xuất lại. Sửa trong file này không tự về được .md.")
    ws["A2"].font = Font(name=FONT, italic=True, size=10, color="525C53")

    headers = ["Phase", "Tên phase", "Tổng", "Done", "Progressing", "Pending", "Blocked", "% xong"]
    for c, h in enumerate(headers, start=1):
        ws.cell(row=4, column=c, value=h)
    style_header(ws, 4, len(headers))

    first = 5
    for i, (num, name) in enumerate(phases):
        r = first + i
        ws.cell(row=r, column=1, value=num)
        ws.cell(row=r, column=2, value=name)
        # "Tổng" không tính dòng Cancelled — cùng quy ước với BACKLOG.md
        ws.cell(row=r, column=3, value=f'=COUNTIFS(Backlog!$A:$A,$A{r},Backlog!${ST_COL}:${ST_COL},"<>Cancelled")')
        for c, word in zip(range(4, 8), ["Done", "Progressing", "Pending", "Blocked"]):
            ws.cell(row=r, column=c, value=f'=COUNTIFS(Backlog!$A:$A,$A{r},Backlog!${ST_COL}:${ST_COL},"{word}")')
        ws.cell(row=r, column=8, value=f'=IF(C{r}=0,"—",D{r}/C{r})')
    total = first + len(phases)
    ws.cell(row=total, column=2, value="Tổng")
    for c in range(3, 8):
        col = ws.cell(row=4, column=c).column_letter
        ws.cell(row=total, column=c, value=f"=SUM({col}{first}:{col}{total - 1})")
    ws.cell(row=total, column=8, value=f'=IF(C{total}=0,"—",D{total}/C{total})')

    for r in range(first, total + 1):
        for c in range(1, 9):
            cell = ws.cell(row=r, column=c)
            cell.font = Font(name=FONT, size=10, bold=(r == total))
            cell.border = Border(bottom=THIN)
        ws.cell(row=r, column=8).number_format = "0%"
        ws.cell(row=r, column=8).alignment = Alignment(horizontal="right")

    notes = [
        ("Quy ước", None),
        ("Trạng thái", "Pending = chưa bắt đầu · Progressing = đang làm / xong một phần · Done · "
                       "Blocked = bị chặn (ghi lý do ở Ghi chú) · Cancelled = bỏ, giữ dòng, không tính vào Tổng"),
        ("ID", "P<phase>-<số>, đánh tiếp từ số lớn nhất của phase, không dùng lại số của dòng đã bỏ"),
        ("Note", "Mốc (định nghĩa ở DOCS/DEV/PHASE<n>_PLAN.md), đã có gì / còn thiếu gì, link doc hệ thống"),
        ("Việc mới", "Điền vào DOCS/BACKLOG.md, không thêm trực tiếp vào đây"),
    ]
    r = total + 2
    for label, text in notes:
        ws.cell(row=r, column=1, value=label).font = Font(name=FONT, bold=True, size=10, color=GREEN if text is None else "1B211C")
        if text:
            ws.cell(row=r, column=2, value=text).font = Font(name=FONT, size=10)
        r += 1

    for col, width in zip("ABCDEFGH", [10, 38, 8, 8, 12, 9, 9, 9]):
        ws.column_dimensions[col].width = width


def main():
    phases, tasks = parse(SRC.read_text(encoding="utf-8"))
    phases = [p for p in phases if any(t["Phase"] == p[0] for t in tasks)]
    wb = Workbook()
    summary = wb.active
    summary.title = "Tổng quan"
    build_summary_sheet(summary, phases)
    build_backlog_sheet(wb.create_sheet("Backlog"), tasks)
    # File do openpyxl ghi không có giá trị đã tính sẵn — bắt Excel tính lại khi mở
    wb.calculation.fullCalcOnLoad = True
    wb.save(OUT)
    print(f"{OUT} — {len(phases)} phase, {len(tasks)} việc")


if __name__ == "__main__":
    main()
