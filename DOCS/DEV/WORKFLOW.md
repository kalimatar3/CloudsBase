# Quy ước làm việc — prefab, Addressables, config, test bằng Odin

> Đây là **hợp đồng giữa người và agent**: agent làm theo đúng file này, và khi muốn đổi cách làm thì sửa
> file này TRƯỚC, không sửa ngầm trong code.
>
> Quy ước code và framework (namespace, `...Manager` vs `...Service`, phân rã component, SOLID):
> [`CLAUDE.md`](../../CLAUDE.md) gốc repo. File này nói về **cách vận hành dự án**, không nói về cách viết class.

---

## 1. Chạy game — luôn bắt đầu từ scene Bootstrap

| | |
|---|---|
| Build Settings | `0: Game.Scenes/Bootstrap.unity` → `1: <scene gameplay>` |
| Bấm Play ở đâu | **Scene Bootstrap.** Nó nạp config rồi tự chuyển sang scene sau |
| Bấm Play thẳng từ scene gameplay | Chạy được nhưng thứ gì cần config sẽ thiếu, và log cảnh báo nói rõ lý do |

**Vì sao không tự chữa cho trường hợp Play thẳng:** `Bootstrap` là nơi duy nhất nạp `ConfigService` và
`DataService`. Cho mỗi scene tự nạp lại config khi thiếu nghe thì tiện, nhưng nó tạo ra **hai đường khởi
động khác nhau**, và bug nào chỉ xuất hiện ở một đường sẽ rất khó lần. Thay vào đó, component nào cần
config thì hỏi `ConfigService.IsLoaded<T>()` rồi chờ, và cảnh báo đúng một lần với hướng dẫn cụ thể.

---

## 2. Prefab

**Chỗ đặt:** `Assets/Game.Prefabs/`, tên file = tên dùng để gọi trong code.

**Cấu trúc chuẩn của một prefab nhân vật** — theo quy tắc phân rã component trong `CLAUDE.md`:

```
<Tên>                          ← coordinator + các component "logic"
├── <Tên>Character             ← điều phối, KHÔNG tự làm gì
├── <Tên>Movement              ← chỉ chuyển động
├── <Tên>Interactor            ← chỉ tìm vật thể + mở tương tác
├── Collider
└── Model                      ← toàn bộ phần nhìn thấy
    ├── Animator / SkeletonAnimation
    └── <Tên>Model             ← chọn animation, skin, lật trái/phải
```

Phần **logic** và phần **nhìn thấy** tách làm hai nhánh: đổi cách thể hiện (2D sang 3D, Spine sang
Animator) thì chỉ thay nhánh `Model`, không chạm vào nhánh logic.

---

## 3. Addressables — ba đường, đừng trộn

| Đường | Dùng cho | Nạp thế nào | Đăng ký ra sao |
|---|---|---|---|
| **Group `Game.Config`** | Config `ScriptableObject` dùng chung cả game | `ConfigService.GetConfig<T>()` — đồng bộ, đã nạp sẵn từ Bootstrap | Kéo asset vào group; `ConfigGroupLabeler` tự gắn label `Game.Config` |
| **Group riêng theo mảng** | Prefab nạp theo key | `AssetService.LoadAsync<T>(key)` hoặc `PoolService.SpawnAsync(key, …)` | Thêm entry vào group, `address` = tên file |
| **Thư mục `Assets/Game.Addressable/`** | Asset nội dung (bộ màn, bảng số liệu) | `AssetService.LoadAsync<T>(key)` | **Tự động** — `AddressableFolderSync` đăng ký mọi file trong thư mục, address = tên file |

### Chọn `ConfigService` hay `AssetService`

Đây là chỗ dễ chọn sai nhất, và chọn sai thì **hỏng im lặng**:

- **`ConfigService`** đánh khoá theo `typeof(T)` → **mỗi kiểu đúng MỘT asset**. Hai asset cùng kiểu cùng
  mang label `Game.Config` thì cái nạp sau **đè lên cái trước mà không log gì cả**.
  → Dùng cho: thứ luôn cần, luôn chỉ có một bản.
- **`AssetService`** đánh khoá theo **chuỗi key** → một kiểu có bao nhiêu asset cũng được, nạp khi cần,
  giải phóng được.
  → Dùng cho: nội dung, thứ chỉ một phần đang dùng tại một lúc.

★ **Cái bẫy cụ thể:** cần hai bộ số của cùng một hệ thống (ví dụ hai thiết bị điều khiển, hai độ khó)
thì **copy asset ra thành hai bản là hỏng** — chúng cùng một kiểu. Cách đúng: **một asset, nhiều profile
bên trong nó**, hoặc chuyển sang `AssetService` và đánh key.

### ★ Con số CẢM GIÁC phải nằm trong config, không hardcode

Luật: **thứ gì sẽ bị chỉnh đi chỉnh lại lúc playtest thì phải ở trong một config asset.** Để trong code
nghĩa là mỗi lần thử một giá trị phải sửa code; để `[SerializeField]` trên một component có mặt nhiều lần
trong scene thì mỗi bản sao giữ một giá trị riêng rồi lệch dần.

Thuộc config: tốc độ, ngưỡng, tầm, thời gian giữ phím, thời gian trộn animation, bán kính, vùng chết,
sức chứa, độ dài một khung giờ, mọi thứ liên quan tới *cảm giác* và *cân bằng*.

**Vẫn để `[SerializeField]`** cho thứ khác nhau theo từng object: biên khu vực, hộp che của từng công
trình, điểm vào, danh sách tilemap. Đó là **bố cục**, không phải **cảm giác**.

★ Một config nhận phần trình bày thế giới không thuộc hệ nào cả nên có thuộc tính tĩnh kiểu `.Current`
tự trả về bản mặc định khi chưa nạp — nhờ vậy giá trị mặc định chỉ viết đúng một lần ở khai báo trường,
không phải chép lại ở mỗi chỗ dùng dưới dạng `nếu chưa nạp thì 0.35f`.

### Nhân vật được SPAWN, không đặt sẵn trong scene

Spawner nạp nhân vật theo key rồi publish một tín hiệu `…ReadyMsg`. Camera và bảng test **nghe tín hiệu
đó** để lấy tham chiếu, chứ không kéo sẵn trong Inspector — vì lúc scene mở ra thì nhân vật chưa tồn tại.

Khi mỗi khu vực là một scene riêng và nhân vật phải đi theo người chơi, đây là đường duy nhất chạy được —
không phải đường vòng cho vui.

---

## 4. Test bằng Odin lúc đang chạy

**Quy ước:** mọi con số cần dò bằng tay phải chỉnh được **trong Play mode**, không phải thoát Play →
sửa asset → vào lại.

Hai chỗ đặt nút, mỗi chỗ một vai:

| Chỗ | Đặt gì |
|---|---|
| **Trên chính component** | Thao tác chỉ liên quan tới component đó |
| **Một panel debug riêng** | Con số cân bằng + thao tác cần nhiều đối tượng |

### ⚠️ Sửa trong Play mode CÓ DÍNH vào asset — trong Editor

Trong Editor, Addressables ở chế độ *Use Asset Database* trả về **chính asset thật**, không phải bản sao.
Nên mọi con số kéo trong bảng test đều ghi thẳng vào `ScriptableObject` đang nằm trên đĩa, và thoát Play
**không** hoàn lại.

Trong bản build thì ngược lại — asset là bản sao nạp từ bundle, sửa xong là mất. **Chính chỗ lệch nhau
giữa hai môi trường mới là thứ dễ cắn:** nghịch thử trong Editor tưởng vô hại, hoá ra đã đổi cân bằng.

**Quy trình an toàn:**

1. Trước khi nghịch một loạt: commit hoặc ghi lại giá trị hiện tại của asset.
2. Dò trong Play cho tới khi thấy số ưng.
3. Thoát Play, **mở asset trong Inspector và xác nhận con số đúng là thứ mình muốn giữ** — chứ không
   phải thứ còn sót lại từ một lần kéo thử.
4. Số nào chỉ để thử thì trả về tay.

```bash
git diff Assets/Game.Config/
```

Chạy sau mỗi buổi chỉnh là cách rẻ nhất để thấy mình đã đổi những gì.

### Nút test phải đi ĐÚNG đường mà gameplay đi

Nút test publish đúng tín hiệu mà gameplay publish, **không** gọi tắt vào manager. Nút test đi đường vòng
khác với đường thật thì nó chỉ chứng minh được rằng đường tắt chạy được.

---

## 5. Input

**Asset:** một `.inputactions` trong `Assets/Game.Input/`, trỏ tới từ một config.

**Luật đặt tên bắt buộc:** tên action map phải **trùng đúng** tên trong enum định danh người chơi, vì
service tra map theo `player.ToString()`. Thêm người chơi thứ ba = thêm một map cùng tên, **không** phải
thêm một bảng ánh xạ nữa phải nhớ cập nhật.

Ngoài các map người chơi, có một map **`UI`** không thuộc ai: phím mở/đóng popup.

**Không đọc thẳng `Keyboard.current` / `Mouse.current`.** Mọi input qua action, vì đó là điều kiện để đổi
phím được và để thêm nền tảng sau này chỉ là thêm binding.

Phím đã đổi lưu trong PlayerPrefs. Chưa cần UI đổi phím thì chưa làm, **nhưng đường lưu/nạp phải có sẵn
từ đầu** — gắn nó vào sau khi input đã rải khắp code là việc đắt.

---

## 6. Nghiệm thu một mốc

Trước khi báo xong, agent phải chạy được hết danh sách này và **nói rõ kết quả**:

1. **Biên dịch sạch** — không error, không warning mới.
2. **Vào Play từ Bootstrap** — console không có error.
3. **Chạy đúng kịch bản kiểm tra của mốc** (ghi trong `PHASE<n>_PLAN.md`), báo **số liệu thật** chứ không
   báo "chắc là chạy được".
4. **Ảnh chụp** nếu mốc có phần nhìn thấy được.

Không đạt phần nào thì **nói thẳng phần đó, kèm output** — không im lặng bỏ qua rồi báo xong.
