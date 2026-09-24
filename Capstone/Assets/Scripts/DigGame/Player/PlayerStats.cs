using UnityEngine;

namespace DigGame
{
    public enum StatKind { Health = 0, MoveSpeed = 1, Stamina = 2, Power = 3 }

    /// <summary>플레이어의 4가지 능력치와 업그레이드 단계.</summary>
    public class PlayerStats
    {
        public int HealthLevel;
        public int MoveSpeedLevel;
        public int StaminaLevel;
        public int PowerLevel;

        public float Health;
        public float Stamina;

        public PlayerStats()
        {
            Health = MaxHealth;
            Stamina = MaxStamina;
        }

        public float MaxHealth => GameConfig.BaseMaxHealth + HealthLevel * 25f;
        public float MoveSpeed => GameConfig.BaseMoveSpeed + MoveSpeedLevel * 0.9f;
        public float MaxStamina => GameConfig.BaseMaxStamina + StaminaLevel * 35f;
        public int Power => GameConfig.BasePower + PowerLevel;

        /// <summary>가방 용량 (부피). 힘·기력 강화로도 조금씩 늘어난다.</summary>
        public int BagVolume => GameConfig.BaseBagVolume + PowerLevel * 2 + StaminaLevel * 2;

        public int LevelOf(StatKind k)
        {
            switch (k)
            {
                case StatKind.Health: return HealthLevel;
                case StatKind.MoveSpeed: return MoveSpeedLevel;
                case StatKind.Stamina: return StaminaLevel;
                default: return PowerLevel;
            }
        }

        public void Increase(StatKind k)
        {
            switch (k)
            {
                case StatKind.Health:
                    HealthLevel++;
                    Health = MaxHealth;
                    break;
                case StatKind.MoveSpeed:
                    MoveSpeedLevel++;
                    break;
                case StatKind.Stamina:
                    StaminaLevel++;
                    Stamina = MaxStamina;
                    break;
                case StatKind.Power:
                    PowerLevel++;
                    break;
            }
        }

        public int CostOf(StatKind k)
        {
            int lv = LevelOf(k);
            // 힘은 핵심 능력치라 훨씬 비싸다. (1.6 -> 2.4, 기존 대비 +50%)
            float mult = k == StatKind.Power ? 2.4f : 1f;
            return Mathf.RoundToInt(GameConfig.UpgradeBaseCost * mult *
                                    Mathf.Pow(GameConfig.UpgradeCostGrowth, lv));
        }

        public bool IsMaxed(StatKind k) => LevelOf(k) >= GameConfig.MaxUpgradeLevel;

        public static string DisplayName(StatKind k)
        {
            switch (k)
            {
                case StatKind.Health: return "체력";
                case StatKind.MoveSpeed: return "이동속도";
                case StatKind.Stamina: return "기력";
                default: return "힘";
            }
        }

        public string ValueText(StatKind k)
        {
            switch (k)
            {
                case StatKind.Health: return $"{MaxHealth:0}";
                case StatKind.MoveSpeed: return $"{MoveSpeed:0.0}";
                case StatKind.Stamina: return $"{MaxStamina:0}";
                default: return $"{Power}";
            }
        }

        /// <summary>다음 단계에서 얼마가 되는지 (상점 표시용).</summary>
        public string NextValueText(StatKind k)
        {
            switch (k)
            {
                case StatKind.Health: return $"{GameConfig.BaseMaxHealth + (HealthLevel + 1) * 25f:0}";
                case StatKind.MoveSpeed: return $"{GameConfig.BaseMoveSpeed + (MoveSpeedLevel + 1) * 0.9f:0.0}";
                case StatKind.Stamina: return $"{GameConfig.BaseMaxStamina + (StaminaLevel + 1) * 35f:0}";
                default: return $"{GameConfig.BasePower + PowerLevel + 1}";
            }
        }
    }
}
