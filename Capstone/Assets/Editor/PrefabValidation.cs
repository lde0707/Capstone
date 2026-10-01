using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DigGame.Editor
{
    public static class PrefabValidation
    {
        const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [MenuItem("DigGame/Prefabs/Validate Prefabs and Gameplay")]
        public static void Run()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GamePrefabCatalog>(PrefabAssets.CatalogPath);
            Require(catalog != null, "카탈로그가 없습니다.");
            catalog.Validate();
            foreach (PrefabId id in Enum.GetValues(typeof(PrefabId)))
            {
                var asset = catalog.Get(id);
                Require(PrefabUtility.IsPartOfPrefabAsset(asset), $"{id}가 프리팹 에셋이 아닙니다.");
                foreach (var t in asset.GetComponentsInChildren<Transform>(true))
                    Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0,
                        $"{id}/{t.name}에 Missing Script가 있습니다.");
            }

            var root = new GameObject("Prefab Validation");
            var randomState = UnityEngine.Random.state;
            try
            {
                IGameObjectFactory factory = new PrefabGameObjectFactory(catalog);
                var player1 = factory.Create<PlayerController>(PrefabId.Player, root.transform);
                var player2 = factory.Create<PlayerController>(PrefabId.Player, root.transform);
                Require(player1 != player2 && player1.GetComponent<SpriteRenderer>() != player2.GetComponent<SpriteRenderer>(),
                    "플레이어 인스턴스가 분리되지 않았습니다.");
                player1.transform.localScale = Vector3.one * 9f;
                Require(player2.transform.localScale == catalog.Get(PrefabId.Player).transform.localScale,
                    "인스턴스 변경이 원본 또는 다른 인스턴스에 전파되었습니다.");
                UnityEngine.Object.DestroyImmediate(player1.gameObject);
                UnityEngine.Object.DestroyImmediate(player2.gameObject);

                bool rejected = false;
                try { factory.Create<MinerWorker>(PrefabId.Villager, root.transform); }
                catch (InvalidOperationException) { rejected = true; }
                Require(rejected, "잘못된 프리팹/타입 요청이 거부되지 않았습니다.");

                var game = root.AddComponent<GameManager>();
                game.StartGame(20260927, false, factory);
                Require(game.Initialized && game.Player.GetComponents<SpriteRenderer>().Length == 1,
                    "게임 초기화 또는 플레이어 렌더러 연결 실패.");
                Require(game.Player.Inventory.Count == 0 && game.Money == GameConfig.StartingMoney,
                    "시작 상태가 바뀌었습니다.");
                Require(root.GetComponentsInChildren<Villager>().Length > 0, "초기 주민 생성 실패.");

                // 입구 근처 주민 퀘스트를 받고 1층 고유 유물 7종으로 다음 집게를 해금한다.
                var quests = game.PickaxeQuests;
                var firstGiver = quests.Giver;
                Require(firstGiver != null && firstGiver.HasPickaxeQuest && quests.PickaxeTier == 1,
                    "초기 집게 퀘스트 주민/단계 설정 실패.");
                game.Player.transform.position = firstGiver.transform.position;
                Require(quests.TryInteract() && quests.Accepted, "집게 퀘스트 수락 실패.");
                var firstLayerRelics = RelicCatalog.OfDepth(1);
                for (int i = 0; i < GameConfig.PickaxeQuestRelicCounts[0]; i++)
                    game.Relics.Excavate(firstLayerRelics[i].Id);
                Require(quests.Progress == GameConfig.PickaxeQuestRelicCounts[0], "고유 유물 진행도 집계 실패.");
                Require(quests.TryInteract() && quests.PickaxeTier == 2 && quests.Giver != firstGiver,
                    "집게 보상 또는 다음 주민 전환 실패.");
                Require(firstGiver != null && !firstGiver.HasPickaxeQuest,
                    "이전 퀘스트 주민이 제거되거나 퀘스트 표시가 남았습니다.");

                int cx = game.Grid.Width / 2;
                var mine = new MineSite(9999, new RectInt(cx, GameConfig.SurfaceRow + 4, 2, 2), Zone.Surface, 1, 30);
                var miner = factory.Create<Worker>(PrefabId.Miner, root.transform);
                var hauler = factory.Create<Worker>(PrefabId.Hauler, root.transform);
                miner.Init(game, mine, 11);
                hauler.Init(game, mine, 12);
                Require(miner is MinerWorker && hauler is HaulerWorker && miner.Wage == GameConfig.MinerWage
                    && hauler.Wage == GameConfig.HaulerWage, "일꾼 다형성/일당 오류.");
                int moneyBefore = game.Money;
                int deliveredBefore = game.DeliveredToday;
                Tick(miner, GameConfig.MinerExtractInterval);
                Require(game.Money > moneyBefore && game.DeliveredToday > deliveredBefore && mine.Reserve == 29,
                    "수거 일꾼의 20초 자동 재활용 실패.");

                // 실제 연결 통로를 만든 뒤 운반 상태 머신으로 적재, 이동, 판매한다.
                var center = mine.CenterCell;
                for (int y = game.RecycleDropCell.y; y <= center.y; y++) game.Grid.SetRaw(center.x, y, TileKind.Empty);
                for (int x = game.RecycleDropCell.x; x <= center.x; x++) game.Grid.SetRaw(x, game.RecycleDropCell.y, TileKind.Empty);
                mine.Stockpile.Add(new LootItem("운반 검증", 123, Color.white, 1, MaterialKind.Metal));
                moneyBefore = game.Money;
                for (int i = 0; i < 100 && game.Money == moneyBefore; i++) Tick(hauler, 1f);
                Require(game.Money > moneyBefore && mine.Stockpile.Count == 0, "운반 일꾼의 경로/판매 실패.");
                // 마감에 운반 중이던 화물이 보관함에 보존되어야 한다.
                mine.Stockpile.Add(new LootItem("마감 화물", 31, Color.white, 1, MaterialKind.Metal));
                for (int i = 0; i < 100 && mine.Stockpile.Count > 0; i++) Tick(hauler, 1f);
                Require(mine.Stockpile.Count == 0, "마감 검증용 화물 적재 실패.");
                hauler.DumpCargoToMine();
                Require(mine.Stockpile.Count == 1, "마감 화물 보존 실패.");

                var pet = factory.Create<PetCompanion>(PrefabId.Pet, root.transform);
                pet.Init(game, PetKind.Mole);
                Require(root.transform.Find("MoleArrow") != null, "펫 화살표 프리팹 생성 실패.");
                var firstRelic = RelicCatalog.Get(0);
                Require(firstRelic != null, "유물 데이터가 없습니다.");
                game.Village.PlaceRestoredRelic(firstRelic);
                Require(root.GetComponentsInChildren<RelicUnit>().Length > 0, "유물 프리팹 생성 실패.");

                // 단계 달성으로 주민/건물/효과까지 프리팹 경로가 연결되는지 검증한다.
                game.AddRelicIncome(GameConfig.QuotaForDay(1), "검증");
                Require(game.QuotaCleared >= 1 && root.GetComponentsInChildren<VillageUpgradeFx>().Length > 0,
                    "할당량 달성 마을 성장/효과 연결 실패.");
                game.View.ShowRecyclingOutputs(new[] { MaterialKind.Metal, MaterialKind.Glass });
                Require(root.GetComponentsInChildren<RecyclingOutputFx>().Length >= 2, "재활용 효과 생성 실패.");

                var previewFactory = new EditorPrefabFactory(catalog);
                var resident = previewFactory.Create<Villager>(PrefabId.Villager, root.transform);
                Require(PrefabUtility.IsPartOfPrefabInstance(resident.gameObject), "편집 미리보기의 프리팹 연결 실패.");
                Debug.Log("[DigGame] PREFAB_VALIDATION_PASSED: 19 assets, initialization, quest giver/unique relic/tier reward, isolated instances, miner income, hauler delivery/cargo, pets, relics, growth, effects, editor links.");
            }
            finally
            {
                // 펫의 OnDestroy는 런타임 지연 Destroy를 사용하므로 EditMode에서는 먼저 정리한다.
                var arrow = root.transform.Find("MoleArrow");
                if (arrow != null) UnityEngine.Object.DestroyImmediate(arrow.gameObject);
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Random.state = randomState;
                SpriteFactory.ClearCache();
            }
        }

        static void Tick(Worker worker, float deltaTime) =>
            worker.GetType().GetMethod("TickWork", PrivateInstance).Invoke(worker, new object[] { deltaTime });

        static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }
    }
}
