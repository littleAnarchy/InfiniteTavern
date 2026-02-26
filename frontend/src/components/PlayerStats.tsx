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
  const xpPercentage = stats.experienceToNextLevel > 0
    ? Math.min(100, (stats.experience / stats.experienceToNextLevel) * 100)
    : 0;

  const translateRace = (race: string) => t(race.toLowerCase(), race);
  const translateClass = (cls: string) => t(cls.toLowerCase(), cls);

  const defenseLabel = t('defense');

  return (
    <div className="player-stats" style={{ position: 'relative' }}>
      <div
        className="defense-badge"
        title={defenseLabel}
        aria-label={defenseLabel}
      >
        <svg viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 2L4 5v6c0 5.25 3.5 10.15 8 11.35C16.5 21.15 20 16.25 20 11V5l-8-3z"/>
        </svg>
        <span className="defense-badge__value">{stats.defense}</span>
        <span className="defense-badge__tooltip">{defenseLabel}</span>
      </div>
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

      <div className="hp-bar">
        <div className="hp-label">
          <span>{t('xp')}</span>
          <span>
            {stats.experience} / {stats.experienceToNextLevel}
          </span>
        </div>
        <div className="hp-progress">
          <div
            className="hp-fill"
            style={{
              width: `${xpPercentage}%`,
              backgroundColor: '#9c27b0',
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
          <span className="stat-label">{t('con')}</span>
          <span className="stat-value">{stats.constitution}</span>
        </div>
        <div className="stat-item">
          <span className="stat-label">{t('int')}</span>
          <span className="stat-value">{stats.intelligence}</span>
        </div>
        <div className="stat-item">
          <span className="stat-label">{t('wis')}</span>
          <span className="stat-value">{stats.wisdom}</span>
        </div>
        <div className="stat-item">
          <span className="stat-label">{t('cha')}</span>
          <span className="stat-value">{stats.charisma}</span>
        </div>
      </div>

      <div className="location">
        <span className="location-icon">📍</span>
        <span>{currentLocation}</span>
      </div>
    </div>
  );
}
