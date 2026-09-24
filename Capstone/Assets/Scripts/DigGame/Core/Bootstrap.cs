using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 씬 세팅 없이 Play 만 누르면 게임이 구성되도록 하는 진입점.
    /// 씬에 아무것도 배치하지 않아도 동작한다.
    /// </summary>
    public static class Bootstrap
    {
        public static bool AutoStart = true;   // 자동 실행을 끄고 싶으면 false

        static GameObject _root;
        static int _seed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (!AutoStart) return;

            _seed = Random.Range(1, int.MaxValue);
            Launch(_seed, GameObject.Find("DigGame"));
        }

        public static void Launch(int seed)
        {
            Launch(seed, null);
        }

        static void Launch(int seed, GameObject sceneRoot)
        {
            Teardown();

            _seed = seed;
            _root = sceneRoot != null ? sceneRoot : new GameObject("DigGame");

            // 편집 모드 미리보기가 Play 진입 직전까지 남아 있더라도
            // 런타임 계층과 겹치지 않도록 먼저 비활성화해 정리한다.
            for (int i = _root.transform.childCount - 1; i >= 0; i--)
            {
                var child = _root.transform.GetChild(i).gameObject;
                child.SetActive(false);
                Object.Destroy(child);
            }

            var game = _root.AddComponent<GameManager>();

            try
            {
                game.StartGame(seed);
            }
            catch (System.Exception e)
            {
                // 초기화가 실패하면 반쯤 만들어진 게임을 남기지 않는다.
                Debug.LogError("[DigGame] 게임 초기화 실패 — 아래 예외를 확인하세요.");
                Debug.LogException(e);
                _root.SetActive(false);
                Object.Destroy(_root);
                _root = null;
                return;
            }

            var hud = _root.AddComponent<GameHUD>();
            hud.Init(game);
        }

        public static void Restart()
        {
            Launch(Random.Range(1, int.MaxValue));
        }

        static void Teardown()
        {
            if (_root != null)
            {
                // Restart 는 구 GameManager 의 Update 안에서 불릴 수 있으므로
                // DestroyImmediate 로 자기 자신을 지우면 안 된다.
                // 대신 즉시 비활성화해서 Update/렌더링을 멈추고, 파괴는 프레임 끝에 맡긴다.
                _root.SetActive(false);
                Object.Destroy(_root);
                _root = null;
            }

            // 카메라의 CameraFollow 는 GameManager.SetupCamera 가 새로 붙이면서 교체한다.
            SpriteFactory.ClearCache();
        }
    }
}
