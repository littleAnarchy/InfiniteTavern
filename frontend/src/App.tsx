import { useState, useEffect } from 'react';
import { GameState, NewGameRequest, TurnRequest, EquipItemRequest } from './types/game';
import { gameService } from './services/gameService';
import CharacterCreation from './components/CharacterCreation';
import GameView from './components/GameView';
import './App.css';

function useIsMobile() {
  const [isMobile, setIsMobile] = useState(() => window.innerWidth < 1024);
  useEffect(() => {
    const handler = () => setIsMobile(window.innerWidth < 1024);
    window.addEventListener('resize', handler);
    return () => window.removeEventListener('resize', handler);
  }, []);
  return isMobile;
}

function App() {
  const isMobile = useIsMobile();
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

  const handleEquipItem = async (itemName: string) => {
    if (!gameState.sessionId) return;
    const request: EquipItemRequest = { gameSessionId: gameState.sessionId, itemName };
    try {
      const response = await gameService.equipItem(request);
      setGameState((prev) => ({
        ...prev,
        playerStats: prev.playerStats
          ? {
              ...prev.playerStats,
              strength: response.strength,
              dexterity: response.dexterity,
              constitution: response.constitution,
              intelligence: response.intelligence,
              wisdom: response.wisdom,
              charisma: response.charisma,
              defense: response.defense,
              inventory: response.inventory,
            }
          : null,
      }));
    } catch (error) {
      console.error('Failed to equip item:', error);
    }
  };

  return (
    <div className="app">
      {isMobile && (
        <div style={{
          position: 'fixed', inset: 0, zIndex: 9999,
          background: 'linear-gradient(135deg, #1a0a00 0%, #2d1200 50%, #1a0a00 100%)',
          display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
          padding: '2rem', textAlign: 'center',
        }}>
          <div style={{ fontSize: '4rem', marginBottom: '1.5rem' }}>⚔️</div>
          <h1 style={{ color: '#d4a853', fontFamily: 'serif', fontSize: '1.6rem', marginBottom: '1rem' }}>
            Infinite Tavern
          </h1>
          <p style={{ color: '#c8a882', fontSize: '1.1rem', lineHeight: 1.6, maxWidth: '320px', marginBottom: '0.75rem' }}>
            Цей сайт ще не адаптований під мобільні пристрої.
          </p>
          <p style={{ color: '#8a6a45', fontSize: '0.95rem', lineHeight: 1.6, maxWidth: '320px' }}>
            Будь ласка, відкрийте його на комп'ютері для найкращого досвіду.
          </p>
          <div style={{ marginTop: '2rem', color: '#5a4030', fontSize: '0.85rem' }}>🖥️ Desktop only</div>
        </div>
      )}
      {!gameState.sessionId ? (
        <CharacterCreation
          onCreateCharacter={handleCreateCharacter}
          isLoading={gameState.isLoading}
        />
      ) : (
        <GameView gameState={gameState} onSubmitAction={handleSubmitAction} onNewGame={handleNewGame} onEquipItem={handleEquipItem} />
      )}
    </div>
  );
}

export default App;
