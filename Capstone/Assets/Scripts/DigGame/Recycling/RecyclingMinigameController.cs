using UnityEngine;
using UnityEngine.InputSystem;
namespace DigGame
{
    [DefaultExecutionOrder(-200)]
    public sealed class RecyclingMinigameController : MonoBehaviour
    {
        [SerializeField] RecyclingSettings _settings;
        [SerializeField] RecyclingMinigameUI _viewPrefab;
        LazyPrefabInstance<RecyclingMinigameUI> _view;
        public bool IsViewCreated => _view != null && _view.IsCreated;
        static readonly Key[] NumberKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
        static readonly Key[] NumpadKeys = { Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5 };
        [SerializeField] RecyclingFeedback _feedback;
        int _capturedFrame = -1;
        public GameManager Game { get; private set; }
        public RecyclingSession Session { get; private set; }
        public bool IsOpen => Session != null && Session.IsOpen;
        public bool InputCaptured => IsOpen || _capturedFrame == Time.frameCount;
        public void Initialize(GameManager game)
        {
            Game = game;
            Session = new RecyclingSession(new RecyclingQueue(), new PollutionMeter(),
                new GameRecyclingRewardSink(game), _settings.TransitionSeconds,
                _settings.MistakePollution, _settings.AnswerSeconds);
            Session.Resolved += OnResolved;
            Session.Pollution.Changed += OnPollutionChanged;
            _view = new LazyPrefabInstance<RecyclingMinigameUI>(_viewPrefab, transform, view => view.Bind(this));
            OnPollutionChanged(Session.Pollution.Value);
        }
        void OnResolved(RecyclingRoundResult result)
        {
            _feedback.Play(result);
            if (!result.Correct)
                Game.Log((result.TimedOut ? "분류 시간 초과" : "분류 실패") +
                    $" — 오염도 +1칸 (현재 {Session.Pollution.Value}/{PollutionMeter.MaxValue})");
            else if (result.PollutionDelta < 0)
                Game.Log($"정상 분리수거 — 오염도 1칸 회복 (현재 {Session.Pollution.Value}/{PollutionMeter.MaxValue})");
        }
        void OnPollutionChanged(int value) => Game?.View?.SetPollutionLevel(value);
        public bool Deposit()
        {
            if (IsOpen || !Game.IsRunning || Game.ShopOpen || !Game.PlayerNearRecycle()) return false;
            if (Session.Queue.IsFull)
            {
                Game.Log($"분리수거 작업대가 가득 찼습니다 ({Session.Queue.Count}/{RecyclingQueue.MaxCapacity}) — F로 먼저 분류하세요.");
                return false;
            }
            var items = Game.Player.TakeUpTo(Session.Queue.RemainingCapacity);
            int accepted = Session.Queue.Enqueue(items);
            if (accepted == 0)
                Game.Log("가방이 비어 있습니다. F로 보관된 쓰레기를 분류하세요.");
            else if (Game.Player.Inventory.Count > 0)
                Game.Log($"{accepted}개 보관 완료 · 작업대 가득 참 ({Session.Queue.Count}/{RecyclingQueue.MaxCapacity}) — 가방에 {Game.Player.Inventory.Count}개 남음");
            else
                Game.Log($"{accepted}개 보관 완료 — F로 분류 · 대기 {Session.Queue.Count}/{RecyclingQueue.MaxCapacity}개");
            return items.Count > 0;
        }
        public bool Open()
        {
            if (IsOpen || !Game.IsRunning || Game.TimeLeft <= 0 || Game.ShopOpen || !Game.PlayerNearRecycle()) return false;
            _capturedFrame = Time.frameCount;
            if (Session.Open()) { _view.Show(); return true; }
            Game.Log("대기열이 비어 있습니다. E로 쓰레기를 먼저 넣어주세요.");
            return false;
        }
        public void Close()
        {
            if (!IsOpen) return;
            Session.Close(); _view.Hide(); _capturedFrame = Time.frameCount;
        }
        public bool Submit(RecyclingCategory category) => IsOpen && Game.IsRunning && Session.Submit(category);
        /// <summary>미분류 쓰레기를 다음 날까지 방치하면 하루에 한 칸 오염된다.</summary>
        public bool ApplyOvernightPollution()
        {
            if (Session == null || Session.Queue.Count == 0) return false;
            int before = Session.Pollution.Value;
            Session.Pollution.Add(1);
            return Session.Pollution.Value > before;
        }
        void Update()
        {
            if (Session == null) return;
            var kb = Keyboard.current;
            if (!IsOpen)
            {
                if (kb != null && kb.fKey.wasPressedThisFrame) Open();
                return;
            }
            if (!Game.IsRunning || Game.TimeLeft <= 0) { Close(); return; }
            if (kb != null && (kb.escapeKey.wasPressedThisFrame ||
                (Session.State == RecyclingState.Completed && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))))
            { Close(); return; }
            // 준비 상태의 2초 판정 타이머와 카드 전환 시간을 같은 상태 기계에서 갱신한다.
            // 시간 초과 또는 전환 완료 프레임의 입력은 다음 물건에 적용하지 않는다.
            RecyclingState beforeAdvance = Session.State;
            Session.Advance(Time.deltaTime);
            if (beforeAdvance != Session.State || Session.State == RecyclingState.Transition) return;
            if (kb == null) return;
            for (int i = 0; i < NumberKeys.Length; i++)
                if (kb[NumberKeys[i]].wasPressedThisFrame || kb[NumpadKeys[i]].wasPressedThisFrame)
                { Submit((RecyclingCategory)(i + 1)); break; }
        }
    }
}
