import './CompactHPBar.css';

interface CompactHPBarProps {
  characterName: string;
  hp: number;
  maxHP: number;
  level: number;
}

export default function CompactHPBar({ characterName, hp, maxHP, level }: CompactHPBarProps) {
  const hpPercentage = (hp / maxHP) * 100;
  const isLowHP = hpPercentage <= 30;
  const isCriticalHP = hpPercentage <= 15;

  return (
    <div className="compact-hp-bar">
      <span className="compact-hp-name">{characterName}</span>
      <span className="compact-hp-level">Lv.{level}</span>
      <div className="compact-hp-bar-container">
        <div 
          className={`compact-hp-bar-fill ${isLowHP ? 'low' : ''} ${isCriticalHP ? 'critical' : ''}`}
          style={{ width: `${hpPercentage}%` }}
        />
        <span className="compact-hp-text">
          {hp} / {maxHP}
        </span>
      </div>
    </div>
  );
}
