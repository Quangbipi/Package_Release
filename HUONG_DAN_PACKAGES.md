# Hướng dẫn chi tiết các package `com.quang.*`

> Bản phát hành: `release-2026.10.11.1` · Unity tối thiểu: **2022.3** (khuyến nghị Unity 6)
>
> Tài liệu này giải thích, cho **từng package** trong repo:
> 1. **Tác dụng** – package giải quyết vấn đề gì, khi nào nên dùng.
> 2. **Chức năng** – các thành phần / API chính bên trong.
> 3. **Cách dùng** – các bước cài đặt, cấu hình và ví dụ code.
>
> README gốc của từng package (trong `Packages/<tên>/README.md`) vẫn là nguồn chính xác nhất cho chi tiết kỹ thuật.

---

## Mục lục

- [0. Tổng quan & cài đặt](#0-tổng-quan--cài-đặt)
  - [Sơ đồ phân tầng (layer)](#sơ-đồ-phân-tầng-layer)
  - [Cách cài package vào project Unity](#cách-cài-package-vào-project-unity)
  - [Thư viện bên thứ ba cần có](#thư-viện-bên-thứ-ba-cần-có)
- **Layer 0 – Nền tảng**
  - [1. com.quang.designpattern](#1-comquangdesignpattern--design-pattern-dùng-chung)
  - [2. com.quang.utilities](#2-comquangutilities--tiện-ích-cấp-thấp)
  - [3. com.quang.framework](#3-comquangframework--tài-liệu-ai-cho-dự-án)
- **Layer 1 – Lõi**
  - [4. com.quang.persistence](#4-comquangpersistence--lưu-trữ-dữ-liệu-an-toàn)
  - [5. com.quang.base](#5-comquangbase--lõi-runtime)
  - [6. com.quang.sync](#6-comquangsync--đồng-bộ-thao-tác-với-server)
  - [7. com.quang.sync.simulator](#7-comquangsyncsimulator--server-giả-lập-chỉ-dùng-khi-dev)
- **Layer 2 – Dữ liệu**
  - [8. com.quang.data](#8-comquangdata--framework-dữ-liệu)
- **Layer 3 – Gameplay**
  - [9. com.quang.character](#9-comquangcharacter--khung-nhân-vật--ai)
- **Layer 4 – UI, Services, LiveOps**
  - [10. com.quang.ui](#10-comquangui--framework-giao-diện)
  - [11. com.quang.services.analytics](#11-comquangservicesanalytics--analytics-đa-backend)
  - [12. com.quang.services.ads](#12-comquangservicesads--quảng-cáo)
  - [13. com.quang.services.iap](#13-comquangservicesiap--mua-hàng-trong-ứng-dụng)
  - [14. com.quang.liveops.dailygift](#14-comquangliveopsdailygift--quà-đăng-nhập-hằng-ngày)
  - [15. com.quang.liveops.energy](#15-comquangliveopsenergy--năng-lượng--mạng-chơi)
- **Layer 6 – Công cụ phát triển**
  - [16. com.quang.tools](#16-comquangtools--công-cụ-editor)
  - [17. com.quang.autotest](#17-comquangautotest--kiểm-thử-tự-động)
- [Phụ lục: Chọn package theo loại game](#phụ-lục-chọn-package-theo-loại-game)

---

## 0. Tổng quan & cài đặt

### Bảng tóm tắt nhanh

| Package | Phiên bản | Layer | Một câu mô tả |
|---|---|---|---|
| `com.quang.designpattern` | 0.4.5 | 0 | Pool, EventBus, StateMachine, Singleton, Observer, Command… |
| `com.quang.utilities` | 0.2.2 | 0 | Hàm tiện ích, Math, JSON (LitJson), Timer (STimer) |
| `com.quang.framework` | 0.1.2 | 0 | Chỉ chứa tài liệu/rule/skill cho AI (Claude/Cursor), không có code |
| `com.quang.persistence` | 0.1.0 | 1 | Cơ chế lưu file an toàn: atomic write, backup, HMAC, migration |
| `com.quang.base` | 0.26.0 | 1 | `Locator` + toàn bộ interface service, init flow, Stats, GameData |
| `com.quang.sync` | 0.1.0 | 1 | Hàng đợi đồng bộ thao tác lên server (idempotent, retry, conflict) |
| `com.quang.sync.simulator` | 0.1.0 | 1 | Server giả lập trong bộ nhớ để test `sync` — **chỉ dùng dev/test** |
| `com.quang.data` | 0.12.6 | 2 | ScriptableObject data, ItemCatalog, DataManager, bootstrap lưu trữ |
| `com.quang.character` | 0.2.8 | 3 | Nhân vật dạng module + BrainGraph (AI node-graph có editor riêng) |
| `com.quang.ui` | 0.8.0 | 4 | `UIManager`, vòng đời canvas, hiệu ứng chuyển cảnh, widget |
| `com.quang.services.analytics` | 0.5.1 | 4 | Một proxy analytics gửi tới Firebase/AppsFlyer/AppMetrica/GameAnalytics |
| `com.quang.services.ads` | 0.8.3 | 4 | Quảng cáo Reward/Inter/Banner, tách vendor (LevelPlay/MAX/AdMob) |
| `com.quang.services.iap` | 0.7.5 | 4 | Pipeline mua hàng an toàn trên Unity Purchasing v5 |
| `com.quang.liveops.dailygift` | 0.4.8 | 4 | Tính năng quà đăng nhập hằng ngày + popup |
| `com.quang.liveops.energy` | 0.2.8 | 4 | Hệ thống năng lượng/mạng chơi an toàn giao dịch |
| `com.quang.tools` | 0.11.0 | 6 | Bộ tool Editor: UI flow, Spine bake, xử lý texture, dọn asset… |
| `com.quang.autotest` | 0.4.4 | 6 | Framework test tự động theo kịch bản, chạy được qua CLI/CI |

### Sơ đồ phân tầng (layer)

Quy tắc: **package ở layer cao chỉ được phụ thuộc vào layer thấp hơn**, không bao giờ ngược lại.

```text
Layer 6  ┌──────────────┐  ┌───────────────┐
(Tools)  │ tools        │  │ autotest      │   ← độc lập, không phụ thuộc com.quang.*
         └──────────────┘  └───────────────┘
Layer 4  ┌─────┐ ┌───────────┐ ┌─────┐ ┌─────┐ ┌───────────┐ ┌────────┐
         │ ui  │ │ analytics │ │ ads │ │ iap │ │ dailygift │ │ energy │
         └──┬──┘ └─────┬─────┘ └──┬──┘ └──┬──┘ └─────┬─────┘ └───┬────┘
Layer 3     │    ┌─────┴─────┐    │       │          │           │
            │    │ character │    │       │          │           │
Layer 2     │    └─────┬─────┘    │       │          │           │
            │      ┌───┴───────── data ───┴──────────┘           │
Layer 1     └──────┴──────────── base ─────────────────────────┘
                     │            │      sync ── sync.simulator
                     │            └──── persistence ──┘
Layer 0         designpattern · utilities · framework
```

### Cách cài package vào project Unity

Mở `Packages/manifest.json` trong project game và thêm package theo tag release:

```json
{
  "dependencies": {
    "com.quang.designpattern": "https://github.com/Quangbipi/Package_Release.git?path=Packages/com.quang.designpattern#release-2026.10.11.1",
    "com.quang.utilities":     "https://github.com/Quangbipi/Package_Release.git?path=Packages/com.quang.utilities#release-2026.10.11.1",
    "com.quang.persistence":   "https://github.com/Quangbipi/Package_Release.git?path=Packages/com.quang.persistence#release-2026.10.11.1",
    "com.quang.base":          "https://github.com/Quangbipi/Package_Release.git?path=Packages/com.quang.base#release-2026.10.11.1",
    "com.quang.data":          "https://github.com/Quangbipi/Package_Release.git?path=Packages/com.quang.data#release-2026.10.11.1",
    "com.quang.ui":            "https://github.com/Quangbipi/Package_Release.git?path=Packages/com.quang.ui#release-2026.10.11.1"
  }
}
```

**Lưu ý quan trọng:**

- Unity **không tự tải dependency** từ Git URL. Bạn phải **liệt kê tường minh mọi package `com.quang.*`** mà package bạn dùng phụ thuộc vào (xem cột "Phụ thuộc" ở mỗi mục bên dưới).
- Dùng **cùng một tag** cho tất cả package để phiên bản khớp nhau. Danh sách phiên bản trong từng release nằm ở [release-index.json](release-index.json).
- Các package Unity chính thức (`com.unity.inputsystem`, `com.unity.addressables`, `com.unity.purchasing`, `com.unity.nuget.newtonsoft-json`) được Unity Registry tự cài theo `package.json`.

### Thư viện bên thứ ba cần có

Một số package cần asset bên ngoài (không có trên Unity Registry), phải import thủ công:

| Thư viện | Cần cho |
|---|---|
| **DOTween** (Asset Store) | `ui`, `character`, `tools` |
| **Odin Inspector** (Sirenix) | `ui` (chỉ dùng attribute) và một số drawer trong `data` |
| **TextMeshPro** | `base`, `ui`, `dailygift` (Unity 6 đã tích hợp sẵn) |
| **Spine Unity** | `base` (nợ kỹ thuật, dự kiến gỡ), `tools` (Spine bake) |
| SDK quảng cáo: LevelPlay/ironSource, AppLovin MAX, Google Mobile Ads 9.1.0 | `services.ads` |
| SDK analytics: Firebase, AppsFlyer, AppMetrica, GameAnalytics | `services.analytics` (chỉ cái nào bạn bật) |

---

# LAYER 0 – NỀN TẢNG

## 1. `com.quang.designpattern` – Design pattern dùng chung

**Phụ thuộc:** không có. · **Namespace:** `Quang.DesignPattern`

### Tác dụng
Cung cấp các mẫu thiết kế (design pattern) hay dùng trong game Unity, để mọi dự án dùng chung một cách viết thay vì mỗi game tự viết lại Pool, Singleton, EventBus…

### Chức năng

| Thư mục | Thành phần | Dùng để |
|---|---|---|
| `Pool/` | `SimplePool`, `GameUnit`, `ParticlePool`, `MiniPool`, `PoolController` | Object pooling – tái sử dụng GameObject thay vì `Instantiate/Destroy` liên tục (đạn, enemy, hiệu ứng) |
| `EventBus/` | `EventBus<T>`, `EventBinding<T>`, `IEvent` | Gửi/nhận sự kiện kiểu an toàn (type-safe) giữa các hệ thống mà không cần tham chiếu trực tiếp |
| `StateMachine/` | `StateMachine`, `BaseState` | Máy trạng thái cho AI, flow game, nhân vật |
| `Singleton/` | `Singleton<T>`, `SimpleSingleton<T>` | MonoBehaviour singleton truy cập qua `.Ins` |
| `Observe/` | `ISubject`, `IObserver`, `Dispatcher` | Mẫu Observer cổ điển |
| `Command/` | `Command` | Mẫu Command (undo/redo, hàng đợi lệnh) |
| `SavingState/` | `IMemento`, `IOriginator` | Mẫu Memento – chụp/khôi phục trạng thái |
| `MonoConstructor/` | `IConstructor` | Khởi tạo MonoBehaviour có tham số |

### Cách dùng

**Object Pool:**

```csharp
using Quang.DesignPattern;

// 1. Prefab kế thừa GameUnit
public class Bullet : GameUnit
{
    public override void OnDespawn() { /* reset trạng thái */ }
}

// 2. Preload khi vào màn (tuỳ chọn)
SimplePool.Preload(bulletPrefab, qty: 20, parent: poolRoot);

// 3. Lấy ra / trả về
Bullet b = SimplePool.Spawn<Bullet>(bulletPrefab, firePoint.position, Quaternion.identity);
b.Despawn();            // trả về pool (extension method)
SimplePool.CollectAll(); // thu hồi tất cả khi kết thúc màn
```

**EventBus:**

```csharp
public struct PlayerDiedEvent : IEvent { public int Score; }

EventBinding<PlayerDiedEvent> binding;

void OnEnable()
{
    binding = new EventBinding<PlayerDiedEvent>(e => ShowGameOver(e.Score));
    EventBus<PlayerDiedEvent>.Subscribe(binding);
}
void OnDisable() => EventBus<PlayerDiedEvent>.Unsubscribe(binding);

// Ở nơi khác:
EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent { Score = 120 });
```

**Singleton:**

```csharp
public class SoundManager : Singleton<SoundManager> { public void Play(string id) { } }
SoundManager.Ins.Play("click");
```

> Luôn `Unsubscribe` trong `OnDisable/OnDestroy` để tránh leak. Trong test có thể gọi `EventBusRegistry.ClearAllBindings()`.

---

## 2. `com.quang.utilities` – Tiện ích cấp thấp

**Phụ thuộc:** `designpattern`. · **Assembly:** `Quang.Utilities`, `Quang.Utilities.Json`, `Quang.Utilities.STimer`

### Tác dụng
Gom các hàm tiện ích nhỏ, toán học, JSON và bộ hẹn giờ để không phải dùng Coroutine/`Invoke` rải rác.

### Chức năng

| Thành phần | Mô tả |
|---|---|
| `Math/MathHelper`, `EdgeHelpers`, `SRandom`, `LinearRegression` | Hàm toán học, random có seed, hồi quy tuyến tính |
| `STimer/TimerManager`, `STimer` | Hẹn giờ theo thời gian / theo frame, lặp, hỗ trợ unscaled time |
| `ThirdParty/LitJson` (`Quang.Utilities.Json`) | Thư viện JSON LitJson (`JsonMapper`, `JsonData`) |
| `MainThreadDispatcher` | Đẩy callback từ thread khác (SDK) về main thread của Unity |
| `VFXController`, `VFXObject`, `VFXColor`, `Gradient`, `PosYToSortingOrder` | Tiện ích hiển thị: điều khiển VFX, gradient UI, sắp sorting order theo trục Y (game 2D top-down) |

### Cách dùng

```csharp
using Quang.Utilities.Timer;

// Gọi hàm sau 2 giây
TimerManager.Ins.WaitForTime(2f, () => SpawnBoss());

// Gọi hàm sau 2 giây, không bị ảnh hưởng khi Time.timeScale = 0 (đang pause)
TimerManager.Ins.WaitForTime(2f, ShowReward, isUnscaleTime: true);

// Đợi 3 frame
TimerManager.Ins.WaitForFrame(3, RefreshLayout);

// Lặp lại 5 lần, mỗi 0.5 giây
TimerManager.Ins.TriggerLoopAction(Shoot, 0.5f, num: 5);

// Dùng STimer riêng để đếm ngược
STimer timer = TimerManager.Ins.PopSTimer();
timer.TimeOut += code => Debug.Log("Hết giờ");
timer.Start(10f);
// ... khi không cần nữa:
TimerManager.Ins.StopTimer(ref timer);
```

---

## 3. `com.quang.framework` – Tài liệu AI cho dự án

**Phụ thuộc:** không có. · **Không chứa code C#.**

### Tác dụng
Là package **chỉ chứa tài liệu** dành cho trợ lý AI (Claude Code, Cursor): các quy tắc viết code, quy trình, skill dùng chung cho mọi dự án xây trên ComQuang framework. Giúp AI hiểu đúng kiến trúc khi làm việc trong project game.

### Chức năng

| Thư mục | Được nạp | Nội dung |
|---|---|---|
| `AI~/rules/` | luôn luôn (thành `.claude/rules/pkg-framework-*.md`) | Quy tắc compile-gate, MCP, test, bug-ledger, C# style |
| `AI~/knowledge/` | khi cần | Kiến thức "cái gì đúng và vì sao" |
| `AI~/playbooks/` | khi cần | Quy trình từng bước |
| `AI~/tools/`, `AI~/skills/`, `AI~/hooks/` | thành `.claude/tools|skills|hooks/pkg-framework-*` | Script và skill (vd. `find-bug`, `unity-csproj-sync`) |

Thư mục có dấu `~` (`AI~`) bị Unity bỏ qua, nên không ảnh hưởng build.

### Cách dùng
1. Thêm package vào `manifest.json`.
2. Chạy script `sync.ps1` của bộ `claude-unity-shared` để copy nội dung `AI~` vào thư mục `.claude/` của project.
3. Giá trị riêng của từng project đặt ở `.claude/rules/project-values.md`.
4. **Không sửa** các bản copy đã sinh trong project — muốn đổi thì sửa trong package, tăng version, release lại.

---

# LAYER 1 – LÕI

## 4. `com.quang.persistence` – Lưu trữ dữ liệu an toàn

**Phụ thuộc:** `com.unity.nuget.newtonsoft-json`. · **Không gọi bất kỳ API Unity nào.**

### Tác dụng
Lo phần **"lưu byte xuống đĩa sao cho an toàn"**: không mất save khi app bị kill giữa chừng, phát hiện file bị sửa/hỏng, nâng cấp schema save qua các phiên bản. Package chỉ là **cơ chế**, không chứa dữ liệu game nào (không biết "vàng", "level" là gì).

### Chức năng

| Nhóm | Thành phần chính | Ý nghĩa |
|---|---|---|
| Hợp đồng | `IPersistenceService`, `SaveDefinition<T>`, `ISaveStore`, `ISaveCodec`, `ISaveProtector`, `ISaveMigration`, `LoadResult<T>`, `SaveResult` | Giao diện trừu tượng để thay thế/kiểm thử dễ |
| Lưu file | `FileSaveStore` | Ghi atomic (temp → primary), xoay vòng backup, cách ly (quarantine) file hỏng |
| Mã hoá payload | `PlainJsonSaveCodec`, `GZipSaveCodec`, `BeneficialCompressionCodec` | JSON thường / nén GZip / chỉ nén khi có lợi |
| Chống sửa file | `Sha256SaveProtector`, `HmacSha256SaveProtector`, `LocalSecretKeyProvider` | Phát hiện save bị chỉnh sửa hoặc hỏng |
| Migration | `SaveEnvelope`, `SaveMigrationRegistry` | Đóng gói payload có version, chuỗi nâng cấp schema |
| Nhập save cũ | `ILegacySaveSource`, `FileLegacySaveSource` | Đọc một lần từ định dạng cũ (vd. PlayerPrefs) |
| Lắp ráp | `PersistenceBuilder` | Fluent builder tạo `PersistenceService` |

### Cách dùng
Thông thường **bạn không dùng trực tiếp** package này: cài `com.quang.data` thì nó tự cấu hình mặc định trước khi load scene (xem mục [8](#8-comquangdata--framework-dữ-liệu)). Dùng trực tiếp khi:

- Viết package/tính năng mới cần lưu dữ liệu → nhận `IPersistenceService` qua tham số và khai báo một `SaveDefinition<T>` (key, schema version, validator, migration).
- Viết test → ghép `PersistenceService` với store trong bộ nhớ.
- Chọn `SaveFailurePolicy.FailClosed` cho dữ liệu nhạy cảm (ledger IAP, hàng đợi sync): lỗi thì **dừng**, không tự reset.

---

## 5. `com.quang.base` – Lõi runtime

**Phụ thuộc:** `designpattern`, `utilities`, `persistence`, `newtonsoft-json`, `inputsystem`. · **Namespace chính:** `Quang.Base`

### Tác dụng
Là **trái tim** của framework. Mọi package khác đều dựa vào nó. Nó định nghĩa:
- **Service Locator** (`Locator`) – điểm truy cập toàn cục tới các dịch vụ (Ads, IAP, UI, Audio…).
- **Hợp đồng (interface)** trung lập cho mọi dịch vụ, để game code không phụ thuộc SDK cụ thể.
- Luồng khởi động game, hệ thống chỉ số (Stats), dữ liệu người chơi (`GameData`), Pause.

### Chức năng

**a) Locator – các "slot" dịch vụ:**

| Slot | Interface | Được gán bởi |
|---|---|---|
| `Locator.UI` | `IUIService` | `com.quang.ui` |
| `Locator.Ads`, `Locator.AdsEntitlements` | `IAdsService`, `IAdsEntitlements` | `com.quang.services.ads` |
| `Locator.Analytics`, `Locator.RevenueSink` | `IAnalyticsService`, `IRevenueEventSink` | `com.quang.services.analytics` |
| `Locator.IAPService` | `IIAPService` (cũ) | `com.quang.services.iap` |
| `Locator.DailyGift` | `IDailyGiftService` | `com.quang.liveops.dailygift` |
| `Locator.Items` | `IItemService` | game / `data` |
| `Locator.Pause` | `IPauseService` | `PauseService` |
| `Locator.Input`, `Audio`, `Level`, `Gameplay`, `Tutorial`, `DailyReward`, `SpinWheel`, `PiggyBank` | … | do game tự cài đặt |

**b) Các nhóm khác:**

| Thành phần | Mô tả |
|---|---|
| `Init/InitManager`, `LoadStart`, `InitCanvas`, `DebugManager` + prefab `Init.prefab`, `DebugManager.prefab`, `Canvas_Debug.prefab` | Luồng khởi động game và màn debug |
| `Stats/Stat`, `StatModifier`, `TrackingStat` | Chỉ số có modifier (cộng phẳng, % …) cho máu, sát thương, tốc độ… |
| `GameData` | Dữ liệu người chơi; key lưu `GameData.item-id-v1` |
| `ItemId` | Định danh vật phẩm dạng chuỗi có namespace (`base.gold`, `mygame.gem`) |
| `PauseService`, `PauseLease` | Pause theo "hợp đồng thuê": nhiều hệ thống cùng pause, mỗi bên chỉ nhả phần của mình |
| `IClock`, `SystemClock`, `RewardDayPolicy`, `RewardClaimId` | Đồng hồ UTC và cơ chế nhận thưởng theo ngày không bị nhận trùng |
| Hợp đồng mua hàng: `PurchaseProductId`, `IPurchaseIntegrityService`, `IPurchaseGrantHandler` | Trung lập vendor, không tham chiếu SDK store |
| Hợp đồng ads: `AdsShowRequest`, `AdsShowResult`, `IAdsRequestService` | Kết quả hiển thị quảng cáo theo từng request |
| UI scope: `UiScope`, `UiScopeRegistry`, `UICanvas` | Tự đóng canvas khi rời "phạm vi" (vd. rời màn gameplay) |
| `Database` | Facade tương thích cho lưu/đọc (`Database.Save/Load`) |
| `Utilities/Physics` (`AOEDetection`, `Target2D/3DDetection`, `Cube`) | Dò mục tiêu vùng ảnh hưởng (AOE) |
| `Gameplay/CharacterContracts` (`ICharacter`, `IDamageTarget`, `IProjectile`…) | Interface chung cho nhân vật/đạn/sát thương |
| Assembly phụ `Quang.Utilities.Input` | `PlayerInput`, `LineDrawInput` |

### Cách dùng

**Gọi dịch vụ từ game code (không cần biết SDK nào phía sau):**

```csharp
using Quang.Base;

Locator.Analytics.LogEvent("open_shop", AnalyticsCategory.Design, null);
Locator.UI.CloseUIDirectly(someCanvas);
```

**Pause theo lease:**

```csharp
// PauseLeaseKind: Gameplay, Popup, Tutorial, Ads, Application, Debug
var id = PauseLeaseId.Create(PauseLeaseKind.Popup, owner: "ShopPopup", nonce: "1");
Locator.Pause.Acquire(new PauseLease(id, PauseLeaseKind.Popup, "ShopPopup"));
// ... khi popup đóng
Locator.Pause.Release(id); // chỉ nhả pause của popup, không ảnh hưởng pause của Ads/Tutorial
// hoặc nhả mọi lease của một owner:
Locator.Pause.ReleaseOwner("ShopPopup");
```

**Lưu/đọc dữ liệu (cách cũ, vẫn hỗ trợ):**

```csharp
Database.Save(gameData, GameData.SaveKey);
GameData loaded = Database.Load<GameData>(GameData.SaveKey);
```

**Khởi tạo item lúc start:**

```csharp
gameData.InitData(catalogItemIds); // thêm ID mới, giữ nguyên ID đã lưu
var info = Locator.Items.GetPresentation(new ItemId("base.gold")); // icon, tên, độ hiếm
```

**Lưu ý:**
- Nếu project chỉ cài `base` mà **không** cài `data`, phải tự gán `Database.ServiceFactory` và `Database.CompatibilityDefinitionFactory` (không có fallback sang PlayerPrefs).
- Thưởng theo ngày phải lưu `RewardDayKey` đầy đủ, **không** lưu "ngày trong năm".
- `RewardClaimId` phải tạo từ dữ liệu ổn định; **không** dùng `Guid.NewGuid()` cho claim có thể retry.

---

## 6. `com.quang.sync` – Đồng bộ thao tác với server

**Phụ thuộc:** `persistence`, `newtonsoft-json`. · **Không gọi API Unity.**

### Tác dụng
Khi game có server, cần gửi các **thao tác** (mua đồ, nhận thưởng, cộng điểm) lên server một cách tin cậy, kể cả khi mất mạng. `sync` **không phải** là nơi lưu save; nó trả lời câu hỏi: *"thao tác này đã được xác thực, chấp nhận, từ chối, retry, hay đối chiếu chưa?"*

### Chức năng

| Thành phần | Mô tả |
|---|---|
| `SyncOperation` | Gói thao tác: id ổn định, stream key, revision mong đợi, payload |
| `SyncResult`, `SyncResultKind` | Kết quả: `Accepted`, `DuplicateAccepted`, `RejectedBusinessRule`, `RevisionConflict`, `AuthenticationRequired`, `RetryableTransportFailure`, `PermanentProtocolFailure` |
| `SyncQueue` | Hàng đợi chờ gửi, lưu bền qua `persistence` (sống sót khi tắt app) |
| `SyncEngine` | Điều phối gửi, đối chiếu, xử lý offline |
| `SyncRetryClassifier` | Ánh xạ lỗi transport → loại kết quả |
| `ISyncTransport`, `ISyncAuthProvider`, `ISyncClock`, `ISyncDiagnostics` | Cổng (port) – **bạn tự cài đặt** phần gửi HTTP/socket và xác thực |

### Cách dùng
1. Cài đặt `ISyncTransport` (gửi một operation → nhận một `SyncResult`) và `ISyncAuthProvider` cho backend của bạn.
2. Tạo `SyncQueue` + `SyncEngine` với các port đó.
3. Mỗi ý định nghiệp vụ mới → tạo `SyncOperation` với **id mới**; khi retry → **dùng lại id cũ**.

**Quy tắc bắt buộc:** không log/lưu payload hay token; conflict **không** giải quyết kiểu "ghi sau thắng"; queue dùng `FailClosed`.

---

## 7. `com.quang.sync.simulator` – Server giả lập (chỉ dùng khi dev)

**Phụ thuộc:** `sync`. · ⚠️ **TUYỆT ĐỐI KHÔNG tham chiếu từ assembly runtime production.**

### Tác dụng
Cho phép phát triển và test tính năng đồng bộ **khi chưa có server thật**, và mô phỏng các tình huống khó tái hiện (mất mạng, hết hạn token, xung đột).

### Chức năng
`SyncSimulatorServer` + `SimulatorTransport` cài đặt `ISyncTransport`/`ISyncAuthProvider` trong bộ nhớ với: nhiều tài khoản, revision theo stream, chống trùng theo operation id, độ trễ cấu hình được, chế độ offline, hết hạn auth, từ chối theo luật nghiệp vụ, ép xung đột revision, kịch bản lỗi (`SyncFaultScript`), API reset/kiểm tra.

### Cách dùng
- Tham chiếu chỉ từ assembly **Tests** hoặc assembly **Editor/dev**.
- Truyền `SimulatorTransport` vào `SyncEngine` thay cho transport thật, rồi dùng `SyncFaultScript` để tạo tình huống lỗi trong test.
- Trạng thái chỉ nằm trong RAM, tắt là mất.

---

# LAYER 2 – DỮ LIỆU

## 8. `com.quang.data` – Framework dữ liệu

**Phụ thuộc:** `designpattern`, `utilities`, `base`, `persistence`, `newtonsoft-json`. · **Assembly phụ:** `Quang.Common`

### Tác dụng
Quản lý **dữ liệu thiết kế game** (ScriptableObject), **danh mục vật phẩm**, và **cấu hình lưu trữ mặc định** cho production. Cài package này là save game hoạt động ngay, không cần tự ghép.

### Chức năng

| Thành phần | Mô tả |
|---|---|
| `Runtime/DataSO`, `Runtime/Base` | Định nghĩa dữ liệu dạng ScriptableObject, `DataManager` |
| `Runtime/Items` + `ItemCatalog.asset` | Danh mục vật phẩm với `ItemId` ổn định |
| `Editor/Items` | Validator catalog, code generator sinh hằng số `ItemId`, dropdown chọn ItemId có tìm kiếm trên Inspector |
| `Runtime/Persistence` | `PersistenceBootstrap` – tự cài trước khi load scene; lưu vào `persistentDataPath/ComQuang/Saves`, khoá HMAC ở `.../ComQuang/Keys` |
| `Runtime/LiveOps` | Interface cấu hình LiveOps: `IDailyGiftConfig`, `IDailyGiftDay`, `IDailyGiftReward`, cùng Heart/DailyReward/SpinWheel/PiggyBank |
| `Runtime/Rewards`, `Runtime/Character` | Dữ liệu thưởng, dữ liệu nhân vật |
| `Common/Barrier` (`ConditionBarrier`) | Rào điều kiện – chờ nhiều điều kiện cùng thoả |
| `Editor/Compatibility` | Drawer enum tương thích Unity 6 + Odin |

### Cách dùng

**Lưu trữ:** chỉ cần cài package. `PersistenceBootstrap.InstallDefault()` tự chạy. Chỉ gọi tay nếu code cần dùng `Database` sớm hơn lúc Unity khởi tạo runtime.

**Item Catalog:**
1. Mở `ItemCatalog.asset`, thêm item với `ItemId` ổn định. Item gốc dùng namespace `base.`, item của game dùng namespace riêng (vd. `pet_vs_monster.gem`).
2. Chạy **validator** trước khi release (bắt trùng ID, trùng code name, null, ID sai định dạng).
3. Chạy **ItemId code generator** để sinh hằng số tiện dùng trong code.
4. **Không đổi tên ID** nếu không có kế hoạch migrate save – ID là khoá được serialize.

**Composition thủ công khi viết test:**

```csharp
var codec = new PlainJsonSaveCodec();
var protector = new Sha256SaveProtector();
var service = new PersistenceService(testStore, testLegacySource, testDiagnostics);
PackageSaveDefinitions.RegisterAll(service, codec, protector);

Database.CompatibilityDefinitionFactory =
    new CompatibilitySaveDefinitionFactory(codec, protector, testDiagnostics);
Database.ServiceFactory = () => service;
```

---

# LAYER 3 – GAMEPLAY

## 9. `com.quang.character` – Khung nhân vật + AI

**Phụ thuộc:** `designpattern`, `utilities`, `base`, `data`, `inputsystem`. · **Cần:** DOTween. · **Namespace:** `Gameplay.Character.*` (không phải `Quang.Character`)

### Tác dụng
Khung dựng nhân vật (player, enemy, NPC) theo kiểu **module ghép**: mỗi mảng logic/vật lý/cảm biến/AI/hình ảnh là một component riêng. Kèm **BrainGraph** – hệ AI dạng node graph **có editor riêng**, không cần package graph ngoài (không cần `com.unity.behavior`).

### Chức năng

| Thành phần | Mô tả |
|---|---|
| `Character<TDef, TStats>` | Gốc nhân vật, ghép các system; `OnInit(TDef)` để inject dữ liệu |
| `CharacterDefinition` / `CharacterStats` | Dữ liệu tĩnh / chỉ số runtime – bạn kế thừa |
| `LogicSystem`, `LogicModule`, `BaseLogicState` | Pipeline logic + state machine của nhân vật |
| `Physic/` | Module vật lý di chuyển |
| `WorldInterface/Sensor` (`BaseSensor`, 2D/3D: vision, detect sound, ground edge, multi-raycast, scan) | Cảm biến nhận thức môi trường |
| `BrainModule`, `BrainData`, `BrainParameter` | Điểm nối AI/điều hướng |
| `PlayerInput` | Brain điều khiển bằng Input System, dùng asset `.inputactions` của bạn |
| `BrainGraphAsset`, `BrainGraphModule`, `BrainGraphRunner` + Editor | AI node graph |
| `Navigation/`, `Visual/`, `SubModule/Sound` | Điều hướng, hiển thị, âm thanh |
| `RegenStatModule`, `Trigger` | Hồi chỉ số theo thời gian, relay sự kiện trigger collider |

### Cách dùng
1. Tạo cặp `MyDefinition : CharacterDefinition` và `MyStats : CharacterStats`.
2. Tạo class kế thừa `Character<MyDefinition, MyStats>`, gắn vào GameObject, thêm các module cần thiết (Logic, Physic, WorldInterface, Brain, Visual) – mỗi cái là một component trên cùng object.
3. **Nhân vật do người chơi điều khiển:** thêm `PlayerInput`, gán asset `.inputactions` vào field `actions`. Mặc định cần action map `"Movement"` với `Move/Look/Jump/Crouch/Run` (đổi tên được trong Inspector). `PlayerInput` **không tự enable** action map – game tự bật/tắt (vd. qua `Locator.Input`).
4. **Nhân vật AI:** thêm `BrainGraphModule`, gán `BrainGraphAsset`. Tạo graph qua:
   - Menu **Tools → Character → Create Default Enemy BrainGraph**, hoặc **Assets → Create → Gameplay → Character → Default Enemy BrainGraph**
   - Mở editor: **Tools → Character → BrainGraph Editor**
5. Spawn và inject dữ liệu: gọi `OnInit(definition)` **trước** khi object active (Awake sau đó sẽ bỏ qua).

**Quy tắc khi mở rộng:**
- Core **không chứa type riêng của game** – đặt `EnemyAI`, `PlayerDefinition`… trong assembly glue của game.
- Node BrainGraph **chỉ đọc `BrainParameter` và ghi `BrainData`** để graph dùng lại được giữa các game.
- Thay system/data bằng cách override các factory `Create*System` / `Create*Data`.

---

# LAYER 4 – UI, SERVICES, LIVEOPS

## 10. `com.quang.ui` – Framework giao diện

**Phụ thuộc:** `designpattern`, `utilities`, `base`, `com.unity.addressables`. · **Cần:** DOTween, Odin Inspector, TextMeshPro.

### Tác dụng
Quản lý toàn bộ màn hình/popup: mở, đóng, back stack, hiệu ứng xuất hiện/biến mất, nạp prefab từ Resources hoặc Addressables. Cung cấp sẵn nút, dropdown, scroll view tái sử dụng ô.

### Chức năng

| Nhóm | Thành phần | Mô tả |
|---|---|---|
| Facade | `UIManager` | `OpenUI<T>`, `CloseUI<T>`, `GetUI<T>`, `GetUIAsync<T>`, `AcquireAsync<T>(address)`, `CloseUIDirectly`, `IsContain`, `IsInBackStack` |
| Canvas | `UISCanvas`, `UICanvasComponent`, `BasePopup` | Vòng đời canvas, lớp cơ sở cho popup |
| Hiệu ứng | `UISCanvasTransition`, `UIAnim`, `UIAlphaAnim`, `UIScaleAnim`, `UIPositionAnim`, `UIPunchAnim` | Hiệu ứng SHOW/HIDE/IDLE, xếp hàng và ngắt an toàn |
| Nạp prefab | `IUIPrefabProvider`, `ResourcesPrefabProvider`, `AddressablesUIPrefabProvider`, `CanvasRegistry`, `AsyncCanvasRegistry`, `IUIAddressResolver` | Nạp từ Resources (mặc định) hoặc Addressables |
| Widget | `UIButton`, `UISButton`, `UIButtonComponent`, `ImageStateButtonComponent`, `TextsStateButtonComponent`, `Type4ButtonComponent`, `UIDropdown`, `UIItem`, `MaskRaycastImage` | Nút, nút đổi trạng thái, dropdown, ô item |
| Khác | `RecyclableScrollRect` (bên thứ ba), `UIParticleSystem`, `DetectMouse` | Scroll view tái sử dụng ô cho danh sách dài, particle trên UI |

### Cách dùng

**Tạo một popup:**

```csharp
using Quang.UI;

public class ShopPopup : BasePopup
{
    public override void Setup(object param) { base.Setup(param); /* đổ dữ liệu */ }
}
```

1. Tạo prefab canvas, gắn `ShopPopup`, thêm `UISCanvasTransition` và các `UIAnim` cho hiệu ứng.
2. Đặt prefab vào `Resources` (provider mặc định) hoặc đánh dấu Addressable.

**Mở/đóng:**

```csharp
UIManager.Ins.OpenUI<ShopPopup>(param: shopData);
UIManager.Ins.CloseUI<ShopPopup>();
ShopPopup shop = UIManager.Ins.GetUI<ShopPopup>();
```

**Dùng Addressables:**

```csharp
// Gọi trước lần acquire đầu tiên nếu muốn provider riêng
UIManager.Ins.SetAsyncProvider(myProvider /*, myAddressResolver */);
var result = await UIManager.Ins.AcquireAsync<ShopPopup>("UI/ShopPopup");
```

**Danh sách dài:** menu **GameObject → UI → Recyclable Scroll View**, cài `IRecyclableScrollRectDataSource` để cấp dữ liệu cho từng ô.

**Các điểm cần biết:**
- `UICanvas.Scope` mặc định `Global`. Đặt scope cụ thể để canvas tự đóng khi `UiScopeRegistry.Enter(path)` chuyển sang phạm vi khác.
- `UIButton.DefaultClickSfx` (static) bật/tắt âm click mặc định; override `PlaysClickSfx` cho từng nút.

---

## 11. `com.quang.services.analytics` – Analytics đa backend

**Phụ thuộc:** `base`. · **Không cần prefab.**

### Tác dụng
Game chỉ gọi **một chỗ** `Locator.Analytics`, package tự gửi sự kiện tới **bất kỳ tổ hợp** Firebase / AppsFlyer / AppMetrica / GameAnalytics. Bật/tắt từng backend bằng scripting define, không phải sửa code game.

### Chức năng

| Thành phần | Mô tả |
|---|---|
| `AnalyticsService` | Proxy, cài `IAnalyticsService` + `IRevenueEventSink`; lỗi của một backend không làm sập backend khác |
| `AnalyticsBootstrap` | Tự chạy `BeforeSceneLoad`, khởi tạo backend và gán `Locator.Analytics`, `Locator.RevenueSink` |
| `AnalyticsSettings` + cửa sổ **Quang/Analytics/Settings** | Bật backend, chọn category, nhập key, ghi define |
| Backend: `FirebaseBackend`, `AppsFlyerBackend`, `AppMetricaBackend`, `GameAnalyticsBackend` | Adapter từng SDK; `FirebaseRemoteConfigCache` đọc Remote Config không chặn |
| Category: `Ads`, `Product`, `Design`, `Revenue` | Định tuyến sự kiện tới backend phù hợp |
| Tracking (message space): `AnalyticsTracking.Facts`, `IRule`, `StandardRules`, `TrackingSettings` | Game gửi "sự thật" (fact), các rule biến thành event |
| `RecordingAnalyticsBackend` | Backend giả để test |

### Cách dùng

**Cài đặt:**
1. Import SDK bạn cần. Với GameAnalytics, tạo `Assets/GameAnalytics/Plugins/Scripts/GameAnalyticsSDK.asmdef` (nhớ tạo lại sau khi re-import SDK) và nhập key trong `Window/GameAnalytics/Select Settings`.
2. Mở menu **Quang → Analytics → Settings** → tạo `Assets/Resources/QuangAnalyticsSettings.asset`.
3. Với mỗi backend: tick **enabled**, chọn **categories**, nhập key (AppsFlyer: dev key + iOS app id; AppMetrica: API key).
4. Bấm **Apply scripting defines** → ghi `QUANG_ANALYTICS_<ID>` cho Android, iOS, Standalone.
5. Muốn gỡ hẳn SDK khỏi build: **xoá thư mục SDK** trong `Assets/` (tắt define chỉ gỡ adapter).

**Gửi sự kiện:**

```csharp
// Sự kiện một lần
Locator.Analytics.LogEvent("open_shop", AnalyticsCategory.Design, parameters);

// Tracking theo fact
AnalyticsTracking.Facts.StageStart(12, replay: false);
AnalyticsTracking.Facts.WaveReached(5);
AnalyticsTracking.Facts.StageEnd(StageResult.Fail);
AnalyticsTracking.Facts.Feature("skill_pick", ("skill_id", "fire_ring")); // id phải có trong TrackingSettings.featureEvents
AnalyticsTracking.Register(new MyGameRule()); // đăng ký trước frame đầu tiên
```

**Khi qua màn:** gọi `GameData.level.MarkPassed(level)` và truyền kết quả làm `firstPass` cho `LevelTrackEvent`.

**Lưu ý:** AppMetrica mặc định chỉ nhận `Product` vì plugin đã tự track doanh thu quảng cáo – bật thêm `Revenue` sẽ đếm trùng. Không bao giờ đánh số lại `AnalyticsCategory`.

---

## 12. `com.quang.services.ads` – Quảng cáo

**Phụ thuộc:** `designpattern`, `utilities`, `base`, `data`. · **Cần:** SDK LevelPlay/ironSource (bắt buộc cho `AdsManager`), tuỳ chọn MAX, AdMob.

### Tác dụng
Hiển thị quảng cáo **Rewarded / Interstitial / Banner / App Open** qua một lớp trung lập. Code game không đụng tới SDK; đổi vendor hoặc kết hợp nhiều vendor bằng cấu hình.

### Chức năng

| Assembly | Nội dung |
|---|---|
| `Quang.Ads` (trung lập) | `GameRewardAds`, `GameInterAds`, `GameBannerAds`, `GameAppOpenAds`, interface provider, `NullAdsService`, hệ thống routing, `AdsLoadQueue` |
| `Quang.Ads.Integration.IronSource` (`QUANG_ADS_IRONSOURCE`) | Adapter LevelPlay **và `AdsManager`** (gốc composition, gán `Locator.Ads`) |
| `Quang.Ads.Integration.Max` (`QUANG_ADS_MAX`) | Adapter AppLovin MAX |
| `Quang.Ads.Integration.AdMob` (`QUANG_ADS_ADMOB`) | Adapter Google Mobile Ads 9.1.0 |

Tính năng nổi bật:
- **Request-scoped**: mỗi `AdsShowRequest` nhận đúng **một** `AdsShowResult`; callback muộn bị bỏ qua (`DuplicateIgnored`).
- **Tự pause** bằng `PauseLease` khi hiện quảng cáo, chỉ nhả lease của chính nó.
- **Routed mode**: phục vụ từ MAX + AdMob theo chuỗi (Chain), song song (Parallel), theo format, theo placement; ghi đè từ xa bằng JSON.
- **Bật/tắt từng format** trên `AdsManager` (Rewarded/Interstitial/Banner Ads Enabled).
- Doanh thu quảng cáo gửi qua `Locator.RevenueSink` (không phụ thuộc trực tiếp package analytics).

### Cách dùng

**Cài đặt cơ bản:**
1. Import SDK LevelPlay; thêm define `QUANG_ADS_IRONSOURCE` (và `QUANG_ADS_MAX` / `QUANG_ADS_ADMOB` nếu dùng).
2. Kéo `Runtime/AdsManager.prefab` vào scene khởi động (LoadStart).
3. Trong Inspector `AdsManager`, phần **Enabled Ad Formats**: tắt format game không dùng.
4. Thứ tự khởi tạo đã đảm bảo: `AnalyticsBootstrap` → `AdsManager.Awake` (gán `Locator.Ads`, init LevelPlay).

**Hiển thị quảng cáo:**

```csharp
// Rewarded: callback thưởng chỉ gọi khi người xem xem hết
Locator.Ads.Reward.Show(
    rewardCallBack: () => GiveDoubleCoins(),
    hiddenCallBack: () => ResumeGame(),
    placement: Placement.REROLL_SKILL_CARD);

// Kiểm tra trước có thể hiện rewarded cho placement này không (routed mode)
bool canShow = Locator.Ads.Reward.IsCanShowFor(Placement.REROLL_SKILL_CARD);

// Interstitial
Locator.Ads.Inter.Show(() => LoadNextLevel(), Placement.NONE);

// Banner
Locator.Ads.Banner.Show();
Locator.Ads.Banner.Hide();
```

Ngoài API callback ở trên, `IAdsService` còn kế thừa `IAdsRequestService` (dùng `AdsShowRequest` → `AdsShowResult`): `result.IsEarnedReward` chỉ `true` khi rewarded hoàn tất có bằng chứng thưởng; `result.ShouldContinueFlow` cho biết có nên đi tiếp không. Xem `com.quang.base/Runtime/Services/Contracts/Ads/`.

**Bật Routed mode (nhiều vendor):**
1. Tạo asset **Quang/Ads/Routing Config** (`AdsRoutingConfig`), cấu hình JSON dạng:
   ```json
   {
     "providers": [ { "id": "max", "enabled": true, "androidRewarded": "...", "iosRewarded": "..." },
                    { "id": "admob", "enabled": true, "androidRewarded": "...", "iosRewarded": "..." } ],
     "formats":   [ { "format": "Rewarded", "mode": "Chain", "order": ["max", "admob"] } ],
     "placements":[ { "format": "Rewarded", "placement": "REROLL_SKILL_CARD", "mode": "Chain", "order": ["admob"] } ]
   }
   ```
2. Gán vào `AdsManager.routingConfig`.
3. **Xoá** các component provider MAX có sẵn trên scene và làm rỗng mảng `providerBindings` – nếu không sự kiện MAX sẽ bị đếm hai lần.
4. (Tuỳ chọn) gán `AdsManager.RoutingOverrideJson` để ghi đè từ Remote Config.

**Test:** gán `NullAdsService` cho `Locator.Ads` – mọi request trả `Unsupported`, không giả thưởng.

**Quy tắc D5:** một vendor tích hợp **trực tiếp** hoặc qua **adapter mediation của MAX**, không bao giờ cả hai.

---

## 13. `com.quang.services.iap` – Mua hàng trong ứng dụng

**Phụ thuộc:** `designpattern`, `utilities`, `base`, `data`, `persistence`, `com.unity.purchasing 5.1.1`. · **Trạng thái:** candidate (chưa kiểm chứng sandbox store thật).

### Tác dụng
Đảm bảo mua hàng **không mất tiền mà không nhận đồ, không nhận đồ hai lần**, kể cả khi app crash giữa chừng. Order trên store chỉ được xác nhận **sau khi** game đã trao thưởng và lưu bền.

### Chức năng

| Thành phần | Mô tả |
|---|---|
| Catalog: `PurchaseCatalogEntry`, `PurchaseProductId`, `LegacyPurchaseProductMap` | Ánh xạ ID logic → ID Google Play / App Store / editor |
| Ledger (`purchase-ledger`, `FailClosed`) | Sổ giao dịch bền: trạng thái, fingerprint receipt (không lưu receipt gốc) |
| Coordinator + khoá theo giao dịch | Điều phối pipeline, tránh xử lý song song |
| `UnityLocalPurchaseValidator`, `IUnityReceiptValidationBackend` | Xác thực receipt cục bộ |
| Store adapter Unity Purchasing v5 | Giao tiếp với store |
| `LegacyIapServiceAdapter` | Cầu nối cho API cũ `IIAPService` (obsolete) |

**Pipeline:**
```text
PurchaseAsync(productId)
 → store quan sát giao dịch → Ledger: Observed
 → xác thực receipt         → Ledger: GrantPending
 → game trao thưởng (IPurchaseGrantHandler) → Ledger: Granted
 → xác nhận order với store → Ledger: Completed
```

### Cách dùng
1. Kéo `Runtime/IAPManager.prefab` vào scene khởi động.
2. Khai báo catalog: mỗi sản phẩm có `PurchaseProductId` (vd. `"starter-pack"`, `"gold.pack_1"`) và store ID tương ứng. **Phần thưởng không nằm trong catalog** – game tự quản lý.
3. Game cài đặt `IPurchaseGrantHandler`: cộng vật phẩm **và lưu marker cùng lúc**; nếu đã trao rồi thì trả `AlreadyGranted`.
4. Production Android/iOS phải cấu hình validator dùng Unity obfuscation/tangle. Thiếu validator ⇒ **chặn**, không fallback "luôn hợp lệ".
5. Gọi mua:
   ```csharp
   // purchaseService: IPurchaseIntegrityService (lấy từ composition của game)
   await purchaseService.ConnectAsync();
   PurchaseRequestResult result = await purchaseService.PurchaseAsync(new PurchaseProductId("starter-pack"));
   // Khôi phục / đối chiếu giao dịch dở dang:
   await purchaseService.RestoreAsync();
   await purchaseService.ReconcileAsync();
   ```
6. Game PC/Steam chỉ bán bản game (DesktopPremium) có thể **bỏ** package này, dùng `UnsupportedPurchaseIntegrityService` trong `base`.

> Xác thực cục bộ chỉ là bằng chứng chống giả mạo, **không** thay được xác thực trên server cho game giá trị cao.

---

## 14. `com.quang.liveops.dailygift` – Quà đăng nhập hằng ngày

**Phụ thuộc:** `designpattern`, `base`, `data`, `ui`. · **Cần:** TextMeshPro.

### Tác dụng
Tính năng LiveOps có sẵn: người chơi đăng nhập mỗi ngày nhận một phần quà theo chuỗi ngày, kèm popup hiển thị.

### Chức năng

| Thành phần | Mô tả |
|---|---|
| `DailyGiftManager` (+ prefab) | Theo dõi chuỗi đăng nhập, logic nhận quà; cài `IDailyGiftService` → `Locator.DailyGift` |
| `DailyGiftPopup` (+ prefab), `GiftDayItem.prefab`, `DailyGiftUIItem` | Popup và ô hiển thị từng ngày |
| `Debug_DailyGiftPopup` | Popup debug để test nhảy ngày |
| `ConfigureTimeRewardIntegrity(...)` | Inject đồng hồ UTC, chính sách ngày thưởng, profile, coordinator nhận thưởng |

Khi nhận quà, manager phát sự kiện qua **EventBus** để UI game (vd. HomeCanvas) cập nhật – không phụ thuộc trực tiếp UI.

### Cách dùng
1. Kéo `Runtime/DailyGiftManager.prefab` vào scene boot/Home.
2. Tạo một ScriptableObject **của game** cài `Quang.Data.LiveOps.IDailyGiftConfig` (danh sách ngày + phần thưởng `ItemId` + số lượng), gán cho manager và popup.
3. Mở popup:
   ```csharp
   UIManager.Ins.OpenUI<DailyGiftPopup>();
   ```
4. Gọi logic qua `Locator.DailyGift`.
5. (Khuyến nghị) gọi `ConfigureTimeRewardIntegrity` khi composition để dùng UTC và chống nhận trùng.

> Tính ngày theo giờ thiết bị nên có thể gian lận bằng chỉnh giờ – chấp nhận được với game offline.

---

## 15. `com.quang.liveops.energy` – Năng lượng / mạng chơi

**Phụ thuộc:** `base`. · Thay thế `com.quang.liveops.heart` (không migrate save từ Heart).

### Tác dụng
Hệ thống "năng lượng/tim" kiểu game casual: mỗi lượt chơi tốn năng lượng, hồi theo thời gian, có năng lượng thưởng và chế độ không giới hạn. Được thiết kế **an toàn giao dịch**: không trừ/cộng hai lần, crash giữa lượt vẫn hoàn lại đúng.

### Chức năng

| Thành phần | Mô tả |
|---|---|
| `IEnergyService` | `TryStartRun`, `MarkRunEntered`, `CompleteRun`, `CancelFailedStart`, `AddRenewable`, `AddBonus`, `GrantUnlimited`, reconcile, sự kiện `Changed` |
| `EnergySnapshot` | Ảnh chụp chỉ đọc: renewable / bonus / tổng, cap, Unlimited, lần hồi tiếp theo, lượt đang chạy |
| `EnergyResultOutcome` | `Success`, `IdempotentReplay`, `Insufficient`, `InvalidInput`, `Conflict`, `ConfigurationFailure`, `ClockRollback`, `PersistenceFailure`, `Unavailable` |
| `EnergyServiceFactory.CreateLocal(...)` | Điểm tạo service, kiểm tra config |
| `EnergyConfigSO` | Cấu hình |

### Cách dùng

**1. Tạo config:** **Assets → Create → Quang → LiveOps → Energy Config**

| Trường | Ý nghĩa |
|---|---|
| `renewableMax` | Trần năng lượng hồi được |
| `regenerationIntervalSeconds` | Số giây hồi 1 điểm (> 0) |
| `runCost` | Tốn bao nhiêu mỗi lượt (trừ renewable trước, rồi bonus) |
| `initialRenewable` / `initialBonus` | Số dư ban đầu lần chơi đầu tiên |
| `transactionRetentionCapacity` | Số giao dịch lưu để phát hiện gọi lặp |

**2. Tạo service:**

```csharp
EnergyCreationResult result = EnergyServiceFactory.CreateLocal(config, stateFilePath);
if (!result.Success)
{
    Debug.LogError(result.ErrorMessage); // config sai ⇒ chặn vào màn, KHÔNG fallback
    return;
}
IEnergyService energy = result.Service;
```

**3. Vòng đời một lượt chơi:**

```csharp
energy.TryStartRun(runId);         // TRƯỚC khi load gameplay – giữ chỗ chi phí
energy.MarkRunEntered(runId);      // SAU khi gameplay load xong
// Kết thúc bằng đúng MỘT trong hai:
energy.CompleteRun(runId, RunOutcome.Win);   // Win / Loss / Abandoned
energy.CancelFailedStart(runId);            // chỉ khi gameplay CHƯA vào được → hoàn lại
```

**4. Cộng năng lượng – dùng ID ổn định:**

```csharp
// SAI: mỗi lần retry ra ID mới ⇒ cộng trùng
energy.AddRenewable(amount, Guid.NewGuid().ToString());
// ĐÚNG: lấy từ thứ bền vững (mã giao dịch IAP, runId đã lưu)
energy.AddRenewable(amount, purchaseReceipt.TransactionId);
```

File state hỏng sẽ được **cách ly** (đổi tên `energy-state.corrupt-<ticks>-<guid>.json`) rồi tạo mới từ config. `PersistenceFailure` nghĩa là "chưa có gì xảy ra" – retry an toàn với cùng ID.

---

# LAYER 6 – CÔNG CỤ PHÁT TRIỂN

## 16. `com.quang.tools` – Công cụ Editor

**Phụ thuộc:** không khai báo (thực tế cần `Quang.UI`, DOTween, Spine cho một số tool). · Chủ yếu chạy trong **Editor**.

### Tác dụng
Bộ công cụ tăng tốc cho artist và dev: xử lý ảnh, bake Spine, chỉnh màu VFX, scale particle, dọn GUID/asset trùng, xuất UI flow.

### Chức năng & cách mở

| Công cụ | Menu | Dùng để |
|---|---|---|
| Grayscale Texture Converter | **Tools → Universal → Art → Image → Grayscale Texture Converter** | Chuyển texture sang thang xám hàng loạt |
| Style Texture Converter | **Tools → Universal → Art → Image → Style Texture Converter** | Áp "phong cách" cho ảnh theo profile gồm các bước: tách nền, posterize, viền mực (ink line), rim light, gradient ramp, chỉnh màu… |
| VFX Color Tone Shifter | **Tools → Universal → Art → Particle → VFX Color Tone Shifter** | Đổi tông màu cả bộ hiệu ứng (vd. lửa đỏ → lửa xanh) |
| Prefab Particle Scaler | **Tools → Universal → Art → Particle → Prefab Particle Scaler** | Scale đúng tỉ lệ toàn bộ particle trong prefab |
| Spine Sprite Sheet Particle Setup | **Tools → Universal → Art → Particle → Spine Sprite Sheet Particle Setup** | Tạo particle từ sprite sheet bake từ Spine |
| Bake Sprite Sheet (Spine) | **Tools → Universal → Art → Spine → Bake Sprite Sheet** | Bake animation Spine thành sprite sheet (nhẹ hơn khi chạy) |
| Create TMP Sprite Asset From Atlas | **Tools → Universal → Art → UI → Create TMP Sprite Asset From Atlas** | Tạo sprite asset cho TextMeshPro từ atlas (chèn icon vào text) |
| Export Sprite Atlas | **Tools → Universal → Art → UI → Export Sprite Atlas** | Xuất atlas ra ảnh |
| Method Tween Preview | **Tools → Universal → Preview → Method Tween Preview** | Xem trước tween DOTween trong Editor không cần Play |
| UIDOTweenDemo | component | Demo/preview tween trên UI |
| Asmdef Reference Finder | **Tools → Universal → Maintenance → Assembly Definition → Find References Deep** | Tìm asmdef nào tham chiếu tới asmdef nào |
| Duplicate Asset Resolver ⚠️ | **Tools → Universal → Maintenance → Danger → Duplicate Asset Resolver** | Tìm & gộp asset trùng |
| Meta GUID Unifier ⚠️ | **Tools → Universal → Maintenance → Danger → Meta GUID Unifier** | Đồng bộ GUID trong file `.meta` |
| UI Flow Export | **Tools → Universal → UI Flow → Configure Export Folders**; chuột phải asset → **Assets → Universal → UI Flow → Export Selected Definition/Pack Package** | Chụp cấu trúc UI (`UIFlowDefinition`, `UIFlowPack`) và xuất thành gói |

### Cách dùng
- Cài package, mở công cụ qua menu tương ứng ở bảng trên.
- Các công cụ trong nhóm **Danger** sửa trực tiếp asset/meta: **commit git trước khi chạy** để có thể hoàn tác.
- `Tools.json` là cấu hình cho tool đóng gói SDK (thư mục nguồn, thư mục xuất `SDKBuilds`, mẫu đường dẫn loại trừ).

---

## 17. `com.quang.autotest` – Kiểm thử tự động

**Phụ thuộc:** không có package `com.quang.*` nào. · Tuỳ chọn: `inputsystem` + `ugui` để bật FakeInput.

### Tác dụng
Chạy **kịch bản test gameplay tự động** (trong Editor, batchmode CI, hoặc trên bản build), thu log/sự kiện, kiểm tra assertion, xuất báo cáo JSON/Markdown. Package không biết gì về game – mỗi game cắm "glue" của mình vào.

### Chức năng

| Thành phần | Mô tả |
|---|---|
| `AutoTestRunner`, `AutoTestContext`, `AutoTestBootstrapper` | Bộ chạy và vòng đời kịch bản |
| `AutoTestSuiteData`, `AutoTestCaseData`, `AutoTestAssertionConfig` | Dữ liệu suite/case dạng asset |
| Assertion có sẵn: `NoExceptionLog`, `ScenarioStarted`, `ScenarioTimeout`, `NoNaNTransform` | Kiểm tra cơ bản |
| `AutoTestAssertionRegistry` | Đăng ký assertion riêng (theo enum hoặc theo string ID) |
| `RuntimeSnapshot` + extension | Chụp trạng thái game để assertion kiểm tra |
| `AutoTestLogCollector`, `AutoTestEventCollector` | Thu log và sự kiện |
| `AutoTestJsonExporter`, `AutoTestMarkdownExporter` | Xuất báo cáo |
| `AutoTestRunnerWindow` | Cửa sổ **Tools → Testing → Auto Test Runner** |
| `AutoTestCliRunner` | Chạy từ dòng lệnh / CI |
| `AutoTestPlayerEntrypoint`, `RuntimeEvidenceWriter` | Chạy trên bản build, ghi bằng chứng (tự che token/receipt/password) |
| `Quang.AutoTest.FakeInput`: `AutoTestInputSystemDriver`, `AutoTestEventSystemPointerDriver` | Giả lập bàn phím/chuột và click/kéo UI |

### Cách dùng

**1. Viết glue cho game** (assembly riêng tham chiếu `Quang.AutoTest`):

```csharp
public static class MyGameAutoTestBootstrap
{
    [RuntimeInitializeOnLoadMethod]
    static void Install()
    {
        AutoTestRunner.ExecutorFactory        = () => new MyScenarioExecutor();     // IAutoTestScenarioExecutor
        AutoTestRunner.SnapshotBuilderFactory = () => new MySnapshotBuilder();      // IRuntimeSnapshotBuilder
        AutoTestBootstrapper.BootKick         = /* khởi động game */;
        AutoTestBootstrapper.HostBootReset    = /* reset trạng thái game */;
        AutoTestBootstrapper.ExtraReadyCheck  = () => GameManager.Ins != null;

        AutoTestAssertionRegistry.Register(
            "mygame.feature.ready",
            config => new FeatureReadyAssertion(config),
            new AutoTestAssertionDescriptor("mygame.feature.ready", "Feature Ready", "Chờ tính năng sẵn sàng."));
    }
}
```

**2. Tạo asset Suite/Case**, gắn scenario và assertion; case cần giả lập input thì đặt:
```csharp
testCase.requiredCapabilities = AutoTestRuntimeCapability.FakeInput;
```

**3. Chạy:**
- Trong Editor: **Tools → Testing → Auto Test Runner**.
- CI/batchmode:
  ```text
  Unity -batchmode -projectPath . -executeMethod Quang.AutoTest.Editor.AutoTestCliRunner.RunSuiteFromCommandLine \
        -autoTestSuite <path> -autoTestReadyTimeout <giây> -autoTestGameplayScene <tên scene>
  ```
- Trên bản build:
  ```text
  MyGame -rcScenario <id> -rcRun <run-id> -rcOutput <thư mục> -rcPhase <phase> -rcProfile <profile>
  ```
  Mã thoát: `0` = pass, `1` = fail, `3` = bị chặn / timeout / thiếu kịch bản / lỗi trước khi có kết quả.

**4. Giả lập input:**

```csharp
using var input = new AutoTestInputSystemDriver();
input.QueueText("hello");
input.PumpOneFrame();

var pointer = new AutoTestEventSystemPointerDriver();
pointer.TryClick(buttonGO, screenPos);
yield return pointer.Drag(srcGO, startPos, dstGO, endPos, moveFrames: 6);
```

Playbook cho AI: `AI~/playbooks/adopt.md`, `run-from-agent.md`.

---

## Phụ lục: Chọn package theo loại game

| Nhu cầu | Cài các package |
|---|---|
| **Tối thiểu** (core + save) | `designpattern`, `utilities`, `persistence`, `base`, `data` |
| + Giao diện | thêm `ui` |
| **Game mobile F2P đầy đủ** | tối thiểu + `ui`, `services.analytics`, `services.ads`, `services.iap`, `liveops.dailygift`, `liveops.energy` |
| Game hành động / có AI nhân vật | tối thiểu + `character` |
| Game PC/Steam bán bản quyền | tối thiểu + `ui` (bỏ `ads`, `iap`; dùng `UnsupportedPurchaseIntegrityService`) |
| Game có server | thêm `sync`; dùng `sync.simulator` trong test |
| Hỗ trợ đội art / dọn project | `tools` |
| QA tự động / CI | `autotest` |
| Làm việc với AI (Claude/Cursor) | `framework` |

**Nhớ:** khi thêm một package, kiểm tra mục "Phụ thuộc" của nó và thêm đủ tất cả `com.quang.*` liên quan vào `manifest.json` với **cùng một tag release**.
