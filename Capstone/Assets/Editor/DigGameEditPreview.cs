using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigGame.Editor
{
    /// <summary>
    /// 코드로 생성되는 게임 월드를 편집 모드에서도 볼 수 있게 한다.
    /// 미리보기 오브젝트는 저장되지 않으며 Play 직전에 제거된다.
    /// </summary>
    [InitializeOnLoad]
    static class DigGameEditPreview
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        const string SceneRootName = "DigGame";
        const string PreviewName = "[Edit Preview]";
        const int PreviewSeed = 20260830;

        static bool _building;
        static bool _scheduled;

        static DigGameEditPreview()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.hierarchyChanged += SchedulePreview;
            AssemblyReloadEvents.beforeAssemblyReload += DestroyPreview;
            SchedulePreview();
        }

        [MenuItem("DigGame/Rebuild Edit Preview")]
        static void RebuildPreview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            DestroyPreview();
            BuildPreview();
            SceneView.RepaintAll();
        }

        /// <summary>배치 모드와 메뉴에서 미리보기 구성을 검증할 때 사용한다.</summary>
        [MenuItem("DigGame/Validate Edit Preview")]
        public static void ValidatePreview()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            BuildPreview();

            var sceneRoot = FindRoot(scene, SceneRootName);
            var preview = sceneRoot != null ? FindChild(sceneRoot.transform, PreviewName) : null;
            var game = preview != null ? preview.GetComponent<GameManager>() : null;

            if (game == null || !game.Initialized || game.View == null || game.Player == null)
                throw new System.InvalidOperationException("DigGame 편집 미리보기가 완전하게 생성되지 않았습니다.");

            var fog = preview.Find("WorldView/Smooth Fog");
            var fogRenderer = fog != null ? fog.GetComponent<SpriteRenderer>() : null;
            if (fogRenderer == null || fogRenderer.sharedMaterial == null ||
                fogRenderer.sharedMaterial.shader == null || !fogRenderer.sharedMaterial.shader.isSupported)
                throw new System.InvalidOperationException("원형 시야 셰이더가 정상적으로 생성되지 않았습니다.");

            var skyShader = Resources.Load<Shader>("SkyCycle");
            if (skyShader == null || !skyShader.isSupported)
                throw new System.InvalidOperationException("전체 화면 낮/밤 하늘 셰이더가 정상적으로 임포트되지 않았습니다.");

            ValidateTrashSprites();
            ValidateEnvironmentAssets(preview);

            Debug.Log($"[DigGame] 편집 미리보기 검증 완료 — 생성된 오브젝트 {CountHierarchy(preview)}개");
            Debug.Log($"[DigGame] 1칸 쓰레기 비율 — " +
                $"1층 {SingleSlotRate(Zone.Surface):P0}, " +
                $"2층 {SingleSlotRate(Zone.Shallow):P0}, " +
                $"3층 {SingleSlotRate(Zone.Deep):P0}, " +
                $"4층 {SingleSlotRate(Zone.Abyss):P0}");
            ValidateLootDistribution();
            ValidateBalance();
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            SchedulePreview();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                DestroyPreview();
            else if (state == PlayModeStateChange.EnteredEditMode)
                SchedulePreview();
        }

        static void SchedulePreview()
        {
            if (_building || _scheduled || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            _scheduled = true;
            EditorApplication.delayCall += () =>
            {
                _scheduled = false;
                BuildPreview();
            };
        }

        static void BuildPreview()
        {
            if (_building || EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
                return;

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || scene.path != ScenePath)
                return;

            var sceneRoot = FindRoot(scene, SceneRootName);
            if (sceneRoot == null || FindChild(sceneRoot.transform, PreviewName) != null)
                return;

            GameObject preview = null;
            _building = true;
            try
            {
                preview = new GameObject(PreviewName)
                {
                    hideFlags = HideFlags.DontSaveInEditor
                };
                SceneManager.MoveGameObjectToScene(preview, scene);
                preview.transform.SetParent(sceneRoot.transform, false);

                var game = preview.AddComponent<GameManager>();
                game.StartGame(PreviewSeed, configureCamera: false);

                var hud = preview.AddComponent<GameHUD>();
                hud.Init(game);
            }
            catch (System.Exception exception)
            {
                Debug.LogError("[DigGame] 편집 모드 미리보기를 만들지 못했습니다.");
                Debug.LogException(exception);
                if (preview != null)
                    Object.DestroyImmediate(preview);
                SpriteFactory.ClearCache();
            }
            finally
            {
                _building = false;
            }
        }

        static void DestroyPreview()
        {
            if (_building)
                return;

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            var sceneRoot = FindRoot(scene, SceneRootName);
            if (sceneRoot == null)
                return;

            var preview = FindChild(sceneRoot.transform, PreviewName);
            if (preview == null)
                return;

            _building = true;
            try
            {
                Object.DestroyImmediate(preview.gameObject);
                SpriteFactory.ClearCache();
            }
            finally
            {
                _building = false;
            }
        }

        static GameObject FindRoot(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name)
                    return root;

            return null;
        }

        static Transform FindChild(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                    return child;
            }

            return null;
        }

        static int CountHierarchy(Transform root)
        {
            int count = 1;
            for (int i = 0; i < root.childCount; i++)
                count += CountHierarchy(root.GetChild(i));
            return count;
        }

        static float SingleSlotRate(Zone zone)
        {
            const int rolls = 10000;
            var random = new System.Random(20260830 + (int)zone);
            int singleSlot = 0;

            for (int i = 0; i < rolls; i++)
                if (LootTable.RollTreasure(zone, random).Volume == 1)
                    singleSlot++;

            return singleSlot / (float)rolls;
        }

        static void ValidateTrashSprites()
        {
            var textures = Resources.LoadAll<Texture2D>("TrashSprites/Diverse");
            if (textures == null || textures.Length != 29)
                throw new System.InvalidOperationException($"CC0 픽셀 쓰레기 이미지가 {textures?.Length ?? 0}개만 임포트되었습니다.");

            for (int i = 0; i < textures.Length; i++)
                if (textures[i] == null || textures[i].filterMode != FilterMode.Point)
                    throw new System.InvalidOperationException("픽셀 쓰레기 이미지의 Point 필터 설정이 적용되지 않았습니다.");

            var samples = new[]
            {
                new LootItem("소주병", 1, Color.white, 1, MaterialKind.Glass),
                new LootItem("찌그러진 캔", 1, Color.white, 1, MaterialKind.Metal),
                new LootItem("녹슨 자전거", 1, Color.white, 3, MaterialKind.Metal),
                new LootItem("의료 폐기물", 1, Color.white, 2, MaterialKind.Hazard),
                new LootItem("장난감 상자", 1, Color.white, 2, MaterialKind.Plastic),
                new LootItem("고장난 라디오", 1, Color.white, 2, MaterialKind.Electronics),
                new LootItem("브라운관 TV", 1, Color.white, 4, MaterialKind.Electronics),
                new LootItem("카세트테이프", 1, Color.white, 1, MaterialKind.Plastic),
                new LootItem("휴대용 게임기", 1, Color.white, 1, MaterialKind.Electronics),
                new LootItem("중고 스마트폰", 1, Color.white, 1, MaterialKind.Electronics),
                new LootItem("폐배터리 액", 1, Color.white, 2, MaterialKind.Hazard),
                new LootItem("금도금 시계", 1, Color.white, 1, MaterialKind.Relic)
            };
            var distinct = new HashSet<Sprite>();
            for (int i = 0; i < samples.Length; i++)
            {
                Sprite sprite = TrashSpriteCatalog.For(samples[i]);
                if (sprite == null)
                    throw new System.InvalidOperationException($"부피 {samples[i].Volume} 쓰레기 스프라이트를 만들지 못했습니다.");
                distinct.Add(sprite);
            }
            if (distinct.Count < 10)
                throw new System.InvalidOperationException($"대표 쓰레기 12종이 {distinct.Count}개의 실루엣만 사용합니다.");

            Debug.Log($"[DigGame] 다양한 CC0 픽셀 쓰레기 이미지 29개 · 대표 12종 중 서로 다른 실루엣 {distinct.Count}개 검증 완료");
        }

        static void ValidateEnvironmentAssets(Transform preview)
        {
            string[] names =
            {
                "natural_cave_autotile_32", "db32_cave_tileset", "trash_can", "recycle_items",
                "opp_cave_background", "opp_cave_rock_tiles", "opp_cave_passages_32"
            };
            for (int i = 0; i < names.Length; i++)
            {
                var texture = Resources.Load<Texture2D>("EnvironmentPixel/" + names[i]);
                if (texture == null)
                    throw new System.InvalidOperationException($"환경 픽셀 에셋을 임포트하지 못했습니다: {names[i]}");
                if (texture.filterMode != FilterMode.Point)
                    throw new System.InvalidOperationException($"환경 픽셀 에셋 Point 필터가 빠졌습니다: {names[i]}");
            }

            var groundSprites = new HashSet<Sprite>();
            var passageSprites = new HashSet<Sprite>();
            for (int i = 0; i < 16; i++)
            {
                Sprite ground = EnvironmentSpriteCatalog.GroundFill(i);
                Sprite passage = EnvironmentSpriteCatalog.PassageForMask(i, 5);
                if (ground == null || ground.rect.width != 32f || ground.rect.height != 32f)
                    throw new System.InvalidOperationException($"OPP 연속 암반 {i}번 크롭이 올바르지 않습니다.");
                if (passage == null || passage.rect.width != 32f || passage.rect.height != 32f)
                    throw new System.InvalidOperationException($"OPP 통로 마스크 {i}번 크롭이 올바르지 않습니다.");
                groundSprites.Add(ground);
                passageSprites.Add(passage);
            }
            if (groundSprites.Count != 16 || passageSprites.Count != 16)
                throw new System.InvalidOperationException("연속 암반 또는 16방향 통로 타일이 모두 생성되지 않았습니다.");

            if (EnvironmentSpriteCatalog.PassageForMask(15, 0) == null ||
                EnvironmentSpriteCatalog.PassageForMask(5, 0) == null ||
                EnvironmentSpriteCatalog.PassageForMask(1, 0) == null)
                throw new System.InvalidOperationException("자연 연결·직선 통로·막힌 끝 대표 타일을 만들지 못했습니다.");

            if (EnvironmentSpriteCatalog.SurfaceBin() == null ||
                EnvironmentSpriteCatalog.RecycleItem(3) == null)
                throw new System.InvalidOperationException("분리수거장 픽셀 이미지를 만들지 못했습니다.");

            Vector2Int sample = new Vector2Int(7, 9);
            Vector3 world = WorldGrid.CellToWorld(sample.x, sample.y);
            if (WorldGrid.WorldToCell(world) != sample ||
                !Mathf.Approximately(
                    WorldGrid.CellToWorld(sample.x + 1, sample.y).x - world.x,
                    GameConfig.CellSize))
                throw new System.InvalidOperationException("1.5배 셀 좌표 변환이 일치하지 않습니다.");

            var station = preview.Find("WorldView/Markers/SurfaceTrashDropOff/SurfaceBin");
            var highlight = preview.Find("WorldView/DigHighlight");
            if (station == null || station.GetComponent<SpriteRenderer>()?.sprite == null)
                throw new System.InvalidOperationException("지상 분리수거장 에셋이 미리보기에 생성되지 않았습니다.");
            if (highlight == null || !Mathf.Approximately(highlight.localScale.x, GameConfig.CellSize))
                throw new System.InvalidOperationException("굴착 조준 표시가 확대된 셀 크기와 맞지 않습니다.");
            var game = preview.GetComponent<GameManager>();
            if (game == null || game.Player == null || game.Player.IsFalling)
                throw new System.InvalidOperationException("플레이어 무중력 이동 상태가 적용되지 않았습니다.");

            Debug.Log($"[DigGame] OPP 연속 암반 16종 · 통로 마스크 16종 · 환경 에셋 {names.Length}개 · 무중력 검증 완료");
        }

        static void ValidateBalance()
        {
            const int seedCount = 16;
            int minStarterValue = int.MaxValue;
            int maxStarterValue = 0;
            int starterValueSum = 0;

            for (int seed = 1; seed <= seedCount; seed++)
            {
                var grid = WorldGenerator.Generate(seed * 7919);
                int center = grid.Width / 2;
                int startX = center - GameConfig.StarterTrashCount / 2;
                int starterCount = 0;
                int starterValue = 0;

                for (int i = 0; i < GameConfig.StarterTrashCount; i++)
                {
                    int x = Mathf.Clamp(startX + i, 1, grid.Width - 2);
                    if (!grid.TryPeekTreasure(x, GameConfig.SurfaceRow, out var item)) continue;
                    starterCount++;
                    starterValue += item.Value;
                }

                if (starterCount != GameConfig.StarterTrashCount)
                    throw new System.InvalidOperationException($"시작 쓰레기가 {starterCount}개만 배치되었습니다.");
                if (starterValue < GameConfig.QuotaForDay(1))
                    throw new System.InvalidOperationException($"시작 쓰레기 가치 {starterValue}원이 첫날 할당량보다 낮습니다.");

                minStarterValue = Mathf.Min(minStarterValue, starterValue);
                maxStarterValue = Mathf.Max(maxStarterValue, starterValue);
                starterValueSum += starterValue;
            }

            Debug.Log($"[DigGame] 일차별 할당량 — " +
                $"{GameConfig.QuotaForDay(1):N0} → {GameConfig.QuotaForDay(2):N0} → " +
                $"{GameConfig.QuotaForDay(3):N0} → {GameConfig.QuotaForDay(4):N0} → " +
                $"{GameConfig.QuotaForDay(5):N0} → {GameConfig.QuotaForDay(6):N0} → " +
                $"{GameConfig.QuotaForDay(7):N0}원");
            Debug.Log($"[DigGame] 시작 쓰레기 {GameConfig.StarterTrashCount}개 가치 — " +
                $"최소 {minStarterValue:N0} / 평균 {starterValueSum / seedCount:N0} / 최대 {maxStarterValue:N0}원");
        }

        static void ValidateLootDistribution()
        {
            var grid = WorldGenerator.Generate(20260831);
            var names = new HashSet<string>();
            var count = new int[4];
            var singleSlot = new int[4];
            var valueSum = new long[4];

            foreach (var (cell, item) in grid.AllTreasures)
            {
                TileKind kind = grid.Get(cell.x, cell.y);
                if (!TileInfo.IsDiggable(kind))
                    throw new System.InvalidOperationException($"쓰레기가 팔 수 없는 칸 ({cell.x}, {cell.y})에 놓였습니다.");

                int zone = (int)TileInfo.ZoneOf(kind);
                names.Add(item.Name);
                count[zone]++;
                valueSum[zone] += item.Value;
                if (item.Volume == 1) singleSlot[zone]++;
            }

            if (grid.Width != GameConfig.WorldWidth || grid.Width < 183)
                throw new System.InvalidOperationException($"월드 폭이 충분히 확장되지 않았습니다: {grid.Width}칸");
            if (names.Count < 10)
                throw new System.InvalidOperationException($"쓰레기 종류가 {names.Count}개뿐입니다.");

            double previousAverage = 0d;
            for (int zone = 0; zone < 4; zone++)
            {
                if (singleSlot[zone] < GameConfig.GuaranteedSingleSlotTrashPerLayer)
                    throw new System.InvalidOperationException($"{zone + 1}층 1칸 쓰레기가 {singleSlot[zone]}개뿐입니다.");

                double average = count[zone] > 0 ? valueSum[zone] / (double)count[zone] : 0d;
                if (zone > 0 && average <= previousAverage)
                    throw new System.InvalidOperationException($"{zone + 1}층 평균 쓰레기 가치가 이전 층보다 높지 않습니다.");
                previousAverage = average;
            }

            Debug.Log($"[DigGame] 월드 폭 {grid.Width}칸 · 쓰레기 {names.Count}종 · " +
                $"층별 1칸 쓰레기 {singleSlot[0]} / {singleSlot[1]} / {singleSlot[2]} / {singleSlot[3]}개 · " +
                $"평균 가치 {valueSum[0] / count[0]:N0} → {valueSum[1] / count[1]:N0} → " +
                $"{valueSum[2] / count[2]:N0} → {valueSum[3] / count[3]:N0}원");
        }
    }
}
