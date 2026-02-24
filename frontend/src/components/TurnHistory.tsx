import { TurnHistoryEntry } from '../types/game';
import { useTranslation } from 'react-i18next';
import DiceRoll from './DiceRoll';

interface TurnHistoryProps {
  history: TurnHistoryEntry[];
  isLoading: boolean;
}

export default function TurnHistory({ history, isLoading }: TurnHistoryProps) {
  const { t } = useTranslation();
  
  return (
    <div className="turn-history">
      {history.length === 0 && !isLoading && (
        <div className="empty-state">
          <p>{t('adventureBegins')}</p>
          <p className="hint">{t('firstActionHint')}</p>
        </div>
      )}

      {history.map((entry, index) => (
        <div key={index} className="turn-entry">
          <div className="player-action">
            <span className="action-icon">🎮</span>
            <span className="action-text">{entry.playerAction}</span>
          </div>

          <div className="narrative">
            <p>{entry.narrative}</p>
          </div>

          {entry.events.length > 0 && (
            <div className="events">
              {entry.events.map((event, eventIndex) => (
                <div key={eventIndex} className="event-item">
                  <span className="event-icon">⚡</span>
                  <span>{event}</span>
                </div>
              ))}
            </div>
          )}

          {entry.diceRolls && entry.diceRolls.length > 0 && (
            <div className="dice-rolls">
              {entry.diceRolls.map((roll, rollIndex) => (
                <DiceRoll key={rollIndex} roll={roll} />
              ))}
            </div>
          )}
        </div>
      ))}

      {isLoading && (
        <div className="turn-entry loading">
          <div className="loading-spinner"></div>
          <p>{t('dmThinking')}</p>
        </div>
      )}
    </div>
  );
}
