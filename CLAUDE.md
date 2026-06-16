# CLAUDE.md

This file provides coding guidance for AI agents working in this repository.

## Project Snapshot

`CloudsBase` is a Unity (URP) reusable game framework providing shared systems for:

- base component lifecycle (`MyBehaviour`, `Singleton<T>`),
- decoupled messaging (`SignalBus` — global and scoped pub/sub),
- UI animation (`UIAnimationContainer`, `DOTweenAnimationFactory`),
- panel management (`PanelManager`),
- data persistence (`LSManager`, `AbtractDataManager<T>`),
- object pooling and spawning (`Spawner<T>`, despawn strategies),
- hierarchical state machine (`State`, `Statemachine<T>`),
- physics helpers and Timeline integration.

The framework lives entirely under `Assets/CloudsBase/`. Game projects import or copy this folder and build on top of it.

## Project Structure

```
Assets/CloudsBase/
├── Common/          # InterfaceReference<T>, Tools (static utilities)
├── Data/            # DynamicData, SerializableDictionary, ExcelReader
├── Drawner/         # SerializableDictionaryDrawer (Odin property drawer)
├── Editor/          # Custom editors, DOTweenPreviewer, SignalGraph, SpawnerEnumGenerator
├── Helper/          # ComponentExtensions, DataHelper, Serializable2DArray
├── Manager/         # MyBehaviour, Singleton<T>, DataManager, AbtractDataManager, LSManager
├── Materials/       # Shared URP materials
├── Physic/          # PhysicUltilitis, SetAllRigidbody
├── Plugins/         # ExcelDataReader DLLs
├── Resources/       # UISetting.asset (loaded at runtime by name)
├── Shaders/         # Shared HLSL/ShaderGraph assets
├── Signal/          # SignalBus, IMessage (deprecated stub)
├── SignalSystem/    # IUIReceiver (deprecated stub)
├── Singleton/       # Singleton<T>
├── SO/              # Prebuilt UIAnimationData ScriptableObjects
├── Spawner/         # Spawner<T>, IDespawnable
├── State/           # State, Statemachine<T>
├── Strategy/        # DeSpawnStrategy, DeSpawnbyEvent, DeSpawnbytime_Strategy
├── Timeline/        # Custom Timeline tracks and behaviours
└── UI/
    ├── Data/        # UIAnimationData (SO), UISetting, UIData structs
    ├── Enums/       # UIEnums (TRIGGEREFFECT, MOVEEFFECT, etc.)
    ├── Interfaces/  # IUIAnimation, IUIAnimationFactory, IObjectPooler, IPoolable, etc.
    └── *.cs         # UIAnimationContainer, PanelManager, DOTweenAnimationFactory,
                     # UIHelper, UIUtility, FlexibleGridScaler, TextUIFormula, UIEffect, …
```

## Runtime Architecture

### Base Component — MyBehaviour

`Assets/CloudsBase/Manager/MyBehaviour.cs` — base class for all framework MonoBehaviours.

- Override `LoadComponents()` to auto-discover sibling/child components.
- Called automatically on `Reset()` (Editor) and `Awake()` (runtime).
- All concrete classes that need auto-wiring should extend `MyBehaviour`, not `MonoBehaviour`.

### Singleton

`Assets/CloudsBase/Singleton/Singleton.cs` — generic lazy singleton for MonoBehaviours.

- `Singleton<T>.Instance` auto-creates a GameObject if none exists.
- Prevents duplicates (destroys extras).
- Usage: `public class PanelManager : Singleton<PanelManager>`.

### Messaging — SignalBus

`Assets/CloudsBase/Signal/SignalBus.cs` — static pub/sub, no interface constraint on messages.

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

## UI System

### UIAnimationContainer

`Assets/CloudsBase/UI/UIAnimationContainer.cs` — attach to any UI GameObject to give it named animations.

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

`Assets/CloudsBase/UI/Data/UIAnimationData.cs` — holds `UIEffectData[]`. Each effect defines:
- `TRIGGEREFFECT` type: `Move`, `Rotate`, `Scale`, `Shake`, `Punch`, `Fade`, `Color`
- Delay, Duration, Ease, Loop settings
- Type-specific parameters (offsets, vectors, colors, etc.)

Prebuilt assets in `Assets/CloudsBase/SO/` (FadeIn, FadeOut, Bounce, Click, MoveUp, etc.).

### PanelManager

`Assets/CloudsBase/UI/PanelManager.cs` — Singleton managing a list of panel GameObjects.

- `[SerializeField] List<GameObject> ListPanels` — assign in Inspector or auto-discovers direct children.
- `GetPanelbyName(name)` → `Transform`, `DeActivePanel(name)`, `DeActiveAll()`, `ReturntoMainMenu()`.

Show/hide panels via `SetActive()` directly, or with animation:
```csharp
_anim.Play("Show", onStart: () => gameObject.SetActive(true));
_anim.Play("Hide", onComplete: () => gameObject.SetActive(false));
```

### UI Utilities

| File | Purpose |
|---|---|
| `UIHelper.cs` | Static helpers: anchor presets, scroll-to-child, grid utilities, time/number formatting |
| `UIUtility.cs` | Runtime effect helpers: fill text, change opacity |
| `FlexibleGridScaler.cs` | Responsive GridLayout scaling for varying screen sizes |
| `HorizontalLayoutResizer.cs` | Auto-resize parent to fit horizontal layout children |
| `TextUIFormula.cs` | TMP text measurement, number formatting (k/m/g suffixes) |
| `Tools.cs` | List shuffle, dictionary sort, recursive opacity, background resize tweens |

## Data & Persistence

### AbtractDataManager\<T\>

`Assets/CloudsBase/Manager/AbtractDataManager.cs` — base for data managers with async load gate.

- Implement `LoadGameDatas()` (load) and `SaveGame()` (save).
- `ISCOMPLETEDLOADDATA` static flag — other systems should wait until `true`.
- Coroutine `CrLoadGameData()` wraps load and sets flag when done.

### LSManager — Local Storage Manager

`Assets/CloudsBase/Manager/Lsmanager.cs` — JSON serialization to device and PlayerPrefs.

```csharp
LSManager.SaveDatatofile(data);
T data = LSManager.LoadDataFromJson<T>(jsonString);
T data = LSManager.LoadDataFromPlayerPref<T>("key");
string json = LSManager.DataToJson(obj);        // Unity JsonUtility
string json = LSManager.DatatoJsonConvert(obj); // Newtonsoft.Json
```

### DynamicData & SerializableDictionary

- `DynamicData` — abstract base for game data objects (has `Name` property). Extend for custom data.
- `SerializableDictionary<TKey, TValue>` — `Dictionary<K,V>` with Unity serialization via `ISerializationCallbackReceiver`.

### ExcelReader

`Assets/CloudsBase/Data/ExcelReader.cs` — reads `.xlsx`/`.xls` into `string[rows, cols]` using ExcelDataReader. Assets path is auto-converted to absolute path.

## State System

`Assets/CloudsBase/State/`

```csharp
var idle = new State { OnEnter = () => { }, OnExit = () => { }, OnUpdate = () => { } };
var run  = new State { ParentState = idle };  // hierarchical

var machine = new Statemachine<MyStateEnum>();
machine.Initialize(MyStateEnum.Idle);
machine.ChangeState((int)MyStateEnum.Run);
```

- `State` supports parent/child hierarchy — enter walks top-down, exit walks bottom-up.
- `Statemachine<T>` uses index to switch states; `CanChange` flag guards invalid transitions.

## Spawner & Object Pool

`Assets/CloudsBase/Spawner/Spawner.cs` — generic typed pool for prefab spawning.

```csharp
public class EnemySpawner : Spawner<Enemy> { }
```

- `Holder` — active objects parent, auto-found child named `"Holder"`.
- `Prefabs` folder — child named `"Prefabs"` is the template source.
- `Spawn(name, pos, rot)` — reuses pool or instantiates.
- `DeSpawnToPool(obj)` — returns to pool, deactivates.

**Despawn strategies** (`Assets/CloudsBase/Strategy/`):
```csharp
new DeSpawnbytime_Strategy(despawnable, 3f).Execute();    // after 3 seconds
new DeSpawnbyEvent(despawnable, ref myAction).Execute();  // when action fires
```

## Timeline System

`Assets/CloudsBase/Timeline/` — custom Unity Timeline tracks.

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
| `SignalGraphWindow` | `Editor/SignalGraph/` | Visual graph of `Button` → `UIAnimationContainer` signal flow |
| `SpawnerEnumGenerator` | `Editor/` | Auto-generates enum from spawner prefab names |
| `MissingScriptFinder` | `Editor/` | Finds GameObjects with missing script references |
| `TMPFontChecker` | `Editor/` | Validates TextMeshPro font asset references |
| `Show2DArrayDrawer` | `Editor/` | PropertyDrawer for `Serializable2DArray<T>` |

## Common Utilities

**`InterfaceReference<T>`** (`Common/InterfaceReference.cs`) — serializes interface references in Inspector.
```csharp
[SerializeField] InterfaceReference<IMyInterface> myRef;
myRef.Value.DoSomething();
```

**`ComponentExtensions`** (`Helper/ComponentExtensions.cs`):
```csharp
var rb = gameObject.GetOrAddComponent<Rigidbody>();
```

**`Serializable2DArray<T>`** (`Helper/Serializable2DArray.cs`) — Inspector-visible 2D grid. Use `EnsureSize()`, `ToRealArray()`, `SetFromRealArray()`.

## Coding Guidelines

- **Base class:** Extend `MyBehaviour`, override `LoadComponents()` to wire components. Never call `GetComponent` in `Update`.
- **Singletons:** Use `Singleton<T>` only for true manager-level classes (`PanelManager`, `DataManager`). Do not singleton-ify UI panels or gameplay objects.
- **Messaging:** Use `SignalBus` for all cross-system communication. Use scoped `SignalBus.Scope<T>()` when signals should be contained to a specific panel or subsystem. Never call panel methods directly from button code.
- **Animation:** Attach `UIAnimationContainer` to the GameObject, assign `UIAnimationData` assets from `Assets/CloudsBase/SO/`. Call `Play(key)` — do not create DOTween sequences manually outside the animation system.
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
- **Open/Closed:** Extend behavior by adding new implementations, not modifying existing classes. Add a new `DeSpawnStrategy` subclass instead of adding an `if/else` to `Spawner`. Create a new `UIAnimationData` SO asset instead of forking `UIAnimationContainer`. Open for extension, closed for modification.
- **Liskov Substitution:** Any class implementing an interface must fully honor its contract — no "not implemented" stubs. Keep interfaces small (see Interface Segregation) so every implementor uses every member. Prefer composition over inheritance; the removal of `BaseButton`/`BasePopup` hierarchies reflects this.
- **Interface Segregation:** Define many small, focused interfaces rather than one large one. Existing pattern: `IUIAnimation`, `IUIAnimationFactory`, `IObjectPooler`, `IPoolable`, `IDespawnable` — each is a single-concern contract. Never add unrelated methods to an existing interface; create a new one instead.
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
