import { useState } from 'react';
import { GameState } from '../types/game';
import { useTranslation } from 'react-i18next';
import PlayerStats from './PlayerStats';
import TurnHistory from './TurnHistory';
import LanguageSwitcher from './LanguageSwitcher';
import Inventory from './Inventory';

interface GameViewProps {
  gameState: GameState;
  onSubmitAction: (action: string) => void;
  onNewGame: () => void;
}

export default function GameView({ gameState, onSubmitAction, onNewGame }: GameViewProps) {
  const { t } = useTranslation();
  const [action, setAction] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (action.trim() && !gameState.isLoading) {
      onSubmitAction(action);
      setAction('');
    }
  };

  const handleSuggestedAction = (suggestedAction: string) => {
    if (!gameState.isLoading) {
      onSubmitAction(suggestedAction);
    }
  };

  if (!gameState.playerStats) {
    return null;
  }

  return (
    <div className="game-view">
      <div className="game-header">
        <h1>{t('gameTitle')}</h1>
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
          <LanguageSwitcher />
          <button onClick={onNewGame} className="btn-secondary" disabled={gameState.isLoading}>
            {t('newGame')}
          </button>
        </div>
      </div>

      <div className="game-layout">
        <aside className="sidebar">
          <PlayerStats stats={gameState.playerStats} currentLocation={gameState.currentLocation} />
        </aside>

        <main className="main-content">
          <div className="history-container">
            <TurnHistory history={gameState.turnHistory} isLoading={gameState.isLoading} />
          </div>

          {gameState.suggestedActions.length > 0 && (
            <div className="suggested-actions">
              <p className="suggested-actions-label">{t('suggestedActions')}:</p>
              <div className="suggested-actions-buttons">
                {gameState.suggestedActions.map((suggestedAction, index) => (
                  <button
                    key={index}
                    onClick={() => handleSuggestedAction(suggestedAction)}
                    disabled={gameState.isLoading}
                    className="btn-suggested"
                  >
                    {suggestedAction}
                  </button>
                ))}
              </div>
            </div>
          )}

          <form onSubmit={handleSubmit} className="action-form">
            <input
              type="text"
              value={action}
              onChange={(e) => setAction(e.target.value)}
              placeholder={t('whatDoYouDo')}
              disabled={gameState.isLoading}
              className="action-input"
            />
            <button type="submit" disabled={gameState.isLoading || !action.trim()} className="btn-primary">
              {gameState.isLoading ? '...' : t('act')}
            </button>
          </form>

          {gameState.error && <div className="error-message">{gameState.error}</div>}
        </main>

        <aside className="sidebar sidebar-right">
          <Inventory inventory={gameState.playerStats.inventory} gold={gameState.playerStats.gold} />
        </aside>
      </div>
    </div>
  );
}
