using System;
using UnityEngine;

namespace DigGame
{
    /// <summary>일꾼 생명주기. 구체적인 작업과 화물 처리는 하위 타입이 담당한다.</summary>
    public abstract class Worker : SpriteActor
    {
        public abstract WorkerKind Kind { get; }
        public abstract int Wage { get; }
        public MineSite Mine { get; private set; }
        public string StatusText { get; protected set; } = "";
        protected GameManager Game { get; private set; }
        protected System.Random RandomSource { get; private set; }

        public void Init(GameManager game, MineSite mine, int seed)
        {
            Game = game != null ? game : throw new ArgumentNullException(nameof(game));
            Mine = mine ?? throw new ArgumentNullException(nameof(mine));
            RandomSource = new System.Random(seed);
            transform.position = WorldGrid.CellToWorld(mine.CenterCell.x, mine.CenterCell.y);
            OnInitialized();
        }

        protected virtual void OnInitialized() { }
        public virtual void DumpCargoToMine() { }
        protected abstract void TickWork(float deltaTime);

        protected virtual void Update()
        {
            if (Game != null && Game.IsRunning) TickWork(Time.deltaTime);
        }
    }
}
