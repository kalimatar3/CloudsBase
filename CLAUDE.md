# CLAUDE.md

This file provides coding guidance for AI agents working in this repository.

## Project Snapshot

`CloudsBase` is a Unity (URP) reusable game framework providing shared systems for:

- base component lifecycle (`MyBehaviour`, `Singleton<T>`) and a single `Bootstrap` entry point,
- decoupled messaging (`SignalBus` — global and scoped pub/sub),
- UI animation (`UIAnimationContainer`, `DOTweenAnimationFactory`),
- popup show/hide (`PopupService`, `PopupView`),
- data persistence (`LoadSaveService`, `Repository<T>`, `DataService`),
- object pooling (`PoolService`, Addressables key-based, despawn strategies),
- hierarchical state machine (`State`, `Statemachine<T>`),
- physics helpers and Timeline integration.

The framework lives entirely under `Assets/Clouds/`. Game projects import or copy this folder and build on top of it.

## Project Structure

```
Assets/Clouds/
├── Clouds.Common/    # InterfaceReference<T>, Tools, ComponentExtensions, DataHelper, Serializable2DArray (utilities)
├── Clouds.Data/      # DynamicData, SerializableDictionary, ExcelReader
├── Editor/           # Custom editors + property drawers: DOTweenPreviewer, SerializableDictionaryDrawer,
│                     #   Show2DArrayDrawer, MissingScriptFinder, TMPFontChecker, UIAnimation*Editor
├── Clouds.Manager/   # MyBehaviour, Bootstrap, DataService, Repository<T>, LoadSaveService
├── Clouds.Materials/ # Shared materials — reused across games, no game-specific art baked in
├── Clouds.Physics/   # PhysicUltilitis, SetAllRigidbody
├── Clouds.Plugins/   # ExcelDataReader DLLs
├── Resources/        # UISetting.asset (loaded at runtime by name)
├── Clouds.Shaders/   # Shared HLSL/ShaderGraph — pure technique, never game-specific by nature
├── Clouds.Signal/    # SignalBus — the only messaging system; use it, not ad-hoc events
├── Clouds.Singleton/ # Singleton<T>
├── Clouds.Spawner/   # PoolService (Addressables key-based pooling), IDespawnable
├── Clouds.State/     # State, Statemachine<T>
├── Clouds.Strategy/  # DeSpawnStrategy, DeSpawnbyEvent, DeSpawnbytime_Strategy
├── Clouds.Textures/  # Only textures a shared Material in Clouds.Materials/ actually references
├── Clouds.Timeline/  # Custom Timeline tracks and behaviours
└── Clouds.UI/        # Physical subfolders below are organization only — namespace stays flat Clouds.UI
    ├── Animation/    # UIAnimationContainer, DOTweenAnimationFactory, PrimeTweenAnimationFactory,
    │                 #   TweenCUIAnimation, AnimationPresets/, Data/ (UIAnimationData, UISetting, UIData),
    │                 #   Enums/ (UIEnums), Interfaces/ (IUIAnimation*, IUISetData)
    ├── Popup/        # PopupService (static show/hide by key), PopupView (self-registering bridge)
    └── Layout/       # UIHelper, UIUtility, FlexibleGridScaler, HorizontalLayoutResizer, TextUIFormula, FitRectText
```

Each folder is named after the namespace it holds (`Clouds.Common/` ↔ `namespace Clouds.Common`). `Editor/` and `Resources/` keep their literal names — Unity matches those exact strings to exclude editor code from player builds and to resolve `Resources.Load`.

### Namespace Convention

Every script sits in exactly one namespace, matching its **logical component** — not necessarily its physical folder. All namespaces are flat, one level under `Clouds`: `Clouds.Common`, `Clouds.Data`, `Clouds.Editor`, `Clouds.Manager`, `Clouds.Physics`, `Clouds.Signal`, `Clouds.Singleton`, `Clouds.Spawner`, `Clouds.State`, `Clouds.Strategy`, `Clouds.Timeline`, `Clouds.UI`.

- Physical location = compile/organization boundary (e.g. `Editor/` stays a real folder so Unity excludes it from player builds). Namespace = ownership. A property drawer living in `Editor/` for a `Common/` type is `namespace Clouds.Common`, not `Clouds.Editor` — `Clouds.Editor` is reserved for tools with no single owning component (`MissingScriptFinder`, `TMPFontChecker`).
- `Clouds.UI` is intentionally flat — it does not split into `Clouds.UI.Animation`/`.Editor`/`.Settings` sub-namespaces. Everything UI-related (runtime, data, and its editor tooling) is one namespace.
- **Gotcha:** `Clouds.Physics` and `Clouds.Singleton` share their last segment with a type inside them (`SetAllRigidbody`'s namespace vs. `UnityEngine.Physics`, `Singleton<T>` itself). Inside `Clouds.Physics`, always fully-qualify `UnityEngine.Physics` (e.g. `UnityEngine.Physics.gravity`) — the bare name resolves to the namespace, not the class.
- When adding a new script, `using` the component namespace(s) it depends on rather than duplicating types across namespaces.

`Assets/Clouds/` holds framework **code**, plus visual assets that are genuinely reusable across games: `Clouds.UI/AnimationPresets/`, `Resources/UISetting.asset`, `Clouds.Materials/`, `Clouds.Shaders/`, and the small set of `Clouds.Textures/` those materials depend on. It does **not** hold one-off game content — no Models/Prefabs/Scenes, and no Materials/Textures tied to a specific game's art (e.g. a logo texture). When adding a new Material to `Clouds.Materials/`, check what it references: if it bakes in a texture, that texture belongs in `Clouds.Textures/` too, or the Material doesn't belong here.

### Game.* Project Folders

Assets that belong to the current game (not reusable across projects) live in top-level `Assets/Game.*` folders, siblings of `Assets/Clouds/`:

```
Assets/
├── Game.Scripts/    # game-specific C# — subdivided by role like Assets/Clouds/ (Manager/, Editor/, and
│                    #   Common/, Data/, Helper/, etc. as they're needed)
├── Game.Textures/   # art tied to this game (not referenced by any Clouds.Materials/)
├── Game.Models/
├── Game.Prefabs/
├── Game.Scenes/
├── Game.Settings/   # URP pipeline/quality/volume assets
└── Game.GUI/         # 2D UI art (menus, localized textures)
```

`Game.Materials/` and `Game.Shaders/` aren't present right now — everything currently in Materials/Shaders is reusable and lives in `Assets/Clouds/` instead. Recreate a `Game.Materials/`/`Game.Shaders/` folder only for a one-off material/shader tied specifically to this game (e.g. baking in a texture from `Game.Textures/`).

When adding a new game-specific type-folder, prefix it `Game.` (e.g. `Game.Audio/`) rather than adding an unprefixed folder at `Assets/` root, so game content stays visually distinct from `Assets/Clouds/` and from vendored third-party packages (`Plugins/`, `Spine/`, `AllPakage/`, etc., which keep their own names and are never renamed to `Game.*`). `Assets/Resources/` keeps its literal name since Unity resolves `Resources.Load` by that exact folder name.

## Runtime Architecture

### Base Component — MyBehaviour

`Assets/Clouds/Clouds.Manager/MyBehaviour.cs` — base class for all framework MonoBehaviours.

- Override `LoadComponents()` to auto-discover sibling/child components.
- Called automatically on `Reset()` (Editor) and `Awake()` (runtime).
- All concrete classes that need auto-wiring should extend `MyBehaviour`, not `MonoBehaviour`.

### Singleton

`Assets/Clouds/Clouds.Singleton/Singleton.cs` — generic lazy singleton for MonoBehaviours.

- `Singleton<T>.Instance` auto-creates a GameObject if none exists.
- Prevents duplicates (destroys extras).
- Usage: `public class MyManager : Singleton<MyManager>` — only for classes that must actually live on a scene GameObject (see naming convention below).
- No domain-reload reset hook needed: `_instance` is a `UnityEngine.Object` reference, and Unity's overloaded `==` already treats a destroyed object as `null`, so a stale reference from a previous Editor Play session self-heals on the next access.

### Static Services vs. MonoBehaviour Managers

Naming convention (enforced, not just a suggestion):
- **`...Manager`** — reserved for **MonoBehaviour**-derived classes that must live on a scene GameObject (e.g. because they need `Awake`/`Update`, Inspector-assigned references, or `Singleton<T>`).
- **`...Service`** — static classes with no scene presence (`LoadSaveService`, `DataService`, `PopupService`, `PoolService`).
- **`...Repository<T>`** (or bare `Repository<T>`) — static, generic-over-data-type flavor of a service (`Repository<T>`).
- Never name a static class `...Manager`, and never name a MonoBehaviour `...Service`.

### Bootstrap

`Assets/Clouds/Clouds.Manager/Bootstrap.cs` — single MonoBehaviour entry point for installing services at startup.

- The framework's own services are stateless/lazy and need no explicit init step, so the base `InitializeAsync()` is a no-op.
- Override `InitializeAsync()` in a game-specific bootstrap to add ordered async steps (config load, backend auth, player data load, …), mirroring an `AppBootstrap` → `GameBootFlow` split for larger projects.

### Messaging — SignalBus

`Assets/Clouds/Clouds.Signal/SignalBus.cs` — static pub/sub, no interface constraint on messages.

Messages are plain structs:
```csharp
public struct OpenShopMsg { public string Source; }
```

**Global scope:**
```csharp
SignalBus.Subscribe<OpenShopMsg>(OnOpen);
SignalBus.Publish(new OpenShopMsg { Source = "button" });
SignalBus.Unsubscribe<OpenShopMsg>(OnOpen);
```

**Scoped scope** (namespaced to a type, prevents cross-panel leakage):
```csharp
SignalBus.Scope<ShopPanel>().Subscribe<OpenShopMsg>(OnOpen);
SignalBus.Scope<ShopPanel>().Publish(new OpenShopMsg());
SignalBus.Scope<ShopPanel>().Unsubscribe<OpenShopMsg>(OnOpen);
```

**Async handlers:**
```csharp
SignalBus.SubscribeAsync<OpenShopMsg>(async msg => { await DoSomethingAsync(); });
await SignalBus.PublishAsync(new OpenShopMsg());
```

Always subscribe in `OnEnable`, unsubscribe in `OnDisable`. Call `SignalBus.ClearAll()` on scene teardown if needed.

`SignalBus` also resets itself automatically via `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` (Editor-only) — subscriber dictionaries are plain C# state, so without this they'd leak stale entries across Play sessions when domain reload is disabled. `PopupService` and `PoolService` carry the same reset hook for the same reason.

## UI System

### UIAnimationContainer

`Assets/Clouds/Clouds.UI/Animation/UIAnimationContainer.cs` — attach to any UI GameObject to give it named animations.

- Inspector: `List<AnimationEntry>` — each entry is a `string Key` + `UIAnimationData` asset.
- Builds a `Dictionary<string, List<IUIAnimation>>` at `Awake()`.
- `AnimationFactory` static references `UISetting.GetFactory()` (DOTween by default).

```csharp
_anim.Play("Show");
_anim.Play("Hide", onComplete: () => gameObject.SetActive(false));
_anim.Stop("Click");
_anim.StopAll();
bool exists = _anim.HasKey("Show");
```

Edit-mode preview via `UIAnimationContainerEditor` — Play/Stop buttons per key in Inspector.

### UIAnimationData (ScriptableObject)

`Assets/Clouds/Clouds.UI/Animation/Data/UIAnimationData.cs` — holds `UIEffectData[]`. Each effect defines:
- `TRIGGEREFFECT` type: `Move`, `Rotate`, `Scale`, `Shake`, `Punch`, `Fade`, `Color`
- Delay, Duration, Ease, Loop settings
- Type-specific parameters (offsets, vectors, colors, etc.)

Prebuilt assets in `Assets/Clouds/Clouds.UI/Animation/AnimationPresets/` (FadeIn, FadeOut, Bounce, Click, MoveUp, etc.).

### PopupService

`Assets/Clouds/Clouds.UI/Popup/PopupService.cs` — static show/hide by string key. Replaces the old `PanelManager` MonoBehaviour/Singleton; no queue/stacking yet (deferred).

- `PopupView` (`Popup/PopupView.cs`) — attach to a popup root; self-registers in `Awake()` (keyed by an Inspector field or the GameObject name) and unregisters in `OnDestroy()`. Registration happens in `Awake`, not `OnEnable`, specifically so `Hide()`'s `SetActive(false)` doesn't unregister the popup.
- `PopupService.Show(key)` / `Hide(key)` / `HideAll()` — toggle a registered popup's `GameObject.SetActive`.

```csharp
PopupService.Show("Shop");
PopupService.Hide("Shop");
```

Combine with animation the same way panels used to:
```csharp
_anim.Play("Show", onStart: () => gameObject.SetActive(true));
_anim.Play("Hide", onComplete: () => gameObject.SetActive(false));
```

### UI Utilities (`Clouds.UI/Layout/`)

| File | Purpose |
|---|---|
| `UIHelper.cs` | Static helpers: anchor presets, scroll-to-child, grid utilities, time/number formatting |
| `UIUtility.cs` | Runtime effect helpers: fill text, change opacity |
| `FlexibleGridScaler.cs` | Responsive GridLayout scaling for varying screen sizes |
| `HorizontalLayoutResizer.cs` | Auto-resize parent to fit horizontal layout children |
| `TextUIFormula.cs` | TMP text measurement, number formatting (k/m/g suffixes) |
| `Tools.cs` | List shuffle, dictionary sort, recursive opacity, background resize tweens |

## Data & Persistence

### Repository\<T\>

`Assets/Clouds/Clouds.Manager/Repository.cs` — static, generic-over-data-type cache. Replaces the old `AbtractDataManager<T>` (which was `Singleton`/`MonoBehaviour`-based with a coroutine load gate); `Repository<T>` is plain static and loads synchronously on first access — no coroutine, no scene presence.

```csharp
PlayerData data = Repository<PlayerData>.Data;   // lazy-loads from disk on first access, then cached
Repository<PlayerData>.Save();
bool loaded = Repository<PlayerData>.IsLoaded;
```

- `T` must be `DynamicData, new()` — a concrete data class needs its own public parameterless constructor even though `DynamicData`'s own constructor requires a name (chain it: `public PlayerData() : base("PlayerData") { }`).
- Static fields are per-closed-generic-type (`Repository<PlayerData>` and `Repository<SettingData>` don't share state), which also means it has **no** domain-reload reset hook — `[RuntimeInitializeOnLoadMethod]` can't target an open generic. This is a known, accepted limitation, not an oversight.
- `DataService` (`Clouds.Manager/DataService.cs`) — static extension point for game-specific aggregation, e.g. `public static PlayerData Player => Repository<PlayerData>.Data;`. Empty in the base framework by design.

### LoadSaveService — Local Storage Service

`Assets/Clouds/Clouds.Manager/LoadSaveService.cs` — JSON serialization to device and PlayerPrefs. Every method wraps its parse/IO call in try/catch and logs on failure (a corrupted save or malformed JSON no longer crashes unhandled).

```csharp
LoadSaveService.SaveDatatofile("player.json", data);
LoadSaveService.LoadDataFromFile("player.json", out PlayerData data); // returns bool success
T data = LoadSaveService.LoadDataFromJson<T>(jsonString);
T data = LoadSaveService.LoadDataFromPlayerPref<T>("key");
string json = LoadSaveService.DataToJson(obj);        // Unity JsonUtility
string json = LoadSaveService.DatatoJsonConvert(obj); // Newtonsoft.Json
```

`Repository<T>` is built directly on `SaveDatatofile`/`LoadDataFromFile`.

### DynamicData & SerializableDictionary

- `DynamicData` — abstract base for game data objects (has `Name` property). Extend for custom data.
- `SerializableDictionary<TKey, TValue>` — `Dictionary<K,V>` with Unity serialization via `ISerializationCallbackReceiver`.

### ExcelReader

`Assets/Clouds/Clouds.Data/ExcelReader.cs` — reads `.xlsx`/`.xls` into `string[rows, cols]` using ExcelDataReader. Assets path is auto-converted to absolute path.

## State System

`Assets/Clouds/Clouds.State/`

```csharp
var idle = new State { OnEnter = () => { }, OnExit = () => { }, OnUpdate = () => { } };
var run  = new State { ParentState = idle };  // hierarchical

var machine = new Statemachine<MyStateEnum>();
machine.Initialize(MyStateEnum.Idle);
machine.ChangeState((int)MyStateEnum.Run);
```

- `State` supports parent/child hierarchy — enter walks top-down, exit walks bottom-up.
- `Statemachine<T>` uses index to switch states; `CanChange` flag guards invalid transitions.

## Object Pool

`Assets/Clouds/Clouds.Spawner/PoolService.cs` — static, key-based pool backed by Addressables. Replaces the old `Spawner<T>` (`Singleton`/`MonoBehaviour`, scene-populated prefab list) — no scene setup required, pools are created lazily on first spawn.

```csharp
GameObject enemy = await PoolService.SpawnAsync("Enemy", position, rotation); // key = Addressables key
PoolService.Despawn("Enemy", enemy);
PoolService.ReleasePool("Enemy");   // destroy pooled instances + release the Addressable
PoolService.ReleaseAllPools();
```

- Pool holder GameObjects are parented under a lazily-created `DontDestroyOnLoad` root; an `Application.quitting` guard prevents recreating that root during app teardown.
- Dequeue skips over destroyed/null queued instances defensively.
- Domain-reload reset hook included (concrete static class, unlike `Repository<T>`).

**Despawn strategies** (`Assets/Clouds/Clouds.Strategy/`) — implement `IDespawnable.Despawn()` (single-method interface; no `GetTransform()`) and call back into `PoolService.Despawn(key, gameObject)`:
```csharp
new DeSpawnbytime_Strategy(despawnable, 3f).Excute();     // after 3 seconds, via UniTask.Delay (no coroutine/MonoBehaviour host needed)
new DeSpawnbyEvent(despawnable, ref myAction).Excute();   // when action fires
```

## Timeline System

`Assets/Clouds/Clouds.Timeline/` — custom Unity Timeline tracks.

| Class | Role |
|---|---|
| `TriggerTrack` | Timeline track binding to `TriggerableEnvironment`; calls `Trigger()` / `TriggerOut()` |
| `TriggerableEnvironment` | Abstract base — implement `Trigger()` and `TriggerOut()` on scene objects |
| `UpdateableTrack` | Timeline track binding to `TimeLineUpdateObj`; calls `UpdateinTimLine()` per frame |
| `MyPlayableClip` | Custom clip asset for trigger tracks |

## Editor Tools

| Tool | Location | Purpose |
|---|---|---|
| `UIAnimationContainerEditor` | `Editor/` | Play/Stop per key + edit-mode DOTween preview |
| `DOTweenPreviewer` | `Editor/` | Wraps `DOTweenEditorPreview` for edit-mode animation preview |
| `MissingScriptFinder` | `Editor/` | Finds GameObjects with missing script references |
| `TMPFontChecker` | `Editor/` | Validates TextMeshPro font asset references |
| `Show2DArrayDrawer` | `Editor/` | PropertyDrawer for `Serializable2DArray<T>` |
| `SerializableDictionaryDrawer` | `Editor/` | PropertyDrawer for `SerializableDictionary<K,V>` |

## Common Utilities

**`InterfaceReference<T>`** (`Clouds.Common/InterfaceReference.cs`) — serializes interface references in Inspector.
```csharp
[SerializeField] InterfaceReference<IMyInterface> myRef;
myRef.Value.DoSomething();
```

**`ComponentExtensions`** (`Clouds.Common/ComponentExtensions.cs`):
```csharp
var rb = gameObject.GetOrAddComponent<Rigidbody>();
```

**`Serializable2DArray<T>`** (`Clouds.Common/Serializable2DArray.cs`) — Inspector-visible 2D grid. Use `EnsureSize()`, `ToRealArray()`, `SetFromRealArray()`.

## Coding Guidelines

- **Base class:** Extend `MyBehaviour`, override `LoadComponents()` to wire components. Never call `GetComponent` in `Update`.
- **Singletons vs. Services:** Use `Singleton<T>` only for MonoBehaviours that truly must live on a scene GameObject. If a system has no scene dependency, make it a static `...Service`/`...Repository<T>` instead (see Static Services vs. MonoBehaviour Managers above) — do not singleton-ify UI panels, gameplay objects, or anything that could be plain static state.
- **Messaging:** Use `SignalBus` for all cross-system communication. Use scoped `SignalBus.Scope<T>()` when signals should be contained to a specific panel or subsystem. Never call panel methods directly from button code.
- **Animation:** Attach `UIAnimationContainer` to the GameObject, assign `UIAnimationData` assets from `Assets/Clouds/Clouds.UI/AnimationPresets/`. Call `Play(key)` — do not create DOTween sequences manually outside the animation system.
- **DOTween:** Do not use DOTween directly for UI animations. Route through `UIAnimationContainer` → `DOTweenAnimationFactory`. Direct DOTween use is acceptable only for gameplay (non-UI) tweens.
- **Subscribe/Unsubscribe:** Always pair in `OnEnable`/`OnDisable`. Never subscribe in `Awake` or `Start` alone.
- **Odin Inspector:** Use `[ListDrawerSettings]`, `[HorizontalGroup]`, `[HideLabel]`, `[HideInInspector]` freely. Custom editors extend `OdinEditor`.
- **Serialization:** Use `SerializableDictionary` for Inspector-visible dictionaries. Use `Serializable2DArray<T>` for 2D grid data.
- **No base UI classes:** `baseUI`, `BaseButton`, `BasePopup`, `BaseSlider` have been removed. UI panels are plain `MonoBehaviour` subclasses that use `UIAnimationContainer` for animation and `SignalBus` for communication.
- **Component decomposition (Unity SRP):** Split complex GameObjects into focused components — one component per responsibility. A top-level coordinator script (e.g. `Player`) holds references and delegates; sub-components (e.g. `PlayerMovement`, `PlayerModel`, `PlayerDamageReceiver`) each own exactly one domain. Wire them in `LoadComponents()`. Never put movement, health, and visual logic all in the same class.

  ```
  Player (coordinator)
  ├── PlayerMovement   — input → velocity, physics
  ├── PlayerModel      — animator, skin swap, VFX
  └── PlayerDamageReceiver — hit detection, HP, death signal
  ```

  The coordinator calls `movement.Move(dir)`, `model.PlayAnim("Hit")`, etc. Sub-components never reference each other directly — they communicate through the coordinator or `SignalBus`.
- **Open/Closed:** Extend behavior by adding new implementations, not modifying existing classes. Add a new `DeSpawnStrategy` subclass instead of adding an `if/else` to `PoolService`. Create a new `UIAnimationData` SO asset instead of forking `UIAnimationContainer`. Open for extension, closed for modification.
- **Liskov Substitution:** Any class implementing an interface must fully honor its contract — no "not implemented" stubs. Keep interfaces small (see Interface Segregation) so every implementor uses every member. Prefer composition over inheritance; the removal of `BaseButton`/`BasePopup` hierarchies reflects this.
- **Interface Segregation:** Define many small, focused interfaces rather than one large one. Existing pattern: `IUIAnimation`, `IUIAnimationFactory`, `IUISetData`, `IDespawnable` — each is a single-concern contract. Never add unrelated methods to an existing interface; create a new one instead. Don't add a speculative interface with no implementer — `IObjectPooler`/`IPoolable` were removed for exactly this (defined, never implemented, never consumed).
- **Dependency Inversion:** Depend on abstractions. Use `SignalBus` so publishers and subscribers only share a message struct — neither references the other directly. Use `InterfaceReference<T>` in the Inspector to inject collaborators via interface, not concrete type. Avoid `GetComponent<ConcreteClass>()` across system boundaries.

## AI Agent Behavior (Karpathy Principles)

### 1. Think Before Coding

State assumptions explicitly before writing any code. If the request is ambiguous, present multiple interpretations and ask which is intended — do not silently pick one. Surface confusion early rather than delivering a wrong implementation.

### 2. Simplicity First

Write the minimum code that solves the stated problem. No speculative abstractions, no helper utilities for single-use code, no extra error handling for impossible cases. Ask: "Would a senior engineer consider this overcomplicated?" If yes, simplify.

### 3. Surgical Changes

Touch only what the task requires. Do not "improve" adjacent code, reformat unrelated lines, or rename things outside the scope of the request. Only remove imports or functions that *your* changes made obsolete — leave everything else as-is.

### 4. Goal-Driven Execution

Before implementing, define verifiable success criteria. State the plan explicitly, then execute in steps with checkpoints. Prefer writing a failing test or a concrete check first, then make it pass — avoid vague targets like "make it work".
