import { Item } from '../types/game';
import { useLocale } from '../contexts/LocaleContext';
import './Inventory.css';

interface InventoryProps {
  inventory: Item[];
  gold: number;
}

export default function Inventory({ inventory, gold }: InventoryProps) {
  const { t } = useLocale();

  const getItemIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'weapon':
        return '⚔️';
      case 'armor':
        return '🛡️';
      case 'potion':
        return '🧪';
      default:
        return '📦';
    }
  };

  const getItemTypeTranslation = (type: string) => {
    const key = `itemType${type.charAt(0).toUpperCase()}${type.slice(1).toLowerCase()}`;
    return (t as any)[key] || type;
  };

  return (
    <div className="inventory">
      <div className="inventory-header">
        <h3>💼 {t.inventory}</h3>
        <div className="gold-display">
          <span className="gold-icon">💰</span>
          <span className="gold-amount">{gold}</span>
        </div>
      </div>

      <div className="inventory-list">
        {inventory.length === 0 ? (
          <div className="empty-inventory">
            <p>{t.emptyInventory}</p>
          </div>
        ) : (
          inventory.map((item, index) => (
            <div
              key={index}
              className={`inventory-item ${item.isEquipped ? 'equipped' : ''}`}
            >
              <div className="item-icon">{getItemIcon(item.type)}</div>
              <div className="item-details">
                <div className="item-header">
                  <span className="item-name">
                    {item.name}
                    {item.quantity > 1 && (
                      <span className="item-quantity"> x{item.quantity}</span>
                    )}
                  </span>
                  {item.isEquipped && (
                    <span className="equipped-badge">{t.equipped}</span>
                  )}
                </div>
                <div className="item-type">{getItemTypeTranslation(item.type)}</div>
                <div className="item-description">{item.description}</div>
                {Object.keys(item.bonuses).length > 0 && (
                  <div className="item-bonuses">
                    {Object.entries(item.bonuses).map(([stat, value]) => (
                      <span key={stat} className="bonus">
                        +{value} {stat}
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
