using System.Collections.Generic;
using Molecularity.Core.Items;

namespace Molecularity.Web.Services {

    public sealed record ItemMetaInfo(string Icon, string Name, string Desc, int Targets);

    public static class ItemMeta {
        public static readonly IReadOnlyDictionary<LevelItemType, ItemMetaInfo> All =
            new Dictionary<LevelItemType, ItemMetaInfo> {
                [LevelItemType.RevealAll]  = new("◉",  "Проявитель",   "Открыть все значения",    0),
                [LevelItemType.PlusOneAll] = new("+1", "Катализатор",  "+1 каждой молекуле",      0),
                [LevelItemType.Freeze]     = new("✳",  "Крио-импульс", "Заморозить одну цель",    1),
                [LevelItemType.ChainBreak] = new("⌁",  "Разрыв связи", "Выбрать связанную пару",  2),
                [LevelItemType.Undo]       = new("↶",  "Откат",         "Отменить последний ход", 0),
            };
    }
}
