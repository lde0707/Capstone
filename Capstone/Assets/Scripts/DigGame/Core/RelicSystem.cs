using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 유물 수집 흐름을 관리한다.
    ///   땅속에서 발굴 → 가방과 별개로 등에 진다 → 지상 복원소에 E 로 투입
    ///   → 차례로 복원(시간) → 마을에 전시되거나 살아나서 돌아다닌다.
    /// 같은 유물을 또 캐면 복원하지 않고 자동으로 돈으로 바꾼다.
    /// </summary>
    public class RelicSystem : MonoBehaviour
    {
        public struct Job
        {
            public int RelicId;
            public float Remaining;
            /// <summary>이 작업이 시작될 때의 총 시간. 도중에 복원 속도가 빨라져도 진행바가 튀지 않는다.</summary>
            public float Total;
        }

        GameManager _game;

        readonly List<int> _carried = new List<int>();
        readonly List<Job> _queue = new List<Job>();
        bool[] _found;      // 땅에서 한 번이라도 캔 적 있는가
        bool[] _restored;   // 복원까지 마쳤는가

        public int CarriedCount => _carried.Count;
        public bool CarryFull => _carried.Count >= GameConfig.RelicCarryLimit;

        /// <summary>복원 대기열 (앞에서부터 처리된다).</summary>
        public IReadOnlyList<Job> Queue => _queue;
        public int QueueCount => _queue.Count;

        public int FoundCount { get; private set; }
        public int RestoredCount { get; private set; }
        public int TotalCount => RelicCatalog.Count;

        /// <summary>완성한 세트 수. 세트 하나마다 판매액이 오른다.</summary>
        public int CompletedSets { get; private set; }
        public float SetSellBonus => CompletedSets * GameConfig.SetSellBonus
            + (HasSet(RelicGroup.History) ? GameConfig.MuseumSetSellBonus : 0f);

        readonly HashSet<int> _sets = new HashSet<int>();

        /// <summary>그 분류를 전부 복원했는가 (= 랜드마크가 섰는가).</summary>
        public bool HasSet(RelicGroup g) => _sets.Contains((int)g);

        /// <summary>세계 기담관이 서면 복원이 빨라진다.</summary>
        public float RestoreSeconds => GameConfig.RelicRestoreSeconds
            * (HasSet(RelicGroup.Beast) ? GameConfig.CurioSetRestoreRate : 1f);

        /// <summary>추억의 거리가 서면 중복 유물이 두 배로 팔린다.</summary>
        public float DuplicateRate =>
            HasSet(RelicGroup.Modern) ? GameConfig.RetroSetDuplicateRate : 1f;

        public bool IsFound(int id) => _found != null && id >= 0 && id < _found.Length && _found[id];
        public bool IsRestored(int id) => _restored != null && id >= 0 && id < _restored.Length && _restored[id];

        /// <summary>대기열에 이미 들어가 있거나 등에 지고 있는가.</summary>
        public bool IsPending(int id)
        {
            for (int i = 0; i < _carried.Count; i++) if (_carried[i] == id) return true;
            for (int i = 0; i < _queue.Count; i++) if (_queue[i].RelicId == id) return true;
            return false;
        }

        /// <summary>그 그룹에서 복원을 마친 종 수.</summary>
        public int RestoredInGroup(RelicGroup group)
        {
            var defs = RelicCatalog.OfGroup(group);
            int n = 0;
            for (int i = 0; i < defs.Count; i++)
                if (IsRestored(defs[i].Id)) n++;
            return n;
        }

        public void Setup(GameManager game)
        {
            _game = game;
            _found = new bool[RelicCatalog.Count];
            _restored = new bool[RelicCatalog.Count];
        }

        // ==================================================================
        /// <summary>땅속에서 유물을 캤다. 중복이면 그 자리에서 환전된다.</summary>
        public void Excavate(int relicId)
        {
            var def = RelicCatalog.Get(relicId);
            if (def == null) return;

            if (!_found[relicId])
            {
                _found[relicId] = true;
                FoundCount++;
            }

            // 이미 복원했거나 처리 중인 유물은 다시 복원할 이유가 없다 → 자동 환전
            if (IsRestored(relicId) || IsPending(relicId))
            {
                int paid = Mathf.RoundToInt(def.DuplicateValue * DuplicateRate);
                _game.AddRelicIncome(paid, $"{def.Name}(중복)");
                _game.Log($"이미 가진 유물 — <b>{def.Name}</b> 을(를) 골동품상에 넘겼다 (+{paid:N0}원)" +
                    (DuplicateRate > 1f ? " <color=#ffd98a>[추억의 거리 x2]</color>" : ""));
                return;
            }

            if (CarryFull)
            {
                _game.Log($"유물을 더 들 수 없다 ({GameConfig.RelicCarryLimit}개) — 복원소에 먼저 맡기자");
                return;
            }

            _carried.Add(relicId);
            _game.Log($"■ 유물 발굴! <b>{def.Name}</b> ({def.Depth}층 · {def.GroupName}) " +
                $"— 복원소에 E 로 맡기면 마을에 나타난다 [{_carried.Count}/{GameConfig.RelicCarryLimit}]");
        }

        /// <summary>복원소에 등에 진 유물을 전부 맡긴다.</summary>
        public int DepositAll()
        {
            if (_carried.Count == 0) return 0;

            // 비싼 유물 하나 때문에 뒤의 싼 유물까지 막히지 않게, 살 수 있는 것부터 받는다.
            int cost = 0;
            var taken = new List<int>();
            for (int i = 0; i < _carried.Count; i++)
            {
                var def = RelicCatalog.Get(_carried[i]);
                if (def == null) continue;
                int price = RestoreCost(def);
                if (_game.Money < price) continue;
                if (!_game.TrySpend(price)) continue;

                cost += price;
                _queue.Add(new Job { RelicId = def.Id, Remaining = RestoreSeconds, Total = RestoreSeconds });
                taken.Add(i);
            }

            for (int i = taken.Count - 1; i >= 0; i--) _carried.RemoveAt(taken[i]);

            int accepted = taken.Count;
            if (accepted == 0)
                _game.Log($"복원 비용이 모자라다 (가장 싼 유물 {CheapestRestoreCost:N0}원) — 쓰레기를 더 팔고 오자");
            else
                _game.Log($"복원소에 유물 {accepted}개 투입 (비용 {cost:N0}원) — " +
                    $"한 개당 {RestoreSeconds:0}초 뒤에 마을에 나타난다" +
                    (_carried.Count > 0 ? $" · {_carried.Count}개는 돈이 모자라 그대로 지고 있다" : ""));
            return accepted;
        }

        /// <summary>지금 지고 있는 것 중 가장 싼 복원 비용.</summary>
        public int CheapestRestoreCost
        {
            get
            {
                int best = int.MaxValue;
                for (int i = 0; i < _carried.Count; i++)
                {
                    var def = RelicCatalog.Get(_carried[i]);
                    if (def != null) best = Mathf.Min(best, RestoreCost(def));
                }
                return best == int.MaxValue ? 0 : best;
            }
        }

        /// <summary>
        /// 이 유물을 지금 받아 줄 수 있는가.
        /// 중복은 그 자리에서 환전되므로 자리를 차지하지 않는다.
        /// </summary>
        public bool CanAccept(int relicId)
        {
            if (IsRestored(relicId) || IsPending(relicId)) return true;
            return !CarryFull;
        }

        public static int RestoreCost(RelicDef def) =>
            Mathf.RoundToInt(def.DuplicateValue * GameConfig.RelicRestoreCostRate / 10f) * 10;

        /// <summary>지금 등에 진 유물을 전부 맡기는 데 드는 돈.</summary>
        public int CarriedRestoreCost
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < _carried.Count; i++)
                {
                    var def = RelicCatalog.Get(_carried[i]);
                    if (def != null) sum += RestoreCost(def);
                }
                return sum;
            }
        }

        // ==================================================================
        void Update()
        {
            if (_game == null || !_game.IsRunning || _queue.Count == 0) return;

            var job = _queue[0];
            job.Remaining -= Time.deltaTime;
            if (job.Remaining > 0f)
            {
                _queue[0] = job;
                return;
            }

            _queue.RemoveAt(0);
            Complete(job.RelicId);
        }

        /// <summary>대기열 맨 앞의 진행도 (0~1).</summary>
        public float FrontProgress01
        {
            get
            {
                if (_queue.Count == 0) return 0f;
                var job = _queue[0];
                float total = job.Total > 0.01f ? job.Total : RestoreSeconds;
                return Mathf.Clamp01(1f - job.Remaining / Mathf.Max(0.01f, total));
            }
        }

        void Complete(int relicId)
        {
            var def = RelicCatalog.Get(relicId);
            if (def == null) return;

            if (!_restored[relicId])
            {
                _restored[relicId] = true;
                RestoredCount++;
            }

            int reward = Mathf.RoundToInt(def.DuplicateValue * GameConfig.RelicRestoreRewardRate / 10f) * 10;
            _game.AddRelicIncome(reward, $"{def.Name} 복원");

            bool wandered = _game.Village != null && _game.Village.PlaceRestoredRelic(def);
            string how = wandered ? "마을을 돌아다니기 시작했다" : "마을 한켠에 전시됐다";
            _game.Log($"✦ 복원 완료 — <b>{def.Name}</b> 이(가) {how}! (+{reward:N0}원 · 도감 {RestoredCount}/{TotalCount})");

            CheckSetBonus(def.Group);
        }

        void CheckSetBonus(RelicGroup group)
        {
            int total = RelicCatalog.OfGroup(group).Count;
            if (RestoredInGroup(group) < total) return;
            if (!_sets.Add((int)group)) return;          // 이미 센 세트

            CompletedSets = _sets.Count;

            int g = (int)group;
            _game.Log($"★ 세트 완성 — <b>{RelicCatalog.SetBonusName(group)}</b>! " +
                $"마을에 <b>{VillageSpriteCatalog.SetLandmarkNames[g]}</b> 이(가) 들어섰다 " +
                $"— {VillageSpriteCatalog.SetLandmarkDescs[g]}");
            _game.Log($"   판매액 +{SetSellBonus * 100f:0}% · {SetPerkText(group)}");

            if (_game.Village != null) _game.Village.PlaceSetLandmark(group);
        }

        /// <summary>세트마다 다른 실질 보상 한 줄.</summary>
        public static string SetPerkText(RelicGroup g)
        {
            switch (g)
            {
                case RelicGroup.Dino:
                    return $"돌아다니는 유물 상한 +{GameConfig.DinoSetWanderBonus}";
                case RelicGroup.Yokai:
                    return "밤마다 도깨비불이 마을을 떠다닌다";
                case RelicGroup.Beast:
                    return $"복원 시간 {(1f - GameConfig.CurioSetRestoreRate) * 100f:0}% 단축";
                case RelicGroup.History:
                    return $"판매액 추가 +{GameConfig.MuseumSetSellBonus * 100f:0}%";
                case RelicGroup.Modern:
                    return $"중복 유물 환전액 x{GameConfig.RetroSetDuplicateRate:0}";
                default:
                    return "UFO 가 마을 위에 상주한다";
            }
        }

        /// <summary>밤이 되면 남은 복원은 하룻밤 사이에 전부 끝난다.</summary>
        public void FinishAllOvernight()
        {
            while (_queue.Count > 0)
            {
                int id = _queue[0].RelicId;
                _queue.RemoveAt(0);
                Complete(id);
            }
        }
    }
}
