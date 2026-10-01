using UnityEngine;

namespace DigGame
{
    public enum StatKind { MoveSpeed = 0, Stamina = 1, Power = 2 }

    /// <summary>플레이어의 이동·기력·굴착 속도 능력치와 업그레이드 단계.</summary>
    public class PlayerStats
    {
        public int MoveSpeedLevel;
        public int StaminaLevel;
        public int PowerLevel;

        public float Stamina;

        public PlayerStats()
        {
            Stamina = MaxStamina;
        }

        public float MoveSpeed => GameConfig.BaseMoveSpeed + MoveSpeedLevel * 0.9f;
        public float MaxStamina => GameConfig.BaseMaxStamina + StaminaLevel * 35f;
        public int Power => GameConfig.BasePower + PowerLevel;
        public int DigSpeedBonusPercent => Mathf.RoundToInt(PowerLevel * GameConfig.PowerDigSpeedBonusPerLevel * 100f);

        /// <summary>가방 용량 (부피). 힘은 굴착 속도만 올리므로 기력 강화만 용량에 영향을 준다.</summary>
        public int BagVolume => GameConfig.BaseBagVolume + StaminaLevel * 2;

        public int LevelOf(StatKind k)
        {
            switch (k)
            {
                case StatKind.MoveSpeed: return MoveSpeedLevel;
                case StatKind.Stamina: return StaminaLevel;
                default: return PowerLevel;
            }
        }

        public void Increase(StatKind k)
        {
            switch (k)
            {
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
            if (k == StatKind.Power)
                return Mathf.RoundToInt(GameConfig.PowerUpgradeBaseCost *
                    Mathf.Pow(GameConfig.PowerUpgradeCostGrowth, lv));
            return Mathf.RoundToInt(GameConfig.UpgradeBaseCost *
                Mathf.Pow(GameConfig.UpgradeCostGrowth, lv));
        }

        public int MaxLevelOf(StatKind k) =>
            k == StatKind.Power ? GameConfig.MaxPowerUpgradeLevel : GameConfig.MaxUpgradeLevel;

        public bool IsMaxed(StatKind k) => LevelOf(k) >= MaxLevelOf(k);

        public static string DisplayName(StatKind k)
        {
            switch (k)
            {
                case StatKind.MoveSpeed: return "이동속도";
                case StatKind.Stamina: return "기력";
                default: return "힘(굴착 속도)";
            }
        }

        public string ValueText(StatKind k)
        {
            switch (k)
            {
                case StatKind.MoveSpeed: return $"{MoveSpeed:0.0}";
                case StatKind.Stamina: return $"{MaxStamina:0}";
                default: return $"속도 +{DigSpeedBonusPercent}%";
            }
        }

        /// <summary>다음 단계에서 얼마가 되는지 (상점 표시용).</summary>
        public string NextValueText(StatKind k)
        {
            switch (k)
            {
                case StatKind.MoveSpeed: return $"{GameConfig.BaseMoveSpeed + (MoveSpeedLevel + 1) * 0.9f:0.0}";
                case StatKind.Stamina: return $"{GameConfig.BaseMaxStamina + (StaminaLevel + 1) * 35f:0}";
                default: return $"속도 +{Mathf.RoundToInt((PowerLevel + 1) * GameConfig.PowerDigSpeedBonusPerLevel * 100f)}%";
            }
        }
    }
}
