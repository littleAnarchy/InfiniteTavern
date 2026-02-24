import { useLocale } from '../contexts/LocaleContext';
import './LanguageSwitcher.css';

export default function LanguageSwitcher() {
  const { locale, setLocale, t } = useLocale();

  return (
    <div className="language-switcher">
      <button
        className={locale === 'uk' ? 'active' : ''}
        onClick={() => setLocale('uk')}
        title={t.ukrainian}
      >
        🇺🇦 UA
      </button>
      <button
        className={locale === 'en' ? 'active' : ''}
        onClick={() => setLocale('en')}
        title={t.english}
      >
        🇬🇧 EN
      </button>
    </div>
  );
}
