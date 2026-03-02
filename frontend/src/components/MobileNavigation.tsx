import './MobileNavigation.css';
import { useTranslation } from 'react-i18next';

interface MobileNavigationProps {
  activeTab: 'story' | 'character' | 'inventory';
  onTabChange: (tab: 'story' | 'character' | 'inventory') => void;
  hasEnemies: boolean;
}

export default function MobileNavigation({ 
  activeTab, 
  onTabChange,
  hasEnemies 
}: MobileNavigationProps) {
  const { t } = useTranslation();
  
  return (
    <nav className="mobile-navigation">
      <button
        className={`mobile-nav-tab ${activeTab === 'character' ? 'active' : ''}`}
        onClick={() => onTabChange('character')}
        aria-label={t('stats')}
      >
        <span className="mobile-nav-icon">👤</span>
        <span className="mobile-nav-label">{t('stats')}</span>
      </button>
      
      <button
        className={`mobile-nav-tab ${activeTab === 'story' ? 'active' : ''}`}
        onClick={() => onTabChange('story')}
        aria-label={t('story')}
      >
        <span className="mobile-nav-icon">📖</span>
        <span className="mobile-nav-label">{t('story')}</span>
        {hasEnemies && <span className="mobile-nav-badge">!</span>}
      </button>
      
      <button
        className={`mobile-nav-tab ${activeTab === 'inventory' ? 'active' : ''}`}
        onClick={() => onTabChange('inventory')}
        aria-label={t('inventory')}
      >
        <span className="mobile-nav-icon">💼</span>
        <span className="mobile-nav-label">{t('inventory')}</span>
      </button>
    </nav>
  );
}
