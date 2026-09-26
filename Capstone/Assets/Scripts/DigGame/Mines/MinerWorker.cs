using UnityEngine;

namespace DigGame
{
    public sealed class MinerWorker : Worker
    {
        public override WorkerKind Kind => WorkerKind.Miner;
        public override int Wage => GameConfig.MinerWage;
        float _extractTimer;

        protected override void OnInitialized()
        {
            if (ActorRenderer.sprite == null)
                ActorRenderer.sprite = SpriteFactory.Framed(new Color32(255, 190, 70, 255), new Color32(90, 55, 10, 255));
            Vector2 center = Mine.CenterF;
            float x = center.x + ((float)RandomSource.NextDouble() - 0.5f) * (Mine.Rect.width - 0.5f);
            float y = center.y + ((float)RandomSource.NextDouble() - 0.5f) * (Mine.Rect.height - 0.5f);
            transform.position = WorldGrid.GridToWorld(x, y);
        }

        protected override void TickWork(float deltaTime)
        {
            if (Mine.Reserve <= 0 && Mine.Stockpile.Count == 0)
            {
                StatusText = "고갈";
                return;
            }

            StatusText = Mine.Stockpile.Count > 0 ? "보관품 자동 재활용" : "폐기물 자동 회수";
            _extractTimer += deltaTime;
            if (_extractTimer >= GameConfig.MinerExtractInterval)
            {
                _extractTimer -= GameConfig.MinerExtractInterval;

                // 보관품이 없으면 광산 매장량에서 새 고철을 하나 회수한다.
                if (Mine.Stockpile.Count == 0 && !Mine.ExtractOne(RandomSource)) return;

                // 수거 일꾼 단독으로도 지상 반입과 재활용까지 끝내 즉시 수익을 낸다.
                var batch = Mine.TakeFromStockpile(1);
                int paid = Game.RecycleAtFactory(batch, $"자동 수거 일꾼 · 도시광산 #{Mine.Id}");
                Mine.RecordAutomaticSale(batch.Count, paid);
            }
        }

    }
}
