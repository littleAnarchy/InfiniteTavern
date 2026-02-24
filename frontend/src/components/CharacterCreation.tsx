import { useState } from 'react';
import { NewGameRequest } from '../types/game';
import { useLocale } from '../contexts/LocaleContext';

interface CharacterCreationProps {
  onCreateCharacter: (request: NewGameRequest) => void;
  isLoading: boolean;
}

const RACES = ['Human', 'Elf', 'Dwarf', 'Orc', 'Halfling'];
const CLASSES = ['Warrior', 'Wizard', 'Rogue', 'Cleric', 'Ranger'];

export default function CharacterCreation({ onCreateCharacter, isLoading }: CharacterCreationProps) {
  const { t, locale } = useLocale();
  const [playerName, setPlayerName] = useState('');
  const [characterName, setCharacterName] = useState('');
  const [race, setRace] = useState(RACES[0]);
  const [characterClass, setCharacterClass] = useState(CLASSES[0]);
  const [gameLanguage, setGameLanguage] = useState(locale === 'uk' ? 'Ukrainian' : 'English');

  const translateRace = (race: string) => (t as any)[race.toLowerCase()] || race;
  const translateClass = (cls: string) => (t as any)[cls.toLowerCase()] || cls;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onCreateCharacter({
      playerName,
      characterName,
      race,
      class: characterClass,
      language: gameLanguage,
    });
  };

  return (
    <div className="character-creation">
      <h1>{t.welcomeTitle}</h1>
      <p className="subtitle">{t.welcomeSubtitle}</p>

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="playerName">{t.yourName}</label>
          <input
            id="playerName"
            type="text"
            value={playerName}
            onChange={(e) => setPlayerName(e.target.value)}
            placeholder={t.enterYourName}
            required
            disabled={isLoading}
          />
        </div>

        <div className="form-group">
          <label htmlFor="characterName">{t.characterName}</label>
          <input
            id="characterName"
            type="text"
            value={characterName}
            onChange={(e) => setCharacterName(e.target.value)}
            placeholder={t.enterCharacterName}
            required
            disabled={isLoading}
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="race">{t.race}</label>
            <select
              id="race"
              value={race}
              onChange={(e) => setRace(e.target.value)}
              disabled={isLoading}
            >
              {RACES.map((r) => (
                <option key={r} value={r}>
                  {translateRace(r)}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="class">{t.class}</label>
            <select
              id="class"
              value={characterClass}
              onChange={(e) => setCharacterClass(e.target.value)}
              disabled={isLoading}
            >
              {CLASSES.map((c) => (
                <option key={c} value={c}>
                  {translateClass(c)}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="gameLanguage">{t.gameLanguage}</label>
          <select
            id="gameLanguage"
            value={gameLanguage}
            onChange={(e) => setGameLanguage(e.target.value)}
            disabled={isLoading}
          >
            <option value="Ukrainian">{t.ukrainian}</option>
            <option value="English">{t.english}</option>
          </select>
          <small style={{ display: 'block', marginTop: '4px', opacity: 0.7 }}>
            {t.selectLanguage}
          </small>
        </div>

        <button type="submit" className="btn-primary" disabled={isLoading}>
          {isLoading ? t.creating : t.beginAdventure}
        </button>

        {isLoading && (
          <div className="loading-story">
            <div className="loading-spinner"></div>
            <p>{t.creatingStory}</p>
          </div>
        )}
      </form>
    </div>
  );
}
