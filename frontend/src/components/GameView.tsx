import { useState } from 'react';
import { GameState } from '../types/game';
import { useTranslation } from 'react-i18next';
import PlayerStats from './PlayerStats';
import TurnHistory from './TurnHistory';
import LanguageSwitcher from './LanguageSwitcher';
import Inventory from './Inventory';
import EnemyList from './EnemyList';

interface GameViewProps {
  gameState: GameState;
  onSubmitAction: (action: string) => void;
  onNewGame: () => void;
}

export default function GameView({ gameState, onSubmitAction, onNewGame }: GameViewProps) {
  const { t } = useTranslation();
  const [action, setAction] = useState('');
  const [leftTab, setLeftTab] = useState<'stats' | 'inventory'>('stats');

  console.log('GameView - isInCombat:', gameState.isInCombat, 'enemies:', gameState.enemies);

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

      <div className={`game-layout ${gameState.isInCombat ? 'in-combat' : ''}`}>
        <aside className="sidebar">
          <div className="sidebar-tabs">
            <button
              className={`tab-button ${leftTab === 'stats' ? 'active' : ''}`}
              onClick={() => setLeftTab('stats')}
            >
              📊 {t('stats')}
            </button>
            <button
              className={`tab-button ${leftTab === 'inventory' ? 'active' : ''}`}
              onClick={() => setLeftTab('inventory')}
            >
              💼 {t('inventory')}
            </button>
          </div>
          
          <div className="sidebar-content">
            {leftTab === 'stats' ? (
              <PlayerStats stats={gameState.playerStats} currentLocation={gameState.currentLocation} />
            ) : (
              <Inventory
                inventory={gameState.playerStats.inventory}
                gold={gameState.playerStats.gold}
              />
            )}
          </div>
        </aside>

        <main className="main-content">
          <div className="history-container">
            <TurnHistory history={gameState.turnHistory} isLoading={gameState.isLoading} />
          </div>

          <form onSubmit={handleSubmit} className="action-form">
            <div className="action-panel">
              <input
                type="text"
                value={action}
                onChange={(e) => setAction(e.target.value)}
                placeholder={t('whatDoYouDo')}
                disabled={gameState.isLoading}
                className="action-input"
              />
              <button
                type="submit"
                disabled={gameState.isLoading || !action.trim()}
                className="btn-primary"
              >
                {gameState.isLoading ? '...' : t('act')}
              </button>
            </div>
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
          </form>

          {gameState.error && <div className="error-message">{gameState.error}</div>}
        </main>

        {gameState.isInCombat && (
          <aside className="sidebar sidebar-right">
            <EnemyList enemies={gameState.enemies} />
          </aside>
        )}
      </div>
    </div>
  );
}
