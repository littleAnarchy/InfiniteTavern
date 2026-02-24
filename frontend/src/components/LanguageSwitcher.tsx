import { useTranslation } from 'react-i18next';
import './LanguageSwitcher.css';

export default function LanguageSwitcher() {
  const { t, i18n } = useTranslation();

  const changeLanguage = (lng: string) => {
    i18n.changeLanguage(lng);
    localStorage.setItem('locale', lng);
  };

  return (
    <div className="language-switcher">
      <button
        className={i18n.language === 'uk' ? 'active' : ''}
        onClick={() => changeLanguage('uk')}
        title={t('ukrainian')}
      >
        🇺🇦 UA
      </button>
      <button
        className={i18n.language === 'en' ? 'active' : ''}
        onClick={() => changeLanguage('en')}
        title={t('english')}
      >
        🇬🇧 EN
      </button>
    </div>
  );
}
