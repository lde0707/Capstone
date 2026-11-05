using System;
using System.Collections.Generic;

namespace DigGame
{
    public enum RecyclingCategory { Specialist = 0, Can = 1, Pet = 2, Paper = 3, Plastic = 4, Aluminum = 5 }
    public enum RecyclingState { Closed, Ready, Transition, Completed }

    public sealed class RecyclingQueue
    {
        public const int MaxCapacity = 30;
        readonly Queue<LootItem> _items = new Queue<LootItem>();
        public int Count => _items.Count;
        public int RemainingCapacity => Math.Max(0, MaxCapacity - Count);
        public bool IsFull => Count >= MaxCapacity;
        public int Enqueue(IEnumerable<LootItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            int accepted = 0;
            foreach (var item in items)
            {
                if (IsFull) break;
                _items.Enqueue(item);
                accepted++;
            }
            return accepted;
        }
        public bool TryPeek(int offset, out LootItem item)
        {
            item = default;
            if (offset < 0 || offset >= Count) return false;
            int index = 0;
            foreach (var candidate in _items)
                if (index++ == offset) { item = candidate; return true; }
            return false;
        }
        internal LootItem RemoveFirst() => _items.Dequeue();
    }

    public sealed class PollutionMeter
    {
        public const int MaxValue = 10;
        public int Value { get; private set; }
        public event Action<int> Changed;
        public void Add(int amount)
        {
            int next = Math.Max(0, Math.Min(MaxValue, Value + amount));
            if (next == Value) return;
            Value = next;
            Changed?.Invoke(Value);
        }
    }

    public interface IRecyclingRewardSink { int Pay(LootItem item); }

    public readonly struct RecyclingRoundResult
    {
        public readonly LootItem Item;
        public readonly bool Correct;
        public readonly int Reward;
        public readonly int PollutionDelta;
        public readonly bool TimedOut;
        public bool Specialist => Item.Category == RecyclingCategory.Specialist;
        public RecyclingRoundResult(LootItem item, bool correct, int reward, int pollutionDelta, bool timedOut)
        { Item = item; Correct = correct; Reward = reward; PollutionDelta = pollutionDelta; TimedOut = timedOut; }
    }

    /// <summary>Unity 입력/화면과 독립된 FIFO 처리 상태 기계. 판정 즉시 제거하여 재입장 시 중복 지급을 막는다.</summary>
    public sealed class RecyclingSession
    {
        readonly IRecyclingRewardSink _rewards;
        readonly float _duration;
        readonly float _answerDuration;
        readonly int _penalty;
        readonly int _recovery;
        float _elapsed;
        float _answerElapsed;
        public RecyclingQueue Queue { get; }
        public PollutionMeter Pollution { get; }
        public RecyclingState State { get; private set; }
        public RecyclingRoundResult LastResult { get; private set; }
        public float Progress => Math.Min(1f, _elapsed / _duration);
        public float AnswerProgress => Math.Min(1f, _answerElapsed / _answerDuration);
        public float AnswerTimeRemaining => Math.Max(0f, _answerDuration - _answerElapsed);
        public bool IsOpen => State != RecyclingState.Closed;
        public event Action<RecyclingRoundResult> Resolved;

        public RecyclingSession(RecyclingQueue queue, PollutionMeter pollution, IRecyclingRewardSink rewards,
            float duration = .45f, int penalty = 2, float answerDuration = 2f, int recovery = 1)
        {
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            Pollution = pollution ?? throw new ArgumentNullException(nameof(pollution));
            _rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
            _duration = Math.Max(.01f, duration);
            _answerDuration = Math.Max(.1f, answerDuration);
            _penalty = Math.Max(0, penalty);
            _recovery = Math.Max(0, recovery);
        }
        public bool Open()
        {
            if (IsOpen || Queue.Count == 0) return false;
            Prepare(); return true;
        }
        public void Close() { State = RecyclingState.Closed; _elapsed = 0; _answerElapsed = 0; }
        public bool Submit(RecyclingCategory answer)
        {
            if (State != RecyclingState.Ready || answer < RecyclingCategory.Can || answer > RecyclingCategory.Aluminum)
                return false;
            Queue.TryPeek(0, out var item);
            Resolve(item.Category == answer, false); return true;
        }
        public void Advance(float deltaTime)
        {
            float dt = Math.Max(0, deltaTime);
            if (State == RecyclingState.Ready)
            {
                _answerElapsed += dt;
                if (_answerElapsed >= _answerDuration) Resolve(false, true);
                return;
            }
            if (State == RecyclingState.Transition)
            {
                _elapsed += dt;
                if (_elapsed >= _duration) Prepare();
            }
        }
        public bool Current(out LootItem item)
        {
            if (State == RecyclingState.Transition) { item = LastResult.Item; return true; }
            return Queue.TryPeek(0, out item);
        }
        public bool Next(out LootItem item) => Queue.TryPeek(State == RecyclingState.Transition ? 0 : 1, out item);
        void Prepare()
        {
            _elapsed = 0;
            _answerElapsed = 0;
            State = Queue.Count == 0 ? RecyclingState.Completed : RecyclingState.Ready;
            if (Queue.TryPeek(0, out var item) && item.Category == RecyclingCategory.Specialist) Resolve(true, false);
        }
        void Resolve(bool correct, bool timedOut)
        {
            State = RecyclingState.Transition; _elapsed = 0;
            var item = Queue.RemoveFirst();
            int reward = correct ? _rewards.Pay(item) : 0;
            int before = Pollution.Value;
            Pollution.Add(correct ? -_recovery : _penalty);
            LastResult = new RecyclingRoundResult(item, correct, reward, Pollution.Value - before, timedOut);
            Resolved?.Invoke(LastResult);
        }
    }
}
