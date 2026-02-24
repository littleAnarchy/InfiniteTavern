import { useEffect, useState } from 'react';
import { DiceRollResult } from '../types/game';
import { useTranslation } from 'react-i18next';
import './DiceRoll.css';

interface DiceRollProps {
  roll: DiceRollResult;
  onAnimationComplete?: () => void;
}

export default function DiceRoll({ roll, onAnimationComplete }: DiceRollProps) {
  const { t } = useTranslation();
  const [isRolling, setIsRolling] = useState(true);
  const [displayNumber, setDisplayNumber] = useState(1);

  useEffect(() => {
    // Animate dice rolling
    const rollInterval = setInterval(() => {
      setDisplayNumber(Math.floor(Math.random() * 20) + 1);
    }, 100);

    // Stop rolling after 1 second
    const timeout = setTimeout(() => {
      clearInterval(rollInterval);
      setDisplayNumber(roll.diceRoll);
      setIsRolling(false);
      
      // Call callback after animation completes
      if (onAnimationComplete) {
        setTimeout(onAnimationComplete, 500);
      }
    }, 1000);

    return () => {
      clearInterval(rollInterval);
      clearTimeout(timeout);
    };
  }, [roll.diceRoll, onAnimationComplete]);

  const getAttributeTranslation = (attr: string) => {
    const key = attr.toLowerCase();
    return t(key, attr);
  };

  const modifier = Math.floor((roll.attributeValue - 10) / 2);
  const modifierText = modifier >= 0 ? `+${modifier}` : `${modifier}`;

  return (
    <div className={`dice-roll ${roll.success ? 'success' : 'failure'}`}>
      <div className="dice-roll-header">
        <span className="dice-icon">🎲</span>
        <div className="dice-roll-info">
          <div className="skill-check-label">
            {getAttributeTranslation(roll.attribute)} {t('check')}
          </div>
          <div className="skill-check-purpose">{roll.purpose}</div>
        </div>
      </div>

      <div className="dice-roll-result">
        <div className={`dice ${isRolling ? 'rolling' : ''}`}>
          <div className="dice-face">
            <span className="dice-number">{displayNumber}</span>
          </div>
        </div>

        {!isRolling && (
          <div className="dice-calculation">
            <span className="roll-value">{roll.diceRoll}</span>
            <span className="modifier">{modifierText}</span>
            <span className="equals">=</span>
            <span className="total">{roll.total}</span>
            <span className="vs">vs</span>
            <span className="difficulty">DC {roll.difficulty}</span>
          </div>
        )}
      </div>

      {!isRolling && (
        <div className={`result-badge ${roll.success ? 'success' : 'failure'}`}>
          {roll.success ? `✓ ${t('success')}` : `✗ ${t('failure')}`}
        </div>
      )}
    </div>
  );
}
