import { PlayerStats as PlayerStatsType } from '../types/game';
import { useTranslation } from 'react-i18next';

interface PlayerStatsProps {
  stats: PlayerStatsType;
  currentLocation: string;
}

export default function PlayerStats({ stats, currentLocation }: PlayerStatsProps) {
  const { t } = useTranslation();
  const hpPercentage = (stats.hp / stats.maxHP) * 100;
  const hpColor = hpPercentage > 50 ? '#4caf50' : hpPercentage > 25 ? '#ff9800' : '#f44336';

  const translateRace = (race: string) => t(race.toLowerCase(), race);
  const translateClass = (cls: string) => t(cls.toLowerCase(), cls);

  return (
    <div className="player-stats">
      <div className="stats-header">
        <h2>{stats.name}</h2>
        <p className="character-info">
          {t('level')} {stats.level} {translateRace(stats.race)} {translateClass(stats.class)}
        </p>
      </div>

      <div className="hp-bar">
        <div className="hp-label">
          <span>{t('hp')}</span>
          <span>
            {stats.hp} / {stats.maxHP}
          </span>
        </div>
        <div className="hp-progress">
          <div
            className="hp-fill"
            style={{
              width: `${hpPercentage}%`,
              backgroundColor: hpColor,
            }}
          />
        </div>
      </div>

      <div className="stats-grid">
        <div className="stat-item">
          <span className="stat-label">{t('str')}</span>
          <span className="stat-value">{stats.strength}</span>
        </div>
        <div className="stat-item">
          <span className="stat-label">{t('dex')}</span>
          <span className="stat-value">{stats.dexterity}</span>
        </div>
        <div className="stat-item">
          <span className="stat-label">{t('int')}</span>
          <span className="stat-value">{stats.intelligence}</span>
        </div>
      </div>

      <div className="location">
        <span className="location-icon">📍</span>
        <span>{currentLocation}</span>
      </div>
    </div>
  );
}
