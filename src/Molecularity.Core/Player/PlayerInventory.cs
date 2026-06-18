using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Items;

namespace Molecularity.Core.Player {
    public class PlayerInventory {
        private readonly Dictionary<LevelItemType, List<ILevelItem>> _items = new();

        public IReadOnlyDictionary<LevelItemType, IReadOnlyList<ILevelItem>> Items =>
            _items.ToDictionary(k => k.Key, v => (IReadOnlyList<ILevelItem>)v.Value);

        public void Add(ILevelItem item) {
            if (!_items.ContainsKey(item.Type)) {
                _items[item.Type] = new List<ILevelItem>();
            }

            _items[item.Type].Add(item);
        }

        public void Remove(ILevelItem item) {
            if (_items.TryGetValue(item.Type, out List<ILevelItem>? gotItems)) {
                gotItems.Remove(item);
            }
        }

        public int Count(LevelItemType type) {
            return _items.TryGetValue(type, out List<ILevelItem>? gotItems) ? gotItems.Count : 0;
        }

        public ILevelItem? GetItem(LevelItemType type) {
            if (_items.TryGetValue(type, out List<ILevelItem>? gotItems) && gotItems.Count > 0) {
                return gotItems[0];
            }

            return null;
        }
    }
}
