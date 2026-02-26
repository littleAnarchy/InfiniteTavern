import { Item } from '../types/game';
import { useTranslation } from 'react-i18next';
import './Inventory.css';

interface InventoryProps {
  inventory: Item[];
  gold: number;
  hideTitle?: boolean;
  onEquipItem?: (itemName: string) => void;
}

const EQUIPPABLE_TYPES = new Set(['weapon', 'armor', 'shield', 'helmet', 'boots', 'amulet', 'ring', 'accessory']);

export default function Inventory({ inventory, gold, hideTitle, onEquipItem }: InventoryProps) {
  const { t } = useTranslation();

  const getItemIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'weapon': return '⚔️';
      case 'armor': return '🛡️';
      case 'shield': return '🛡️';
      case 'helmet': return '⛑️';
      case 'boots': return '👢';
      case 'amulet': return '📿';
      case 'ring': return '💍';
      case 'accessory': return '✨';
      case 'potion': return '🧪';
      case 'scroll': return '📜';
      case 'key': return '🗝️';
      default: return '📦';
    }
  };

  const getItemTypeLabel = (type: string): string => {
    const key = `itemType${type.charAt(0).toUpperCase()}${type.slice(1).toLowerCase()}`;
    const translated = t(key);
    return translated === key ? type : translated;
  };

  const getStatLabel = (stat: string): string => {
    const key = stat.toLowerCase();
    const translated = t(key);
    return translated === key ? stat : translated;
  };

  return (
    <div className="inventory">
      <div className={`inventory-header ${hideTitle ? 'inventory-header--no-title' : ''}`}>
        {!hideTitle && (
          <div className="inventory-title">
            <span className="inventory-title-icon">💼</span>
            <span className="inventory-title-text">{t('inventory')}</span>
          </div>
        )}
        <div className="gold-display">
          <span className="gold-icon">💰</span>
          <span className="gold-amount">{gold}</span>
        </div>
      </div>

      <div className="inventory-list">
        {inventory.length === 0 ? (
          <div className="empty-inventory">
            <p>{t('emptyInventory')}</p>
          </div>
        ) : (
          inventory.map((item, index) => (
            <div key={index} className={`inventory-item ${item.isEquipped ? 'equipped' : ''}`}>
              <div className="item-icon">{getItemIcon(item.type)}</div>
              <div className="item-details">
                <div className="item-header">
                  <span className="item-name">
                    {item.name}
                    {item.quantity > 1 && <span className="item-quantity"> x{item.quantity}</span>}
                  </span>
                  <div className="item-header-badges">
                    {item.isEquipped && <span className="equipped-badge">{t('equipped')}</span>}
                    {EQUIPPABLE_TYPES.has(item.type.toLowerCase()) && onEquipItem && (
                      <button
                        className={`equip-btn ${item.isEquipped ? 'equip-btn--unequip' : ''}`}
                        onClick={() => onEquipItem(item.name)}
                        title={item.isEquipped ? t('unequip') : t('equip')}
                      >
                        {item.isEquipped ? t('unequip') : t('equip')}
                      </button>
                    )}
                  </div>
                </div>
                <div className="item-type">{getItemTypeLabel(item.type)}</div>
                <div className="item-description">{item.description}</div>
                {Object.keys(item.bonuses).length > 0 && (
                  <div className="item-bonuses">
                    {Object.entries(item.bonuses).map(([stat, value]) => (
                      <span key={stat} className="bonus">
                        +{value} {getStatLabel(stat)}
                      </span>
                    ))}
                  </div>
                )}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}

