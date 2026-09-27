# Module, prefab, and communication rules

Parent: [`PROJECT.md`](PROJECT.md). These rules cover where new code goes and how it talks to
other code: who owns what, which direction calls go, and when to use `EventManager`. What exists
today is described in [`architecture.md`](architecture.md).

They are adapted from the ETL project's `module-boundary-rules.md`, `module-decision-procedure.md`,
and `prefab-rules.md`. They are cut to this project's size and bound to its real mechanisms. The
rule IDs belong to this project. Cite them by ID, for example "violates C3".

**Scope: new code only.** That means new prefabs, new scripts and systems, and the parts of
existing code you already have to change for the request. Don't retrofit anything else. Existing
code breaks several of these rules (see *Known exceptions*), and that is not a reason to copy it.

**Status: not yet validated here.** If a rule forces awkward code, say so and name the rule ID,
instead of quietly working around it.

## Project bindings

| The rule concept | In this project |
|---|---|
| App-level owners | The persistent `SingletonMono` managers in `Assets/Scripts/Manager/`, plus `SceneLoader`/`FadeManager`. Anything can reach them through `X.Instance`. |
| Scene-level owners | Non-persistent singletons (`UnitManager`, `UnitRenderManager`) and the per-scene bootstrap and root UI scripts. |
| Domain owner of a fact or value | The manager that owns the data: `PlayerDataManager` for player data, pity, and synergy; `GameManager` for battle flow; `BackendManager` for server calls; `DataManager` for static tables. For UI, it is the root panel script of that screen. |
| Global fact bus | `EventManager`, facts only. It has no request/response channel, and none should be added. |
| State change across systems | A call to the owning manager's public method. |
| Lifecycle pair | `OnEnable` ↔ `OnDisable`. Pooled objects are toggled active on get and release, and `OpenUI`/`CloseUI` toggle the UI too, so this one pair covers MonoBehaviours, pooled objects, and UI. |

This project keeps Unity's lifecycle callbacks. ETL's rule that only the composition root may use
`Awake`/`OnEnable`/`OnDestroy` (its O5) was **not** ported. The managers and `BaseUI`/`BasePopUpUI`
are built on those callbacks.

## Decision procedure

Answer three questions to pick the mechanism:

```
1. Is the call top-down (owner -> something it owns)?   -> Direct call. Done.
2. Does it cross a prefab/system boundary?               -> picks the column
3. Is it a command, a fact, a value, or a state change?  -> picks the row
```

| Intent | Inside one prefab/system | Across prefabs/systems |
|---|---|---|
| Command downward | Direct call (O1) | Direct call on a reference the common owner handed over (P3) |
| Report a fact upward | A `public event` on the child; the parent subscribes (O2) | Up to the domain owner. If that owner is app-level, it publishes the fact on `EventManager` (C1). |
| Need a value | The owner pushes it (E1) | Read the domain manager's getter once, or subscribe to its update fact and cache the value (E1, C1) |
| Change state | Call the owner's method | Call the owning manager's public method (C2). Never publish an event to ask for it. |

**Channel ownership.** A channel belongs to the lowest common owner of everything that uses it.
It goes on `EventManager` only when that owner is the whole app, meaning the parties live in
different scenes, different managers, or unrelated UI trees.

```
parent -> child                    direct call (O1)
child -> parent                    public event, parent subscribes with a method (O2)
sibling <-> sibling                their parent wires them together (P3)
unrelated systems / manager -> UI  EventManager fact published by the domain owner (C1)
anything -> manager state          the manager's public method (C2)
```

**Signs that a fact is not global:** both ends are under one prefab or one screen, the event is
fired only to reach a sibling, or its payload names a specific UI object. In those cases the
common owner should wire the two ends instead.

## Rules

### O — Ownership and direction

- **O1.** An owner calls the things it owns directly. How it owns them (a `[SerializeField]`
  reference, or instantiating them) doesn't matter. What matters is who decides their creation and
  lifetime.
- **O2.** A child structurally references only three kinds of thing: its own direct children,
  what its owner handed it, and app-level managers through `Instance`. To report upward, it
  raises a return-less `public event`, or a fact through its domain owner. No `abstract`/`virtual`
  hook is used as a callback into the owner, and no stored `Action` setter is injected (a
  per-request completion callback is fine). **Don't use `GetComponentInParent` or
  `transform.parent` to find an owner.** If you need one, the hierarchy or the ownership is wrong.
  A pooled object or payload you received in a handler must not be kept past that handler.
- **O3.** Before adding a new `SingletonMono`, name the existing manager you considered and say
  why it isn't the owner. A manager is app-wide state. A scene-scoped one must override
  `IsPersistent => false`.

### C — EventManager channels

- **C1. Facts only, from the domain owner.** Publish what already happened, such as
  `SynergyDataUpdatedEvent` or `BattleEndedEvent`. The domain owner publishes after its state has
  changed. Leaf objects such as a unit, a slot, or a pooled effect don't publish global facts.
  They tell their owner, and the owner publishes. UI-to-UI input relays that have no single owner
  (the existing guide and hold events) are allowed, but prefer the common owner.
- **C2. Never use an event to request a state change.** `XRequested`/`XSucceeded` pairs over
  `EventManager` are banned. Call the owning manager's method, and await it if it is async
  (`BackendManager` calls are UniTask).
- **C3. Payloads are immutable values.** Use ids, enums, numbers, and `readonly` structs where
  practical. Never put a `GameObject`, `MonoBehaviour`, or mutable collection in a payload.
- **C4. Declare the struct with its publisher.** Put it in the publisher's file, or in a small file
  of its own beside the publisher. Choose the location by the publisher, not the subscriber. Don't
  collect events in a shared `Events/` folder.
- **C5. Declare the channel as a field and pair its lifetime.** Fetch `GetPublisher<T>()` /
  `GetSubscriber<T>()` into a field, in `Awake` or at the start of `OnEnable`. Subscribe in
  `OnEnable` and call `?.Unsubscribe` in `OnDisable`. A cached field is necessary here, and not
  just tidy: `GetSubscriber` goes through the hidden `Instance`, which is null after
  `EventManager` is destroyed on quit. An `OnDisable` that calls `GetSubscriber` again can
  therefore throw. Subscribing in `Awake` and unsubscribing in `OnDisable` is wrong, because the
  object stops listening after it is re-enabled.
- **C6. Handlers are method references.** Don't use lambdas or local functions. `Unsubscribe`
  compares delegates, and a lambda written again at the removal site never matches, so
  subscriptions pile up. `Subscribe`'s built-in dedup also only works for the same method on the
  same target. The single exception is a subscription that lives as long as the channel, such as
  a persistent manager subscribing once and never unsubscribing (`UIManager`).

### E — Values

- **E1. Push, don't poll.** If a view needs a value that changes, the owner pushes it, or the
  view subscribes to the owner's update fact and re-reads it then. Don't read a manager from
  `Update` to detect changes. A one-time read on open is fine.
- **E2. Don't drill.** A level that only passes a value on to deeper levels, and reads nothing
  from it, is a placement problem. Fix the hierarchy, or pass one context object (E3) instead of
  growing every signature.
- **E3.** If a call needs many inputs, pass one immutable context or struct, not a growing
  parameter list.
- **E4. Per-frame work goes in the owner's data structure.** It doesn't go through events. For
  example, targeting scans `UnitManager`'s lists. It doesn't publish or subscribe per unit per
  frame.

### P — Prefabs

- **P1.** The owner script is the mediator at every level. Don't add a separate mediator or hub
  class.
- **P2.** Asset references (prefabs, SOs, sprites) go in `[SerializeField]`. A prefab can't
  serialize references to scene objects, so its owner hands those in after instantiating it, or
  it reads them from a manager.
- **P3.** Ownership is recursive: scene root → screens and systems → their children. Each owner
  wires its own children to each other. Siblings don't find each other.
- **P4.** Only the domain owner touches a global channel for its subtree. It subscribes once and
  pushes down to its children. The children don't each subscribe separately.
- **P5.** `Resources/Prefabs/UI/<TypeName>` and `Resources/Prefabs/ObjPooling/<PoolType>` are
  name-based contracts (see [`architecture.md`](architecture.md)). Renaming a class or a prefab is
  a breaking change, and so is moving a file without its `.meta`.

## Anti-patterns

| Pattern | Rule | What breaks |
|---|---|---|
| `GetComponentInParent<T>()` / `transform.parent.GetComponent<T>()` to reach an owner | O2 | Breaks silently when the hierarchy changes, and signals wrong placement |
| `Subscribe(_ => …)` on anything that later unsubscribes | C6 | The removal never matches, so handlers pile up and fire on disabled objects |
| `EventManager.GetSubscriber<T>().Unsubscribe(…)` inside `OnDisable`/`OnDestroy` | C5 | Throws on quit, because `EventManager` is already destroyed |
| `XRequested` event plus `XSucceeded` event | C2 | A hand-rolled RPC with no ordering, error path, or single handler |
| A `GameObject` or `List<>` in an event payload | C3 | Stale after pooling or scene reset, and mutable by any subscriber |
| An SO used as a runtime event or state channel | — | In the Editor its runtime state survives leaving Play mode. The `Resources/DB` SOs are static data. |
| A new manager for something an existing manager already owns | O3 | Two sources of truth, and a new auto-created singleton on first `Instance` |

## Verification greps

Run these over the files your change touched. Each hit needs a reason or a fix.

```
git diff --name-only -- '*.cs' | xargs -r grep -nE 'GetComponentInParent|transform\.parent\.GetComponent'   # O2
git diff --name-only -- '*.cs' | xargs -r grep -nE 'Subscribe\(\s*\(?\w*\)?\s*=>'                          # C6
git diff --name-only -- '*.cs' | xargs -r grep -nE 'EventManager\.Get(Subscriber|Publisher)<' # C5: must be field assignments
git diff --name-only -- '*.cs' | xargs -r grep -nE 'struct \w+(Requested|Request)Event'                       # C2
git diff --name-only -- '*.cs' | xargs -r grep -nE ': SingletonMono<'                                         # O3: name the manager you considered
```

For untracked new files, run the same patterns on those paths.

## Known exceptions in existing code (do not copy)

- `UIManager` subscribes with lambdas for the lifetime of the app. That's allowed under the C6
  exception, but don't copy it into anything that unsubscribes.
- Some views subscribe in `Awake` and unsubscribe in `OnDisable`, for example `UITimer`. That
  breaks C5. If you touch one, fix it in the same change.
- `BattleEndedEvent` is declared in `UI/BattleScene/`, next to a subscriber, but `GameManager`
  publishes it. That breaks C4. Declare new events with their publisher.
- `GetComponentInParent`/`GetComponentInChildren` has about 17 uses in `Assets/Scripts`. That
  breaks O2 where the target is an owner. A lookup down into your own children is fine.
