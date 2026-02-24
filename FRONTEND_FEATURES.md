# Frontend Features & Implementation Details

Детальний опис функціоналу та архітектури React фронтенду.

## 🎨 UI/UX Features

### 1. Character Creation Screen
Перший екран при запуску гри.

**Елементи:**
- Player Name input
- Character Name input
- Race dropdown (Human, Elf, Dwarf, Orc, Halfling)
- Class dropdown (Warrior, Wizard, Rogue, Cleric, Ranger)
- Submit button

**UX:**
- Gradient заголовок
- Форма валідується (required fields)
- Loading state при створенні
- Центрована карточка з тінню

### 2. Game View Layout

**Sidebar (Ліворуч):**
- Character name та info
- HP bar з динамічним кольором
- STR/DEX/INT stats
- Current location

**Main Content (Праворуч):**
- Turn history (scrollable)
- Action input + submit button
- Error messages

**Responsive:**
- Desktop: 2-column layout
- Mobile: stacked vertically

### 3. Turn History

Кожний хід містить:
- 🎮 **Player Action** - з синім акцентом
- 📖 **AI Narrative** - в окремому блоці
- ⚡ **Events** - якщо були зміни (damage, heal, item found, etc.)

**Scroll Behavior:**
- Auto-scroll до останнього ходу
- Smooth animations (fade in)
- Custom scrollbar styling

### 4. Stats Display

**HP Bar:**
- Зелений (> 50%)
- Помаранчевий (25-50%)
- Червоний (< 25%)
- Smooth transitions

**Attributes:**
- Grid layout (3 колонки)
- Purple accent для значень
- Dark background boxes

**Location:**
- 📍 Icon + location name
- Updates після кожного ходу

### 5. Loading States

**Character Creation:**
- Disabled inputs
- "Creating..." button text

**Turn Processing:**
- Loading spinner
- "The Dungeon Master is thinking..."
- Disabled action input

### 6. Error Handling

**Display:**
- Red-bordered box
- Error message from API
- Doesn't block UI

**Scenarios:**
- API connection failed
- Invalid response
- Server error

## 🏗️ Component Architecture

```
App.tsx (State Management)
│
├── CharacterCreation.tsx
│   └── Form with validation
│
└── GameView.tsx (Game Container)
    ├── PlayerStats.tsx
    │   ├── HP Bar
    │   ├── Attributes Grid
    │   └── Location
    │
    ├── TurnHistory.tsx
    │   └── TurnEntry[] (mapped)
    │
    └── ActionForm
        ├── Input
        └── Submit Button
```

## 📦 State Management

### GameState Type
```typescript
{
  sessionId: string | null;
  playerStats: PlayerStats | null;
  currentLocation: string;
  turnHistory: TurnHistoryEntry[];
  isLoading: boolean;
  error: string | null;
}
```

### State Updates

**On Character Create:**
- Set sessionId
- Set playerStats
- Add initial turn to history

**On Turn Submit:**
- Set isLoading = true
- Call API
- Update playerStats (HP)
- Update location
- Append to turnHistory
- Set isLoading = false

**On Error:**
- Set error message
- Keep UI functional

## 🔌 API Integration

### Service Layer (`gameService.ts`)

**Methods:**
- `createNewGame(request)` → NewGameResponse
- `processTurn(request)` → TurnResponse

**Error Handling:**
- Try/catch на кожному виклику
- Throw з нормальними повідомленнями
- Catch в App.tsx

**Base URL:**
- `/api/game` (proxied by Vite)
- Requires backend on `localhost:5000`

## 🎨 Styling System

### CSS Variables
```css
--primary-color: #8b5cf6 (purple)
--bg-dark: #1f2937
--bg-darker: #111827
--text-primary: #f9fafb
--success: #10b981
--warning: #f59e0b
--danger: #ef4444
```

### Theming
- Dark theme by default
- Gradient backgrounds
- Smooth transitions (0.3s)
- Box shadows for depth

### Animations
- fadeIn (0.5s) for new turns
- spin (1s) for loading spinner
- transform on button hover

### Responsive Breakpoints
```css
@media (max-width: 768px) {
  /* Mobile styles */
}
```

## ⚡ Performance Considerations

### React Optimization
- Components are functional (hooks)
- No unnecessary re-renders
- Event handlers use useCallback pattern

### API Calls
- Loading states prevent duplicate requests
- Error boundaries
- Graceful degradation

### Scrolling
- Native browser scroll (performant)
- CSS-only animations
- No heavy libraries

## 🔐 Security

### Input Validation
- Required fields
- Trim whitespace
- Max length enforcement (future)

### API Security
- CORS handled by backend
- No sensitive data in frontend
- API key stays on backend

### XSS Protection
- React escapes by default
- No `dangerouslySetInnerHTML`

## 🧪 Testing Strategy (Future)

### Component Tests
- CharacterCreation form submission
- GameView action handling
- PlayerStats HP bar colors
- TurnHistory rendering

### Integration Tests
- API service calls
- State updates
- Error scenarios

### E2E Tests
- Full game flow
- Character creation → turns → new game

## 📱 Mobile Experience

### Touch-Friendly
- Large tap targets (44px+)
- Swipe to scroll
- No hover-dependent features

### Layout
- Single column
- Stats collapsed
- Full-width inputs

### Performance
- Lightweight (no heavy deps)
- Fast load time
- Minimal bundle size

## 🚀 Future Enhancements

### Features
- [ ] Save/Load game (localStorage)
- [ ] Session recovery
- [ ] Sound effects
- [ ] Quest log sidebar
- [ ] Inventory UI
- [ ] Combat log
- [ ] Character sheet PDF export

### UX
- [ ] Keyboard shortcuts
- [ ] Dark/Light theme toggle
- [ ] Accessibility (ARIA labels)
- [ ] Animations toggle
- [ ] Font size adjustments

### Technical
- [ ] State management library (Zustand/Redux)
- [ ] React Query for API
- [ ] Component library (Headless UI)
- [ ] Storybook for components
- [ ] Unit tests
- [ ] E2E tests (Playwright)

## 🎯 Best Practices Used

### React
- ✅ Functional components
- ✅ TypeScript for type safety
- ✅ Props interfaces
- ✅ Controlled components
- ✅ Proper key usage in lists

### Code Quality
- ✅ Consistent naming
- ✅ Single responsibility
- ✅ Reusable components
- ✅ Separation of concerns (types, services, components)

### User Experience
- ✅ Loading indicators
- ✅ Error messages
- ✅ Confirmations for destructive actions
- ✅ Disabled states
- ✅ Smooth animations

## 📊 Bundle Size

**Approximate:**
- React + ReactDOM: ~140KB (gzipped)
- App code: ~20KB (gzipped)
- **Total: ~160KB**

Very lightweight for a full-featured app!

## 🛠️ Development Workflow

### Hot Module Replacement
Vite provides instant updates:
- Change component → instant refresh
- Change styles → instant refresh
- No full page reload

### TypeScript Benefits
- Catch errors at compile time
- Autocomplete in IDE
- Type-safe API calls
- Refactoring confidence

### Dev Tools
- React DevTools (browser extension)
- Redux DevTools (if added)
- Network tab for API debugging

---

**Frontend Architecture: Simple, Fast, Type-Safe** ⚡
