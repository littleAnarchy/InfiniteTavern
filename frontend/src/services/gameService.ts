import { NewGameRequest, NewGameResponse, TurnRequest, TurnResponse } from '../types/game';

const API_BASE_URL = '/api/game';

export const gameService = {
  async createNewGame(request: NewGameRequest): Promise<NewGameResponse> {
    const response = await fetch(`${API_BASE_URL}/new-game`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(`Failed to create game: ${error}`);
    }

    return response.json();
  },

  async processTurn(request: TurnRequest): Promise<TurnResponse> {
    const response = await fetch(`${API_BASE_URL}/turn`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(`Failed to process turn: ${error}`);
    }

    return response.json();
  },
};
