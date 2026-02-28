# InfiniteTavern — Feature Backlog

Список запланованих фіч від найпростіших до складніших.
Кожна має короткий опис і prompt для Copilot щоб почати імплементацію.

---

## 🟢 Дуже просто (< 1 год)

### 1. HP відновлення при level-up
**Що:** При досягненні нового рівня HP гравця відновлюється до MaxHP.  
**Де:** `GameService.cs` — після блоку LeveledUp.  
**Prompt:**
> In `GameService.cs`, find where `LeveledUp = true` is set after XP gain. Immediately after setting it, also set `player.HP = player.MaxHP`. Make sure the updated HP is reflected in the `TurnResponse`.

---

### 2. Критичний удар на nat 20
**Що:** Якщо гравець або ворог кидає d20 і випадає 20 — урон подвоюється. Додати повідомлення "💥 Critical Hit!".  
**Де:** `GameEventHandlerService.cs` — у `HandleDamage`, в блоці dodge/block де вже є `_diceService.Roll("1d20")`.  
**Prompt:**
> In `GameEventHandlerService.cs`, in the `HandleDamage` method, after the dodge/block dice roll, if the roll equals 20 (natural 20), double the `gameEvent.Amount` and prepend "💥 Critical Hit! " to the returned applied-event message. Do this for both player-targeting and enemy-targeting damage.

---

### 3. Кнопка "Використати" для зілля в інвентарі
**Що:** Для предметів типу `Potion` замість кнопки "Екіпірувати" показувати кнопку "Використати", яка надсилає дію `"I use [item name]"` через `onSubmitAction`.  
**Де:** `Inventory.tsx` — умовний рендер кнопки залежно від `item.type`.  
**Prompt:**
> In `frontend/src/components/Inventory.tsx`, for items where `item.type === 'Potion'`, replace (or add alongside) the equip button with a "Use" button. Clicking it should call a prop `onUseItem(item.name)` which the parent (`App.tsx` / `GameView.tsx`) wires to `onSubmitAction("I use " + itemName)`. Style it similarly to the equip button but with a green tint.

---

### 4. Збереження сесії в localStorage
**Що:** При старті гри зберігати `gameSessionId` у `localStorage`. При перезавантаженні сторінки автоматично відновлювати попередню сесію замість екрану створення персонажа.  
**Де:** `App.tsx` — в `handleNewGame` і `useEffect` при старті.  
**Prompt:**
> In `frontend/src/App.tsx`, after a new game is created (`handleNewGame`), save `gameSessionId` and the full `gameState` to `localStorage` (key: `"infiniteTavern_session"`). On app startup (in a `useEffect`), check localStorage for a saved session and restore it — skip the character creation screen if a valid session exists. Add a "New Game" button in the header that clears localStorage and resets to the creation screen.

---

### 5. Тултіпи на статах
**Що:** При наведенні на кожен стат (СИЛ, СПР, ВИТ...) показувати підказку що він означає і на що впливає.  
**Де:** `PlayerStats.tsx` — `title` атрибут або CSS tooltip.  
**Prompt:**
> In `frontend/src/components/PlayerStats.tsx`, add a `title` tooltip to each stat item explaining what it does. For example: Strength → "Affects weapon damage and physical checks", Dexterity → "Affects dodge chance and agility checks", etc. Also add the Ukrainian translations to `frontend/src/locales/uk.ts`. Use either the HTML `title` attribute or a CSS tooltip via `::after` pseudo-element for better styling.

---

## 🟡 Середньо (2–4 год)

### 6. UI Журналу квестів
**Що:** Backend вже надсилає `quest_updates` в `AppliedEvents`. Потрібно зберігати список квестів у стані гри і відображати у окремій вкладці.  
**Де:** `App.tsx` (стан), новий компонент `QuestLog.tsx`, `GameView.tsx` (вкладка).  
**Prompt:**
> Implement a Quest Log feature. In `App.tsx`, add a `quests: {title: string, status: 'Active'|'Completed'|'Failed'}[]` array to `GameState`. In `handleTurn`, parse `response.appliedEvents` for entries matching "Quest: ... Active/Completed/Failed" and update the quests array. Create a new `frontend/src/components/QuestLog.tsx` component that displays active quests (with a scroll icon 📜) and completed ones. Add it as a third tab in `GameView.tsx` alongside Stats and Inventory.

---

### 7. Відпочинок у Таверні (Rest)
**Що:** Кнопка "Відпочити" видима лише коли `locationType === 'Tavern'`. Коштує 10 золота, відновлює HP до максимуму.  
**Де:** `GameView.tsx` (кнопка), або просто передати як suggested action. Backend: новий endpoint або обробка в `GameService`.  
**Prompt:**
> Add a "Rest at the Tavern" button in `GameView.tsx`, visible only when `gameState.locationType === 'Tavern'` and the player has ≥ 10 gold and HP < MaxHP. Add a new backend endpoint `POST /api/game/rest` in `GameController.cs` that takes `{ gameSessionId }`, deducts 10 gold from the player, restores HP to MaxHP, and returns updated `PlayerStats`. Wire it to a `gameService.rest(sessionId)` call in the frontend.

---

### 8. Лічильник ходів і час гри
**Що:** В хедері показувати кількість зроблених ходів і тривалість поточної сесії (HH:MM).  
**Де:** `App.tsx` (state), `GameView.tsx` (хедер).  
**Prompt:**
> In `frontend/src/App.tsx`, add `turnCount: number` and `sessionStartTime: Date` to the game state. Increment `turnCount` on every successful turn. In `GameView.tsx`, display "Turn: X | Time: HH:MM" in the game header. Update the timer every minute using `setInterval` in a `useEffect`. Add Ukrainian translations for "Turn" and "Time" labels.

---

### 9. Flee mechanic (втеча з бою)
**Що:** Є в промпті (`flee_attempt`) і в `CombatSystemPrompt`, але відсутній обробник у `GameEventHandlerService`. Треба реалізувати: перевірка Dexterity, успіх → `IsInCombat = false`, провал → ворог б'є у відповідь.  
**Де:** `GameEventHandlerService.cs` — додати `{ "flee_attempt", HandleFleeAttempt }`.  
**Prompt:**
> In `GameEventHandlerService.cs`, add a `HandleFleeAttempt` handler and register it in the `_eventHandlers` dictionary. The handler should: roll `1d20 + (player.Dexterity - 10) / 2` against DC 12. On success, set `session.IsInCombat = false` and `session.Enemies.Clear()`, yield "Escaped successfully!". On failure, yield "Failed to flee!" and apply 1d4 damage from a random living enemy as punishment.

---

---

## 🟢 Нові — дуже просто

### 12. Хоткеї для suggested actions (клавіші 1/2/3)
**Що:** Натискання клавіш `1`, `2`, `3` на клавіатурі відразу надсилає відповідну suggested action — не треба клікати мишею.  
**Де:** `GameView.tsx` — `useEffect` з `keydown` listener.  
**Prompt:**
> In `frontend/src/components/GameView.tsx`, add a `useEffect` that listens for `keydown` events on `window`. When the user presses `"1"`, `"2"`, or `"3"` and the input field is NOT focused, call `handleSuggestedAction` with `gameState.suggestedActions[0/1/2]` respectively (if it exists). Also add a subtle `[1]`, `[2]`, `[3]` label on each suggested-action button so the player knows the shortcut exists.

---

### 13. Історія введених команд (↑/↓ як у терміналі)
**Що:** Натискання стрілки ↑ в input-полі вставляє попередню команду гравця, ↓ — наступну. Зручно для повторення дій.  
**Де:** `GameView.tsx` — локальний `inputHistory` масив + `historyIndex`.  
**Prompt:**
> In `frontend/src/components/GameView.tsx`, maintain a local `inputHistory: string[]` array and `historyIndex` ref. Every time the player submits an action (non-empty), push it to `inputHistory`. Add an `onKeyDown` handler to the action `<input>`: ArrowUp sets the input value to the previous history entry, ArrowDown to the next one (or empty string). Clamp the index correctly.

---

### 14. Емодзі-іконки для ворогів за типом
**Що:** Перед ім'ям ворога в `EnemyList` показувати емодзі залежно від ключового слова в назві: goblin→👺, wolf→🐺, orc→👹, skeleton→💀, dragon→🐉, rat→🐀, spider→🕷️, bandit→🗡️, troll→👾 тощо.  
**Де:** `EnemyList.tsx` — чиста функція `getEnemyIcon(name)`.  
**Prompt:**
> In `frontend/src/components/EnemyList.tsx`, add a `getEnemyIcon(name: string): string` function that maps common enemy name keywords (case-insensitive) to emojis: goblin→👺, troll→👾, orc→👹, skeleton/undead→💀, dragon→🐉, wolf/werewolf→🐺, rat→🐀, spider→🕷️, bandit/thief/rogue→🗡️, ghost/wraith→👻, witch/mage→🧙, bear→🐻, default→⚔️. Render the icon before the enemy name in the enemy card.

---

### 15. Пасивна регенерація HP на основі Constitution
**Що:** Поза боєм, кожен хід гравець відновлює `(Constitution - 10) / 4` HP (мінімум 0, максимум 3). Дає Constitution реальне значення між боями.  
**Де:** `GameService.cs` — на початку `ProcessTurnAsync`, перед викликом AI, якщо `!session.IsInCombat`.  
**Prompt:**
> In `GameService.cs` in `ProcessTurnAsync`, before calling the AI, add passive out-of-combat HP regen: if `!session.IsInCombat` and `player.HP < player.MaxHP`, calculate `regen = Math.Clamp((player.Constitution - 10) / 4, 0, 3)`. If `regen > 0`, set `player.HP = Math.Min(player.HP + regen, player.MaxHP)` and add a string like `"Regenerated {regen} HP (Constitution)"` to `appliedEvents`. Reflect the updated HP in `TurnResponse`.

---

### 16. Статистика на екрані Game Over
**Що:** Розширити існуючий `game-over-overlay` — показати підсумок: кількість ходів, зібране золото, рівень та XP досягнутий за гру.  
**Де:** `GameView.tsx` — розширити блок `isGameOver`. `App.tsx` — зберігати `turnCount`, `totalGoldEarned`.  
**Prompt:**
> Extend the game over screen in `frontend/src/components/GameView.tsx`. Add props `turnCount` and `totalGoldEarned` to `GameViewProps`. In the `isGameOver` overlay, below the death message, display a stats summary: "Turns survived: X", "Level reached: Y", "Gold collected: Z", "XP earned: W". In `App.tsx`, track `turnCount` (increment each turn) and `totalGoldEarned` (sum all gold_found events). Pass them to `GameView`.

---

### 17. Порівняння предметів при наведенні
**Що:** При наведенні на непоодягнутий предмет (зброя/броня) показувати різницю бонусів відносно поточного equipped предмету того ж типу: `+1 Strength ▲` або `-2 Defense ▼`.  
**Де:** `Inventory.tsx` — tooltip або inline diff під бонусами.  
**Prompt:**
> In `frontend/src/components/Inventory.tsx`, for unequipped items that have bonuses, compute a diff vs the currently equipped item of the same `type`. Find the equipped item via `inventory.find(i => i.isEquipped && i.type === item.type)`. For each stat in the hovered item's bonuses, show the delta (e.g., `+2 Strength ▲` green or `-1 Defense ▼` red) in small text below the bonus badge. If no equipped item of same type exists, show nothing extra.

---

## 🟡 Нові — середньо

### 18. Бонус Charisma до соціальних перевірок у промпті
**Що:** Додати правило в `PromptTemplates.cs` що при CHA ≥ 14 соціальні DC (difficulty) зменшуються на 2, при CHA ≤ 7 — збільшуються на 2. Дає Charisma реальний вплив на гру.  
**Де:** `PromptBuilderService.cs` або `PromptTemplates.cs` — додати до контексту гравця.  
**Prompt:**
> In `PromptBuilderService.cs`, when building the player context string injected into the DM prompt, add a computed line: if `player.Charisma >= 14` append "Player has HIGH Charisma — reduce social skill check DC by 2 (persuasion, intimidation, deception)." If `player.Charisma <= 7` append "Player has LOW Charisma — increase social skill check DC by 2." This makes Charisma mechanically meaningful without backend changes.

---

### 19. Лічильник вбивств (Kill Counter)
**Що:** Зберігати `enemiesDefeated: number` у стані гри. Інкрементувати при кожній смерті ворога (можна відловити з `appliedEvents` або безпосередньо в `GameEventHandlerService`). Показувати в sidebar і в Game Over.  
**Де:** `GameEventHandlerService.cs` (`HandleXpGained` або перевірка мертвих ворогів), `TurnResponse`, `App.tsx`, `PlayerStats.tsx`.  
**Prompt:**
> Add kill tracking. 1) In `GameModels.cs`, add `EnemiesDefeated: int` to `TurnResponse`. 2) In `GameService.cs ProcessTurnAsync`, after processing events, count enemies that transitioned from `IsAlive=true` to `IsAlive=false` this turn and set `TurnResponse.EnemiesDefeated`. 3) In `App.tsx`, maintain `totalKills: number` in game state and add it on each turn. 4) Display it in `PlayerStats.tsx` as a small "☠️ Kills: X" stat, and in the Game Over screen.

---

### 20. Toast-нотифікації для важливих подій
**Що:** Floating pop-up повідомлення при: level up ("⬆️ Level Up! Level 3"), отриманні рідкісного предмету, критичному ударі, смерті ворога. Зникають через 3 секунди.  
**Де:** Новий `Toast.tsx` + `useToast` hook. У `App.tsx` тригерити при обробці `TurnResponse`.  
**Prompt:**
> Implement a toast notification system. 1) Create `frontend/src/components/Toast.tsx` — a component that renders a floating list of toasts (bottom-right corner), each with a message, icon, and color. Toasts auto-dismiss after 3 seconds. 2) Create a `useToast` hook in `frontend/src/contexts/` with `addToast(message, type: 'levelup'|'item'|'combat'|'info')`. 3) In `App.tsx`, after each turn response, trigger toasts for: `LeveledUp` → "⬆️ Level Up!", new items found (parse `appliedEvents`), critical hit messages. Style with CSS animation (slide in from right, fade out).

---

## 🔵 Складніше але круто

### 10. Статус-ефекти (отрута, горіння, заморозка)
**Що:** AI може накласти статус-ефект через новий event тип `status_effect`. Backend зберігає список ефектів на гравці, зменшує HP кожен хід (для DoT ефектів), фронт показує іконки стану.  
**Де:** `PlayerCharacter.cs` (нове поле), `GameEventHandlerService.cs` (новий handler + DoT логіка), `AIResponse.cs` (новий тип), `PromptTemplates.cs` (новий event), `PlayerStats.tsx` (іконки).  
**Prompt:**
> Implement a status effect system. 1) Add `StatusEffects: List<StatusEffect>` to `PlayerCharacter` where `StatusEffect` has `Name`, `DamagePerTurn`, `DurationTurns`, `IconEmoji`. 2) Add `"status_effect"` to `GameEventHandlerService` that applies the effect. 3) At the start of each `ProcessTurnAsync`, tick all active status effects (apply damage, decrement duration, remove expired ones). 4) Add `"status_effect"` event type to `PromptTemplates.cs` with examples: Poison (2 dmg/turn, 3 turns), Burning (3 dmg/turn, 2 turns). 5) In `PlayerStats.tsx`, show status effect icons below the HP bar with remaining turns.

---

### 11. Торговець / Магазин
**Що:** AI може відкрити магазин через event `shop_open` з переліком товарів і цінами. Фронт показує модальне вікно, гравець може купувати предмети за золото.  
**Де:** Новий event у `AIResponse.cs`, обробник у `GameEventHandlerService.cs`, новий компонент `ShopModal.tsx`, новий endpoint `POST /api/game/buy`.  
**Prompt:**
> Implement a shop/merchant system. 1) Add `ShopItems` list to `AIResponse` GameEvent — each item has `name`, `type`, `bonuses`, `price`. 2) In `GameEventHandlerService`, handle `"shop_open"` by storing the shop items in `GameSession`. 3) Return shop items in `TurnResponse`. 4) In the frontend, when `response.shopItems` is non-empty, show a `ShopModal.tsx` with items, prices, and a Buy button. 5) Add `POST /api/game/buy` endpoint that validates gold, deducts it, and adds the item to inventory. 6) Update `PromptTemplates.cs` with `shop_open` event type and example.
