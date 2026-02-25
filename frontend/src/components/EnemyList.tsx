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
          {enemy.description && (
            <p className="enemy-description">{enemy.description}</p>
          )}
        </div>
      ))}
    </div>
  );
}

export default EnemyList;
