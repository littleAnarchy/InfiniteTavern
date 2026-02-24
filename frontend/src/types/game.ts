// API Request Types
export interface NewGameRequest {
  characterName: string;
  race: string;
  class: string;
  language: string;
}

export interface TurnRequest {
  gameSessionId: string;
  playerAction: string;
}

// API Response Types
export interface Item {
  name: string;
  type: string;
  description: string;
  quantity: number;
  isEquipped: boolean;
  bonuses: Record<string, number>;
}

export interface PlayerStats {
  name: string;
  race: string;
  class: string;
  level: number;
  hp: number;
  maxHP: number;
  strength: number;
  dexterity: number;
  intelligence: number;
  inventory: Item[];
  gold: number;
}

export interface NewGameResponse {
  gameSessionId: string;
  message: string;
  playerStats: PlayerStats;
}

export interface TurnResponse {
  narrative: string;
  playerHP: number;
  maxPlayerHP: number;
  currentLocation: string;
  appliedEvents: string[];
  inventory: Item[];
  gold: number;
  diceRolls: DiceRollResult[];
}

export interface DiceRollResult {
  attribute: string;
  attributeValue: number;
  diceRoll: number;
  total: number;
  difficulty: number;
  success: boolean;
  purpose: string;
}

// UI Types
export interface TurnHistoryEntry {
  playerAction: string;
  narrative: string;
  events: string[];
  location: string;
  diceRolls?: DiceRollResult[];
}

export interface GameState {
  sessionId: string | null;
  playerStats: PlayerStats | null;
  currentLocation: string;
  turnHistory: TurnHistoryEntry[];
  isLoading: boolean;
  error: string | null;
}
