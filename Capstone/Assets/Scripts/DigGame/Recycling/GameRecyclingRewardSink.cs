using System.Collections.Generic;
namespace DigGame
{
    public sealed class GameRecyclingRewardSink : IRecyclingRewardSink
    {
        readonly GameManager _game;
        public GameRecyclingRewardSink(GameManager game) => _game = game;
        public int Pay(LootItem item)
        {
            int before = _game.Money;
            _game.RecycleAtFactory(new List<LootItem> { item },
                item.Category == RecyclingCategory.Specialist ? "전문 처리" : "직접 분리수거");
            return _game.Money - before;
        }
    }
}
