import { useState } from 'react';
import { GameState, NewGameRequest, TurnRequest } from './types/game';
import { gameService } from './services/gameService';
import { useLocale } from './contexts/LocaleContext';
import CharacterCreation from './components/CharacterCreation';
import GameView from './components/GameView';
import './App.css';

function App() {
  const { t } = useLocale();
  const [gameState, setGameState] = useState<GameState>({
    sessionId: null,
    playerStats: null,
    currentLocation: 'The Infinite Tavern',
    turnHistory: [],
    isLoading: false,
    error: null,
  });

  const handleCreateCharacter = async (request: NewGameRequest) => {
    setGameState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const response = await gameService.createNewGame(request);
      
      setGameState({
        sessionId: response.gameSessionId,
        playerStats: response.playerStats,
        currentLocation: 'The Infinite Tavern',
        turnHistory: [
          {
            playerAction: 'Entered the tavern',
            narrative: response.message,
            events: [],
            location: 'The Infinite Tavern',
          },
        ],
        isLoading: false,
        error: null,
      });
    } catch (error) {
      setGameState((prev) => ({
        ...prev,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Failed to create game',
      }));
    }
  };

  const handleSubmitAction = async (action: string) => {
    if (!gameState.sessionId) return;

    setGameState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const request: TurnRequest = {
        gameSessionId: gameState.sessionId,
        playerAction: action,
      };

      const response = await gameService.processTurn(request);

      setGameState((prev) => ({
        ...prev,
        playerStats: prev.playerStats
          ? { 
              ...prev.playerStats, 
              hp: response.playerHP, 
              maxHP: response.maxPlayerHP,
              inventory: response.inventory,
              gold: response.gold
            }
          : null,
        currentLocation: response.currentLocation,
        turnHistory: [
          ...prev.turnHistory,
          {
            playerAction: action,
            narrative: response.narrative,
            events: response.appliedEvents,
            location: response.currentLocation,
            diceRolls: response.diceRolls,
          },
        ],
        isLoading: false,
      }));
    } catch (error) {
      setGameState((prev) => ({
        ...prev,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Failed to process turn',
      }));
    }
  };

  const handleNewGame = () => {
    if (window.confirm(t.confirmNewGame)) {
      setGameState({
        sessionId: null,
        playerStats: null,
        currentLocation: 'The Infinite Tavern',
        turnHistory: [],
        isLoading: false,
        error: null,
      });
    }
  };

  return (
    <div className="app">
      {!gameState.sessionId ? (
        <CharacterCreation onCreateCharacter={handleCreateCharacter} isLoading={gameState.isLoading} />
      ) : (
        <GameView gameState={gameState} onSubmitAction={handleSubmitAction} onNewGame={handleNewGame} />
      )}
    </div>
  );
}

export default App;
