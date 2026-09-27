using UnityEngine;
using UnityEngine.InputSystem;
namespace DigGame
{
    [DefaultExecutionOrder(-200)]
    public sealed class RecyclingMinigameController : MonoBehaviour
    {
        [SerializeField] RecyclingSettings _settings;
        [SerializeField] RecyclingMinigameUI _view;
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
                new GameRecyclingRewardSink(game), _settings.TransitionSeconds, _settings.MistakePollution);
            Session.Resolved += OnResolved;
            _view.Bind(this);
        }
        void OnResolved(RecyclingRoundResult result)
        {
            _feedback.Play(result);
            if (!result.Correct) Game.Log($"분류 실패 — 오염도 +{_settings.MistakePollution} (현재 {Session.Pollution.Value})");
        }
        public bool Deposit()
        {
            if (IsOpen || !Game.IsRunning || Game.ShopOpen || !Game.PlayerNearRecycle()) return false;
            var items = Game.Player.TakeAll();
            Session.Queue.Enqueue(items);
            Game.Log(items.Count == 0 ? "가방이 비어 있습니다. F로 보관된 쓰레기를 분류하세요."
                : $"{items.Count}개 보관 완료 — F로 분류 · 대기 {Session.Queue.Count}개");
            return items.Count > 0;
        }
        public bool Open()
        {
            if (IsOpen || !Game.IsRunning || Game.TimeLeft <= 0 || Game.ShopOpen || !Game.PlayerNearRecycle()) return false;
            _capturedFrame = Time.frameCount;
            if (Session.Open()) return true;
            Game.Log("대기열이 비어 있습니다. E로 쓰레기를 먼저 넣어주세요.");
            return false;
        }
        public void Close()
        {
            if (!IsOpen) return;
            Session.Close(); _capturedFrame = Time.frameCount;
        }
        public bool Submit(RecyclingCategory category) => IsOpen && Game.IsRunning && Session.Submit(category);
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
            // 전환 완료 프레임에 눌린 키도 무시하여 이전 물건을 향한 입력이 다음 물건에 적용되지 않는다.
            if (Session.State == RecyclingState.Transition) { Session.Advance(Time.deltaTime); return; }
            if (kb == null) return;
            var keys = new[] { kb.digit1Key, kb.digit2Key, kb.digit3Key, kb.digit4Key, kb.digit5Key };
            var pads = new[] { kb.numpad1Key, kb.numpad2Key, kb.numpad3Key, kb.numpad4Key, kb.numpad5Key };
            for (int i = 0; i < keys.Length; i++)
                if (keys[i].wasPressedThisFrame || pads[i].wasPressedThisFrame)
                { Submit((RecyclingCategory)(i + 1)); break; }
        }
    }
}
