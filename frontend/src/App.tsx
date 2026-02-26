import { useState } from 'react';
import { GameState, NewGameRequest, TurnRequest } from './types/game';
import { gameService } from './services/gameService';
import CharacterCreation from './components/CharacterCreation';
import GameView from './components/GameView';
import './App.css';

function App() {
  const [gameState, setGameState] = useState<GameState>({
    sessionId: null,
    playerStats: null,
    currentLocation: 'The Infinite Tavern',
    locationType: 'Tavern',
    turnHistory: [],
    isLoading: false,
    error: null,
    suggestedActions: [],
    isInCombat: false,
    isGameOver: false,
    enemies: [],
  });

  const handleCreateCharacter = async (request: NewGameRequest) => {
    setGameState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const response = await gameService.createNewGame(request);

      setGameState({
        sessionId: response.gameSessionId,
        playerStats: response.playerStats,
        currentLocation: 'The Infinite Tavern',
        locationType: response.locationType || 'Tavern',
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
        suggestedActions: response.suggestedActions || [],
        isInCombat: false,
        isGameOver: false,
        enemies: [],
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

      console.log('Turn response:', response);
      console.log('Combat state:', response.isInCombat);
      console.log('Enemies:', response.enemies);

      setGameState((prev) => ({
        ...prev,
        playerStats: prev.playerStats
          ? {
              ...prev.playerStats,
              hp: response.playerHP,
              maxHP: response.maxPlayerHP,
              level: response.playerLevel ?? prev.playerStats.level,
              experience: response.playerExperience ?? prev.playerStats.experience,
              experienceToNextLevel: response.playerExperienceToNextLevel ?? prev.playerStats.experienceToNextLevel,
              inventory: response.inventory,
              gold: response.gold,
            }
          : null,
        currentLocation: response.currentLocation,
        locationType: response.locationType || prev.locationType,
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
        suggestedActions: response.suggestedActions || [],
        isInCombat: response.isInCombat,
        isGameOver: response.isGameOver,
        enemies: response.enemies,
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
    setGameState({
      sessionId: null,
      playerStats: null,
      currentLocation: 'The Infinite Tavern',
      locationType: 'Tavern',
      turnHistory: [],
      isLoading: false,
      error: null,
      suggestedActions: [],
      isInCombat: false,
      isGameOver: false,
      enemies: [],
    });
  };

  return (
    <div className="app">
      {!gameState.sessionId ? (
        <CharacterCreation
          onCreateCharacter={handleCreateCharacter}
          isLoading={gameState.isLoading}
        />
      ) : (
        <GameView gameState={gameState} onSubmitAction={handleSubmitAction} onNewGame={handleNewGame} />
      )}
    </div>
  );
}

export default App;
