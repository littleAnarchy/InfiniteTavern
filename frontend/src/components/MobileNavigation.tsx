import './MobileNavigation.css';

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
  return (
    <nav className="mobile-navigation">
      <button
        className={`mobile-nav-tab ${activeTab === 'character' ? 'active' : ''}`}
        onClick={() => onTabChange('character')}
        aria-label="Character"
      >
        <span className="mobile-nav-icon">👤</span>
        <span className="mobile-nav-label">Character</span>
      </button>
      
      <button
        className={`mobile-nav-tab ${activeTab === 'story' ? 'active' : ''}`}
        onClick={() => onTabChange('story')}
        aria-label="Story"
      >
        <span className="mobile-nav-icon">📖</span>
        <span className="mobile-nav-label">Story</span>
        {hasEnemies && <span className="mobile-nav-badge">!</span>}
      </button>
      
      <button
        className={`mobile-nav-tab ${activeTab === 'inventory' ? 'active' : ''}`}
        onClick={() => onTabChange('inventory')}
        aria-label="Inventory"
      >
        <span className="mobile-nav-icon">🎒</span>
        <span className="mobile-nav-label">Inventory</span>
      </button>
    </nav>
  );
}
