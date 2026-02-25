import { useState } from 'react';
import { NewGameRequest } from '../types/game';
import { useTranslation } from 'react-i18next';

interface CharacterCreationProps {
  onCreateCharacter: (request: NewGameRequest) => void;
  isLoading: boolean;
}

const RACES = ['Human', 'Elf', 'Dwarf', 'Orc', 'Halfling'];
const CLASSES = ['Warrior', 'Wizard', 'Rogue', 'Cleric', 'Ranger'];

export default function CharacterCreation({
  onCreateCharacter,
  isLoading,
}: CharacterCreationProps) {
  const { t, i18n } = useTranslation();
  const [characterName, setCharacterName] = useState('');
  const [race, setRace] = useState(RACES[0]);
  const [characterClass, setCharacterClass] = useState(CLASSES[0]);
  const [gameLanguage, setGameLanguage] = useState(
    i18n.language === 'uk' ? 'Ukrainian' : 'English'
  );
  const [useDefaultCampaign, setUseDefaultCampaign] = useState(true);

  const translateRace = (race: string) => t(race.toLowerCase(), race);
  const translateClass = (cls: string) => t(cls.toLowerCase(), cls);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onCreateCharacter({
      characterName,
      race,
      class: characterClass,
      language: gameLanguage,
      useDefaultCampaign,
    });
  };

  return (
    <div className="character-creation">
      <h1>{t('welcomeTitle')}</h1>
      <p className="subtitle">{t('welcomeSubtitle')}</p>

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="characterName">{t('characterName')}</label>
          <input
            id="characterName"
            type="text"
            value={characterName}
            onChange={(e) => setCharacterName(e.target.value)}
            placeholder={t('enterCharacterName')}
            required
            disabled={isLoading}
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="race">{t('race')}</label>
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
            <label htmlFor="class">{t('class')}</label>
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
          <label htmlFor="gameLanguage">{t('gameLanguage')}</label>
          <select
            id="gameLanguage"
            value={gameLanguage}
            onChange={(e) => setGameLanguage(e.target.value)}
            disabled={isLoading}
          >
            <option value="Ukrainian">{t('ukrainian')}</option>
            <option value="English">{t('english')}</option>
          </select>
          <small style={{ display: 'block', marginTop: '2px', opacity: 0.7, fontSize: '0.85rem' }}>
            {t('selectLanguage')}
          </small>
        </div>

        <div className="form-group">
          <label>{t('campaignType')}</label>
          <div className="campaign-type-options">
            <label className="campaign-type-option">
              <input
                type="radio"
                name="campaignType"
                value="default"
                checked={useDefaultCampaign}
                onChange={() => setUseDefaultCampaign(true)}
                disabled={isLoading}
              />
              <div className="campaign-details">
                <strong>{t('defaultCampaign')}</strong>
                <small>{t('defaultCampaignDesc')}</small>
              </div>
            </label>
            <label className="campaign-type-option">
              <input
                type="radio"
                name="campaignType"
                value="random"
                checked={!useDefaultCampaign}
                onChange={() => setUseDefaultCampaign(false)}
                disabled={isLoading}
              />
              <div className="campaign-details">
                <strong>{t('randomCampaign')}</strong>
                <small>{t('randomCampaignDesc')}</small>
              </div>
            </label>
          </div>
        </div>

        <button type="submit" className="btn-primary" disabled={isLoading}>
          {isLoading ? t('creating') : t('beginAdventure')}
        </button>

        {isLoading && (
          <div className="loading-story">
            <div className="loading-spinner"></div>
            <p>{t('creatingStory')}</p>
          </div>
        )}
      </form>
    </div>
  );
}
