import { Enemy } from '../types/game';
import { useTranslation } from 'react-i18next';
import './EnemyList.css';

interface EnemyListProps {
  enemies: Enemy[];
}

function EnemyList({ enemies }: EnemyListProps) {
  const { t } = useTranslation();

  console.log('EnemyList received enemies:', enemies);
  console.log('EnemyList enemies.length:', enemies.length);

  if (enemies.length === 0) {
    console.log('EnemyList: No enemies, returning null');
    return null;
  }

  const aliveEnemies = enemies.filter((e) => e.isAlive);
  console.log('EnemyList aliveEnemies:', aliveEnemies);
  console.log('EnemyList aliveEnemies.length:', aliveEnemies.length);

  if (aliveEnemies.length === 0) {
    console.log('EnemyList: No alive enemies, returning null');
    return null;
  }

  console.log('EnemyList: RENDERING enemy list!');

  return (
    <div className="enemy-list">
      <h3>{t('enemies')}</h3>
      {aliveEnemies.map((enemy, index) => (
        <div key={index} className="enemy-card">
          <div className="enemy-header">
            <span className="enemy-name">{enemy.name}</span>
            <span className="enemy-hp">
              {enemy.hp}/{enemy.maxHP} HP
            </span>
          </div>
          <div className="enemy-hp-bar">
            <div
              className="enemy-hp-fill"
              style={{ width: `${(enemy.hp / enemy.maxHP) * 100}%` }}
            />
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '0.25rem' }}>
            {enemy.description && (
              <p className="enemy-description" style={{ margin: 0, flex: 1 }}>{enemy.description}</p>
            )}
            {enemy.attack > 0 && (
              <span style={{ fontSize: '0.72rem', color: '#ef9a9a', marginLeft: '0.5rem', whiteSpace: 'nowrap' }}>
                ⚔️ {t('atk')} {enemy.attack}
              </span>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}

export default EnemyList;
