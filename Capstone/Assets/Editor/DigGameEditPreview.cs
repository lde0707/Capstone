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
            if (game.Hud == null || !game.Hud.TitleScreenOpen || !game.InputBlocked)
                throw new System.InvalidOperationException("타이틀 화면이 초기 진입 화면이 아니거나 게임 입력을 차단하지 못했습니다.");

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
            ValidatePowerDigScaling();
            ValidateMineUtility();
            ValidateSurfaceEntrance();
            ValidateDayTiming();
            ValidateWorkerEconomy();
            ValidatePixelUi();
            Debug.Log("[DigGame] 메뉴 UI — 타이틀 초기 진입 · 일시정지 중 게임 입력 차단 검증 완료");
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
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null ||
                (Resources.Load<GamePrefabCatalog>(GamePrefabCatalog.ResourcePath) == null ||
                 !Resources.Load<GamePrefabCatalog>(GamePrefabCatalog.ResourcePath).Contains(PrefabId.RecyclingStation))) return;
            var station = Resources.Load<GamePrefabCatalog>(GamePrefabCatalog.ResourcePath).Get(PrefabId.RecyclingStation);
            if (new SerializedObject(station.GetComponent<RecyclingMinigameController>()).FindProperty("_viewPrefab").objectReferenceValue == null) return;
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
                game.StartGame(PreviewSeed, configureCamera: false,
                    factory: new EditorPrefabFactory(Resources.Load<GamePrefabCatalog>(GamePrefabCatalog.ResourcePath)));

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

            var recycle = Resources.LoadAll<Texture2D>("TrashSprites/Recycle");
            if (recycle == null || recycle.Length != 15)
                throw new System.InvalidOperationException($"분리수거 그림이 {recycle?.Length ?? 0}장만 임포트되었습니다 (5칸 x 3종).");

            for (int i = 0; i < textures.Length; i++)
                if (textures[i] == null || textures[i].filterMode != FilterMode.Point)
                    throw new System.InvalidOperationException("픽셀 쓰레기 이미지의 Point 필터 설정이 적용되지 않았습니다.");
            for (int i = 0; i < recycle.Length; i++)
                if (recycle[i] == null || recycle[i].filterMode != FilterMode.Point)
                    throw new System.InvalidOperationException("분리수거 그림의 Point 필터 설정이 적용되지 않았습니다.");

            ValidateRecyclingArtMatchesBin();
        }

        /// <summary>
        /// 땅에서 나오는 모든 쓰레기가 '자기 칸의 그림'을 받는지 확인한다.
        /// 플레이어는 카드 그림만 보고 2초 안에 칸을 골라야 하므로,
        /// 그림과 칸이 한 건이라도 어긋나면 분류가 불공정해진다.
        /// </summary>
        static void ValidateRecyclingArtMatchesBin()
        {
            // 그림 파일 이름 앞머리가 그 칸의 표식이다 (can_ / pet_ / paper_ / plastic_ / foil_).
            string[] prefixes = { null, "can_", "pet_", "paper_", "plastic_", "foil_" };

            var seen = new HashSet<string>();
            int checkedCount = 0;

            foreach (var item in LootTable.AllEntries())
            {
                int category = (int)item.Category;
                if (category <= 0 || category >= prefixes.Length)
                    throw new System.InvalidOperationException(
                        $"'{item.Name}' 에 분리수거 칸이 없습니다. 자동으로 넘어가는 품목은 둘 수 없습니다.");

                Sprite sprite = TrashSpriteCatalog.For(item);
                if (sprite == null)
                    throw new System.InvalidOperationException($"'{item.Name}' 의 그림을 만들지 못했습니다.");

                if (!sprite.name.StartsWith(prefixes[category], System.StringComparison.Ordinal))
                    throw new System.InvalidOperationException(
                        $"'{item.Name}' 은(는) {(RecyclingCategory)category} 칸인데 그림이 '{sprite.name}' 입니다.");

                seen.Add(sprite.name);
                checkedCount++;
            }

            if (seen.Count != 15)
                throw new System.InvalidOperationException($"분리수거 그림 15장 중 {seen.Count}장만 쓰이고 있습니다.");

            Debug.Log($"[DigGame] 쓰레기 {checkedCount}종이 모두 자기 분리수거 칸의 그림을 사용합니다 (그림 {seen.Count}장 전부 사용).");
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

            string[] pixelRefreshTextures =
            {
                "Ground/tileset1", "Ground/tileset2", "Ground/tileset4",
                "Ground/tileset5", "Ground/tileset6", "Era/era_dropoffs", "Era/era_shops",
                "Era/era_houses", "Era/era_restorers",
                "Products/recycled_products", "Player/hero_down", "Player/hero_up",
                "Player/hero_side", "Player/hero_dig_down", "Player/hero_dig_up",
                "Player/hero_dig_side", "Player/tongs"
            };
            for (int i = 0; i < pixelRefreshTextures.Length; i++)
            {
                var texture = Resources.Load<Texture2D>("PixelRefresh/" + pixelRefreshTextures[i]);
                if (texture == null)
                    throw new System.InvalidOperationException($"새 CC0 픽셀 에셋을 임포트하지 못했습니다: {pixelRefreshTextures[i]}");
                if (texture.filterMode != FilterMode.Point)
                    throw new System.InvalidOperationException($"새 CC0 픽셀 에셋 Point 필터가 빠졌습니다: {pixelRefreshTextures[i]}");
            }

            var groundSprites = new HashSet<Sprite>();
            var passageSprites = new HashSet<Sprite>();
            for (int i = 0; i < 16; i++)
            {
                Sprite ground = EnvironmentSpriteCatalog.GroundFill(TileKind.SoftDirt, i);
                Sprite passage = EnvironmentSpriteCatalog.PassageForMask(TileKind.SoftDirt, i, 5);
                if (ground == null || ground.rect.width != 16f || ground.rect.height != 16f)
                    throw new System.InvalidOperationException($"CC0 연속 흙 {i}번 크롭이 올바르지 않습니다.");
                if (passage == null || passage.rect.width != 16f || passage.rect.height != 16f)
                    throw new System.InvalidOperationException($"CC0 통로 마스크 {i}번 크롭이 올바르지 않습니다.");
                groundSprites.Add(ground);
                passageSprites.Add(passage);
            }
            if (groundSprites.Count != 16 || passageSprites.Count != 16)
                throw new System.InvalidOperationException("연속 암반 또는 16방향 통로 타일이 모두 생성되지 않았습니다.");

            if (EnvironmentSpriteCatalog.PassageForMask(255, 0) == null ||
                EnvironmentSpriteCatalog.PassageForMask(5, 0) == null ||
                EnvironmentSpriteCatalog.PassageForMask(1, 0) == null)
                throw new System.InvalidOperationException("자연 연결·직선 통로·막힌 끝 대표 타일을 만들지 못했습니다.");

            // 사용자 스케치 기준: 첫 굴착은 위만 열리고 좌우·바닥에 같은 흙이 남는다.
            // 한 칸 더 아래를 파면 기존 칸과 새 칸 사이의 바닥이 사라져 세로 통로가 된다.
            Sprite firstDig = EnvironmentSpriteCatalog.PassageForMask(1, 0);
            Sprite verticalPassage = EnvironmentSpriteCatalog.PassageForMask(5, 0);
            Sprite fullyOpen = EnvironmentSpriteCatalog.PassageForMask(255, 0);
            if (SpritePixel(firstDig, 8, 15).a != 0 ||
                SpritePixel(firstDig, 8, 0).a == 0 ||
                SpritePixel(firstDig, 0, 8).a == 0 ||
                SpritePixel(firstDig, 15, 8).a == 0)
                throw new System.InvalidOperationException("첫 굴착 타일이 위가 열린 U자형 흙 가장자리가 아닙니다.");
            if (SpritePixel(verticalPassage, 8, 15).a != 0 ||
                SpritePixel(verticalPassage, 8, 0).a != 0)
                throw new System.InvalidOperationException("두 번째 굴착 뒤 세로 통로의 중간 바닥이 제거되지 않았습니다.");
            if (SpritePixel(fullyOpen, 0, 0).a != 0 || SpritePixel(fullyOpen, 15, 15).a != 0)
                throw new System.InvalidOperationException("사방이 열린 통로에 잘린 흙 조각이 남아 있습니다.");

            Color32 originalGroundPixel = SpritePixel(EnvironmentSpriteCatalog.GroundFill(0), 0, 0);
            Color32 passageGroundPixel = SpritePixel(firstDig, 0, 0);
            if (originalGroundPixel.r != passageGroundPixel.r ||
                originalGroundPixel.g != passageGroundPixel.g ||
                originalGroundPixel.b != passageGroundPixel.b ||
                passageGroundPixel.a == 0)
                throw new System.InvalidOperationException("캐기 전과 캐고 난 뒤의 흙 픽셀이 같은 재질이 아닙니다.");

            var originalKindGrid = new WorldGrid(1, 1);
            originalKindGrid.SetRaw(0, 0, TileKind.Clay);
            originalKindGrid.Set(0, 0, TileKind.Empty);
            if (originalKindGrid.OriginalGroundAt(0, 0) != TileKind.Clay)
                throw new System.InvalidOperationException("굴착 전 지층 종류가 보존되지 않았습니다.");

            // 현재 시설은 시대별 아트를 사용한다. 삭제된 구형 공장 파일을 요구하지 않는다.
            for (int era = 0; era < VillageSpriteCatalog.EraCount; era++)
            {
                if (VillageSpriteCatalog.EraDropoff(era) == null || VillageSpriteCatalog.EraShop(era) == null ||
                    VillageSpriteCatalog.EraHouse(era) == null || VillageSpriteCatalog.EraRestorer(era) == null)
                    throw new System.InvalidOperationException($"{era}시대 시설 스프라이트 연결이 누락되었습니다.");
            }
            if (EnvironmentSpriteCatalog.RecycledProduct(MaterialKind.General) == null ||
                EnvironmentSpriteCatalog.RecycledProduct(MaterialKind.Relic) == null)
                throw new System.InvalidOperationException("분리수거장 픽셀 이미지를 만들지 못했습니다.");

            foreach (Facing facing in System.Enum.GetValues(typeof(Facing)))
            {
                if (PlayerSpriteCatalog.Frames(PlayerVisualState.Walk, facing).Length != 4 ||
                    PlayerSpriteCatalog.Frames(PlayerVisualState.Dig, facing).Length != 4)
                    throw new System.InvalidOperationException(
                        $"플레이어 {facing} 방향 걷기·굴착 애니메이션 프레임이 올바르지 않습니다.");
            }

            Vector2Int sample = new Vector2Int(7, 9);
            Vector3 world = WorldGrid.CellToWorld(sample.x, sample.y);
            if (WorldGrid.WorldToCell(world) != sample ||
                !Mathf.Approximately(
                    WorldGrid.CellToWorld(sample.x + 1, sample.y).x - world.x,
                    GameConfig.CellSize))
                throw new System.InvalidOperationException("1.5배 셀 좌표 변환이 일치하지 않습니다.");

            var station = preview.Find("WorldView/Markers/SurfaceTrashDropOff/SurfaceBin");
            var highlight = preview.Find("WorldView/DigHighlight");
            var featureHints = preview.Find("WorldView/Tilemap Grid/Dig Feature Hints");
            if (station == null || station.GetComponent<SpriteRenderer>()?.sprite == null)
                throw new System.InvalidOperationException("지상 분리수거장 에셋이 미리보기에 생성되지 않았습니다.");
            if (highlight == null || !Mathf.Approximately(highlight.localScale.x, GameConfig.CellSize))
                throw new System.InvalidOperationException("굴착 조준 표시가 확대된 셀 크기와 맞지 않습니다.");
            if (featureHints == null || featureHints.GetComponent<UnityEngine.Tilemaps.Tilemap>() == null)
                throw new System.InvalidOperationException("특수 지층 힌트 타일맵이 생성되지 않았습니다.");
            var game = preview.GetComponent<GameManager>();
            if (game == null || game.Player == null || game.Player.IsFalling)
                throw new System.InvalidOperationException("플레이어 무중력 이동 상태가 적용되지 않았습니다.");

            Debug.Log($"[DigGame] 동일 재질 U자 굴·8방향 연결 통로 · CC0 흙 변형 16종 · " +
                $"통로 마스크 256종 · 환경 에셋 {names.Length}개 · 무중력 검증 완료");
        }

        static Color32 SpritePixel(Sprite sprite, int x, int y)
        {
            if (sprite == null || sprite.texture == null || !sprite.texture.isReadable)
                throw new System.InvalidOperationException("검증할 환경 스프라이트 텍스처를 읽을 수 없습니다.");

            int textureX = Mathf.RoundToInt(sprite.rect.x) + x;
            int textureY = Mathf.RoundToInt(sprite.rect.y) + y;
            return sprite.texture.GetPixel(textureX, textureY);
        }

        static void ValidateBalance()
        {
            const int seedCount = 16;
            int minStarterValue = int.MaxValue;
            int maxStarterValue = 0;
            int starterValueSum = 0;
            int minStarterRecycled = int.MaxValue;
            int starterRecycledSum = 0;

            for (int seed = 1; seed <= seedCount; seed++)
            {
                var grid = WorldGenerator.Generate(seed * 7919);
                int entranceX = WorldGenerator.SurfaceEntranceX(grid);
                int startX = entranceX - GameConfig.StarterTrashCount / 2;
                int starterCount = 0;
                int starterValue = 0;
                int starterRecycled = 0;

                for (int i = 0; i < GameConfig.StarterTrashCount; i++)
                {
                    int x = Mathf.Clamp(startX + i, 1, grid.Width - 2);
                    if (!grid.TryPeekTreasure(x, GameConfig.StarterTrashRow, out var item)) continue;
                    starterCount++;
                    starterValue += item.Value;
                    starterRecycled += Mathf.RoundToInt(
                        item.Value * RecyclingInfo.ValueMultiplier(item.Material));
                }

                if (starterCount != GameConfig.StarterTrashCount)
                    throw new System.InvalidOperationException($"시작 쓰레기가 {starterCount}개만 배치되었습니다.");
                if (starterRecycled < GameConfig.QuotaForDay(1) * 0.70f)
                    throw new System.InvalidOperationException(
                        $"시작 쓰레기 재활용가 {starterRecycled}원이 첫날 할당량의 70%보다 낮습니다.");

                minStarterValue = Mathf.Min(minStarterValue, starterValue);
                maxStarterValue = Mathf.Max(maxStarterValue, starterValue);
                starterValueSum += starterValue;
                minStarterRecycled = Mathf.Min(minStarterRecycled, starterRecycled);
                starterRecycledSum += starterRecycled;
            }

            Debug.Log($"[DigGame] 일차별 할당량 — " +
                $"{GameConfig.QuotaForDay(1):N0} → {GameConfig.QuotaForDay(2):N0} → " +
                $"{GameConfig.QuotaForDay(3):N0} → {GameConfig.QuotaForDay(4):N0} → " +
                $"{GameConfig.QuotaForDay(5):N0} → {GameConfig.QuotaForDay(6):N0} → " +
                $"{GameConfig.QuotaForDay(7):N0}원");
            Debug.Log($"[DigGame] 시작 쓰레기 {GameConfig.StarterTrashCount}개 가치 — " +
                $"원물 최소 {minStarterValue:N0} / 평균 {starterValueSum / seedCount:N0} / 최대 {maxStarterValue:N0}원 · " +
                $"재활용 최소 {minStarterRecycled:N0} / 평균 {starterRecycledSum / seedCount:N0}원");
        }

        static void ValidateLootDistribution()
        {
            var grid = WorldGenerator.Generate(20260831);
            var names = new HashSet<string>();
            var count = new int[4];
            var singleSlot = new int[4];
            var valueSum = new long[4];
            var featureCount = new int[4];

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

            foreach (var (cell, feature) in grid.AllDigFeatures)
            {
                if (!TileInfo.IsDiggable(grid.Get(cell.x, cell.y)))
                    throw new System.InvalidOperationException($"특수 지층이 팔 수 없는 칸 ({cell.x}, {cell.y})에 놓였습니다.");
                featureCount[(int)feature]++;
                if ((feature == DigFeature.CompactCache || feature == DigFeature.GasPocket) &&
                    !grid.HasTreasure(cell.x, cell.y))
                    throw new System.InvalidOperationException($"보상형 특수 지층 ({cell.x}, {cell.y})에 쓰레기가 없습니다.");
            }

            if (grid.Width != GameConfig.WorldWidth || grid.Width < 183)
                throw new System.InvalidOperationException($"월드 폭이 충분히 확장되지 않았습니다: {grid.Width}칸");
            if (names.Count < 10)
                throw new System.InvalidOperationException($"쓰레기 종류가 {names.Count}개뿐입니다.");
            for (int feature = 1; feature <= 3; feature++)
                if (featureCount[feature] < 20)
                    throw new System.InvalidOperationException($"{(DigFeature)feature} 특수 지층이 {featureCount[feature]}개뿐입니다.");

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
            Debug.Log($"[DigGame] 특수 지층 — 푸석한 흙 {featureCount[(int)DigFeature.LooseSoil]} · " +
                $"압축층 {featureCount[(int)DigFeature.CompactCache]} · 가스층 {featureCount[(int)DigFeature.GasPocket]}개");
        }

        static void ValidatePowerDigScaling()
        {
            TileKind[] kinds =
            {
                TileKind.SoftDirt, TileKind.Clay, TileKind.Stone, TileKind.HardRock
            };

            foreach (TileKind kind in kinds)
            {
                float basePower = TileInfo.PowerDigDurationMultiplier(kind, GameConfig.BasePower);
                float oneUpgrade = TileInfo.PowerDigDurationMultiplier(kind, GameConfig.BasePower + 1);
                float twoUpgrades = TileInfo.PowerDigDurationMultiplier(kind, GameConfig.BasePower + 2);

                if (!Mathf.Approximately(basePower, 1f) || oneUpgrade >= basePower || twoUpgrades >= oneUpgrade)
                    throw new System.InvalidOperationException(
                        $"{TileInfo.DisplayName(kind)}의 힘 굴착 가속이 단계적으로 적용되지 않습니다.");
            }

            float softDirtAtPowerFour = TileInfo.PowerDigDurationMultiplier(TileKind.SoftDirt, 4);
            if (!Mathf.Approximately(softDirtAtPowerFour, 1f / 1.6f))
                throw new System.InvalidOperationException("힘 굴착 속도 보너스가 설정값과 다릅니다.");

            Debug.Log("[DigGame] 힘 굴착 가속 — 강화 1단계당 속도 +20% · 지층 해금과 분리 · 총 기력 소모 유지");
        }

        static void ValidateMineUtility()
        {
            var mine = new MineSite(999, new RectInt(2, 2, 2, 2), Zone.Surface, 1, 20);
            var rng = new System.Random(20260831);
            for (int i = 0; i < GameConfig.MineDiscoveryCacheCount; i++)
                if (!mine.ExtractOne(rng))
                    throw new System.InvalidOperationException("도시광산 발견 샘플을 생성하지 못했습니다.");

            var bag = new List<LootItem>
            {
                new LootItem("큰 폐기물", 80, Color.gray, 3, MaterialKind.General),
                new LootItem("작은 폐기물", 40, Color.gray, 1, MaterialKind.Plastic)
            };
            int stored = mine.StoreFrom(bag);
            if (stored != 2 || bag.Count != 0 || mine.Stockpile.Count != GameConfig.MineDiscoveryCacheCount + 2)
                throw new System.InvalidOperationException("도시광산 안전 보관 기능이 가방을 올바르게 옮기지 못했습니다.");

            // 앞에 큰 물건이 있어도 남은 부피에 맞는 작은 물건을 찾아 회수해야 한다.
            var fittingMine = new MineSite(1000, new RectInt(0, 0, 2, 2), Zone.Surface, 1, 0);
            fittingMine.Stockpile.Add(new LootItem("큰 폐기물", 80, Color.gray, 3, MaterialKind.General));
            fittingMine.Stockpile.Add(new LootItem("작은 폐기물", 40, Color.gray, 1, MaterialKind.Plastic));
            if (!fittingMine.TryTakeFitting(1, out var fitting) || fitting.Volume != 1)
                throw new System.InvalidOperationException("도시광산 보관품의 부피별 회수 기능이 작동하지 않습니다.");

            int recycled = Mathf.RoundToInt(mine.StockpileRawValue * RecyclingInfo.ValueMultiplier(MaterialKind.Metal));
            if (recycled <= mine.StockpileRawValue)
                throw new System.InvalidOperationException("도시광산 운반품에 재활용 가치가 적용되지 않습니다.");

            Debug.Log($"[DigGame] 도시광산 — 발견 샘플 {GameConfig.MineDiscoveryCacheCount}개 · " +
                $"안전 보관 {GameConfig.MineStockpileCap}칸 · 자동 재활용 운반 검증 완료");
        }

        static void ValidateSurfaceEntrance()
        {
            var grid = WorldGenerator.Generate(20260831);
            int entranceX = WorldGenerator.SurfaceEntranceX(grid);

            if (TileInfo.IsDiggable(TileKind.Grass) || TileInfo.RequiredPower(TileKind.Grass) >= 0)
                throw new System.InvalidOperationException("잔디 지반이 파괴 불가로 설정되지 않았습니다.");

            for (int row = 0; row < GameConfig.GrassLayerRows; row++)
            {
                int y = GameConfig.SurfaceRow + row;
                if (grid.Get(entranceX, y) != TileKind.Empty ||
                    grid.OriginalGroundAt(entranceX, y) != TileKind.Grass)
                    throw new System.InvalidOperationException($"지상 입구 통로 {row + 1}번째 칸이 열리지 않았습니다.");

                if (grid.Get(entranceX - 1, y) != TileKind.Grass ||
                    grid.Get(entranceX + 1, y) != TileKind.Grass)
                    throw new System.InvalidOperationException("지상 입구 옆의 잔디 지반 두께가 유지되지 않았습니다.");
            }

            if (!TileInfo.IsDiggable(grid.Get(entranceX, GameConfig.StarterTrashRow)))
                throw new System.InvalidOperationException("지상 입구 아래가 굴착 가능한 첫 흙으로 이어지지 않습니다.");

            Sprite grass = EnvironmentSpriteCatalog.GroundFill(TileKind.Grass, 0);
            Sprite grassPassage = EnvironmentSpriteCatalog.PassageForMask(TileKind.Grass, 5, 0);
            if (grass == null || grassPassage == null || grass.rect.width != 16f || grassPassage.rect.width != 16f)
                throw new System.InvalidOperationException("초록 잔디 지반 또는 입구 통로 픽셀을 생성하지 못했습니다.");

            Debug.Log($"[DigGame] 지상 잔디 — 파괴 불가 {GameConfig.GrassLayerRows}칸 · " +
                $"마을 중심 +{GameConfig.SurfaceEntranceOffsetX}칸 세로 입구 검증 완료");
        }

        static void ValidateDayTiming()
        {
            if (!Mathf.Approximately(GameConfig.DayLengthSeconds, 120f))
                throw new System.InvalidOperationException("하루 길이가 2분으로 설정되지 않았습니다.");
            if (!Mathf.Approximately(GameConfig.DayEndWarnSeconds, 20f))
                throw new System.InvalidOperationException("마감 경고가 마지막 20초로 설정되지 않았습니다.");
            if (GameConfig.DayEndWarnSeconds >= GameConfig.DayLengthSeconds)
                throw new System.InvalidOperationException("마감 경고 시간이 하루 길이보다 짧아야 합니다.");

            Debug.Log("[DigGame] 하루 주기 — 낮 120초 · 마지막 20초 마감 경고 검증 완료");
        }

        static void ValidateWorkerEconomy()
        {
            int cycles = Mathf.FloorToInt(GameConfig.DayLengthSeconds / GameConfig.MinerExtractInterval);
            int minGross = int.MaxValue;
            int totalGross = 0;
            const int samples = 32;

            for (int seed = 0; seed < samples; seed++)
            {
                var rng = new System.Random(9109 + seed * 7919);
                int gross = 0;
                for (int cycle = 0; cycle < cycles; cycle++)
                {
                    LootItem item = LootTable.RollOre(1, rng); // 지상 근처 도시광산
                    gross += Mathf.RoundToInt(item.Value * RecyclingInfo.ValueMultiplier(item.Material));
                }
                minGross = Mathf.Min(minGross, gross);
                totalGross += gross;
            }

            int firstDayNet = minGross - GameConfig.MinerHireCost - GameConfig.MinerWage;
            if (cycles < 5 || minGross < 2000 || firstDayNet < 1000)
                throw new System.InvalidOperationException(
                    $"자동 재활용 일꾼의 하루 수익이 너무 낮습니다: 최소 {minGross}원, 첫날 순수익 {firstDayNet}원");

            Debug.Log($"[DigGame] 자동 재활용 일꾼 — 하루 {cycles}회 · " +
                $"지상 광산 총수익 최소 {minGross:N0} / 평균 {totalGross / samples:N0}원 · " +
                $"고용비·일당 제외 첫날 순수익 최소 {firstDayNet:N0}원");
        }

        static void ValidatePixelUi()
        {
            string[] textures =
            {
                "panel", "button_normal", "button_highlight", "slot_normal", "slot_selected",
                "bar_frame", "speech_bubble", "item_card", "quest_badge"
            };

            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D texture = Resources.Load<Texture2D>($"PixelUI/{textures[i]}");
                if (texture == null)
                    throw new System.InvalidOperationException($"픽셀 UI 이미지가 없습니다: {textures[i]}");
                if (texture.filterMode != FilterMode.Point)
                    throw new System.InvalidOperationException($"픽셀 UI 필터가 Point가 아닙니다: {textures[i]}");
            }

            Font font = Resources.Load<Font>("Fonts/Galmuri11");
            if (font == null)
                throw new System.InvalidOperationException("한글 픽셀 폰트 Galmuri11을 불러오지 못했습니다.");

            var skin = PixelUISkin.Load();
            if (skin == null || skin.ButtonStyle.normal.background == null ||
                skin.PanelStyle.normal.background == null || skin.HudPanelStyle.normal.background == null)
                throw new System.InvalidOperationException("픽셀 UI 스킨을 생성하지 못했습니다.");

            if (skin.ButtonStyle.border.left > 4 || skin.HudPanelStyle.border.left > 4 ||
                skin.BarStyle.border.left > 3 || skin.PanelStyle.border.left > 6)
                throw new System.InvalidOperationException("얇은 UI 프레임의 테두리 규격이 다시 두꺼워졌습니다.");

            Texture2D titleScene = Resources.Load<Texture2D>("PixelUI/title_scene_background");
            if (titleScene == null || titleScene.width < 1280 || titleScene.height < 720)
                throw new System.InvalidOperationException("16:9 타이틀 장면 배경이 없거나 해상도가 너무 작습니다.");
            float titleAspect = titleScene.width / (float)titleScene.height;
            if (Mathf.Abs(titleAspect - 16f / 9f) > 0.03f || titleScene.filterMode != FilterMode.Point)
                throw new System.InvalidOperationException("타이틀 장면은 16:9 Point 필터 픽셀 이미지여야 합니다.");

            ValidateDesktopTitleLayout(font, skin);

            Debug.Log($"[DigGame] 픽셀 UI — 개별 이미지 {textures.Length}종 · " +
                $"타이틀 장면 {titleScene.width}x{titleScene.height} · " +
                "PC 1920x1080 타이틀 안전 영역 및 한글 폭 검증 · " +
                "게이지 3px / 버튼 4px / HUD 4px / 메뉴 6px 프레임 · Galmuri11 한글 폰트 검증 완료");
        }

        static void ValidateDesktopTitleLayout(Font font, PixelUISkin skin)
        {
            var checks = new[]
            {
                new { Name = "제목", Text = "쓰레기 집게 소년", FontSize = 72, Width = 696f, Height = 100f },
                new { Name = "부제", Text = "쓰집소", FontSize = 30, Width = 696f, Height = 40f },
                new { Name = "문구", Text = "땅을 파고 · 쓰레기를 집고 · 마을을 되살려라", FontSize = 24, Width = 696f, Height = 36f },
                new { Name = "하단 안내", Text = "E / I 인벤토리 · ESC 메뉴 · TAB 간단 조작 안내", FontSize = 18, Width = 776f, Height = 28f }
            };

            for (int i = 0; i < checks.Length; i++)
            {
                var style = new GUIStyle
                {
                    font = font,
                    fontSize = checks[i].FontSize,
                    fontStyle = FontStyle.Bold,
                    wordWrap = false
                };
                Vector2 size = style.CalcSize(new GUIContent(checks[i].Text));
                if (size.x > checks[i].Width || size.y > checks[i].Height)
                    throw new System.InvalidOperationException(
                        $"PC 타이틀 {checks[i].Name} 글씨가 안전 영역을 벗어납니다: {size.x:0}x{size.y:0}");
            }

            GUIStyle buttonStyle = new GUIStyle(skin.ButtonStyle)
            {
                font = font,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };
            string[] buttonTexts = { "게임 시작", "이어하기", "새 게임", "설정", "조작법" };
            for (int i = 0; i < buttonTexts.Length; i++)
            {
                Vector2 size = buttonStyle.CalcSize(new GUIContent(buttonTexts[i]));
                if (size.x > 378f || size.y > 44f)
                    throw new System.InvalidOperationException(
                        $"PC 타이틀 버튼 글씨가 잘립니다: {buttonTexts[i]} ({size.x:0}x{size.y:0})");
            }

            Vector2Int[] desktopResolutions =
            {
                new Vector2Int(1366, 768),
                new Vector2Int(1680, 1050),
                new Vector2Int(1920, 1080),
                new Vector2Int(2560, 1080),
                new Vector2Int(2560, 1440),
                new Vector2Int(3840, 2160)
            };
            for (int i = 0; i < desktopResolutions.Length; i++)
            {
                float scale = Mathf.Min(
                    desktopResolutions[i].x / GameHUD.TitleReferenceWidth,
                    desktopResolutions[i].y / GameHUD.TitleReferenceHeight);
                float canvasW = GameHUD.TitleReferenceWidth * scale;
                float canvasH = GameHUD.TitleReferenceHeight * scale;
                if (canvasW > desktopResolutions[i].x + 0.01f ||
                    canvasH > desktopResolutions[i].y + 0.01f)
                    throw new System.InvalidOperationException(
                        $"PC 타이틀 안전 캔버스가 화면을 벗어납니다: {desktopResolutions[i].x}x{desktopResolutions[i].y}");
            }
        }
    }
}
