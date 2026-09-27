using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DigGame.Editor
{
    /// <summary>배치 모드에서는 -quit 없이 Run을 호출한다. 검증 완료 후 직접 종료한다.</summary>
    [InitializeOnLoad]
    public static class PrefabPlayModeValidation
    {
        const string ActiveKey = "DigGame.PrefabValidation.Active";
        const string ErrorKey = "DigGame.PrefabValidation.Error";
        static double _nextCheck;
        static double _deadline;
        static int _phase;
        static int _previousInstance;
        static RecyclingInputProbe _inputProbe;

        static PrefabPlayModeValidation()
        {
            EditorApplication.playModeStateChanged += StateChanged;
            EditorApplication.update += Tick;
            Application.logMessageReceived += OnLog;
        }

        [MenuItem("DigGame/Prefabs/Validate Play Mode and Restart")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("편집 모드에서 검증을 시작하세요.");
            PrefabValidation.Run();
            RecyclingValidation.Run();
            RuntimeOptimizationValidation.Run();
            DigGameEditPreview.ValidatePreview();
            SessionState.SetBool(ErrorKey, false);
            SessionState.SetBool(ActiveKey, true);
            EditorApplication.EnterPlaymode();
        }

        static void StateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            if (state == PlayModeStateChange.ExitingPlayMode) _inputProbe?.Dispose();
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _phase = 0;
                _inputProbe = null;
                _nextCheck = EditorApplication.timeSinceStartup + 3;
                _deadline = EditorApplication.timeSinceStartup + 90;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                bool failed = SessionState.GetBool(ErrorKey, false);
                SessionState.SetBool(ActiveKey, false);
                if (!failed) Debug.Log("[DigGame] PLAYMODE_VALIDATION_PASSED: bootstrap, frames, day transition, restart, camera cleanup.");
                if (Application.isBatchMode) EditorApplication.Exit(failed ? 1 : 0);
            }
        }

        static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false) || !EditorApplication.isPlaying || _deadline == 0) return;
            try
            {
                Require(!SessionState.GetBool(ErrorKey, false), "Play 모드에서 오류가 기록되었습니다.");
                Require(EditorApplication.timeSinceStartup < _deadline, "Play 모드 검증 시간 초과.");
                if (EditorApplication.timeSinceStartup < _nextCheck) return;
                var game = GameManager.Instance;
                Require(game != null && game.Initialized && game.IsRunning, "실행 중인 게임 초기화 실패.");
                Require(game.Player.GetComponents<SpriteRenderer>().Length == 1, "플레이어 렌더러 중복.");
                Require(UnityEngine.Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length == 1,
                    "GameManager 중복.");
                Require(Camera.main != null && Camera.main.GetComponents<CameraFollow>().Length == 1,
                    "카메라 추적 컴포넌트 중복 또는 누락.");

                if (_phase == 0)
                {
                    Require(game.TimeLeft < GameConfig.DayLengthSeconds, "낮 타이머가 진행되지 않습니다.");
                    if (_inputProbe == null) { _inputProbe = new RecyclingInputProbe(); game.StartCoroutine(_inputProbe.Run()); return; }
                    if (!_inputProbe.Completed) return;
                    Require(_inputProbe.Failure == null, _inputProbe.Failure);
                    var grid = game.Grid;
                    game.AddRelicIncome(game.BaseQuota, "Play 검증");
                    typeof(GameManager).GetMethod("DoEndDay", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
                    Require(game.State == GameState.Night, "밤 정산 전환 실패.");
                    game.StartNextDay();
                    Require(game.Day == 2 && game.IsRunning && ReferenceEquals(grid, game.Grid), "다음 날 월드 유지 실패.");
                    _previousInstance = game.GetInstanceID();
                    Bootstrap.Restart();
                    _phase = 1;
                    _nextCheck = EditorApplication.timeSinceStartup + 3;
                }
                else if (_phase == 1)
                {
                    Require(game.GetComponentsInChildren<RecyclingMinigameUI>(true).Length == 0, "재시작 후 UI가 미리 생성되었습니다.");
                    Require(game.transform.Find("RuntimeEffectPool") != null, "새 게임 효과 풀 누락.");
                    Require(game.Recycling.Session.Queue.Count == 0 && game.Recycling.Session.Pollution.Value == 0, "재시작 시 분리수거 상태 초기화 실패.");
                    Require(game.GetInstanceID() != _previousInstance && game.Day == 1, "새 게임 재시작 실패.");
                    Require(UnityEngine.Object.FindObjectsByType<ParallaxBackdrop>(FindObjectsSortMode.None).Length == 1,
                        "재시작 후 배경 컴포넌트 중복.");
                    ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetTempPath(), "DigGame-prefab-smoke.png"));
                    _phase = 2;
                    _nextCheck = EditorApplication.timeSinceStartup + 1;
                }
                else
                {
                    _deadline = 0;
                    EditorApplication.ExitPlaymode();
                }
            }
            catch (Exception e)
            {
                SessionState.SetBool(ErrorKey, true);
                _deadline = 0;
                Debug.LogException(e);
                EditorApplication.ExitPlaymode();
            }
        }

        static void OnLog(string message, string stack, LogType type)
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            // Unity 6000.3의 에디터 검색 인덱스 초기화 예외는 게임 실행 검증과 분리한다.
            // 원래 오류 로그는 그대로 남기며, 게임/렌더링/프리팹의 다른 오류는 실패 처리한다.
            if (type == LogType.Exception && stack.Contains("UnityEditor.Search.SearchDatabase") &&
                !stack.Contains("DigGame."))
            {
                Debug.LogWarning("[DigGame] EDITOR_SEARCH_ISSUE: Unity 검색 인덱스 예외를 별도 기록했습니다. 게임 검증을 계속합니다.");
                return;
            }
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                SessionState.SetBool(ErrorKey, true);
        }

        static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }
    }
}
