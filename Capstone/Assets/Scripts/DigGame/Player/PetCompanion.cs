using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 플레이어를 따라다니는 펫.
    /// 두더지는 근처에서 가장 비싼 쓰레기 방향을 화살표로 가리킨다.
    /// (반딧불이 시야·거북이 가방 보너스는 PlayerGear 가 수치로 처리한다.)
    /// </summary>
    public class PetCompanion : MonoBehaviour
    {
        GameManager _game;
        PetKind _kind;
        SpriteRenderer _sr;
        Transform _arrow;
        SpriteRenderer _arrowSr;
        float _scanTimer;
        Vector2Int _target;
        bool _hasTarget;

        public void Init(GameManager game, PetKind kind)
        {
            _game = game;
            _kind = kind;

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = SpriteFactory.Framed(PetColor(kind), new Color32(40, 30, 20, 255));
            _sr.sortingOrder = WorldView.PlayerSortingOrder - 1;
            transform.localScale = Vector3.one * (GameConfig.PlayerScale * 0.55f);

            if (kind == PetKind.Mole)
            {
                var arrowGo = new GameObject("MoleArrow");
                arrowGo.transform.SetParent(null, false);
                _arrow = arrowGo.transform;
                _arrowSr = arrowGo.AddComponent<SpriteRenderer>();
                _arrowSr.sprite = SpriteFactory.Diamond(new Color32(255, 200, 60, 255));
                _arrowSr.sortingOrder = WorldView.PlayerSortingOrder + 1;
                _arrow.localScale = new Vector3(0.5f, 0.28f, 1f);
                _arrowSr.enabled = false;
            }

            if (_game.Player != null)
                transform.position = _game.Player.transform.position;
        }

        static Color PetColor(PetKind kind)
        {
            switch (kind)
            {
                case PetKind.Mole: return new Color32(120, 85, 60, 255);
                case PetKind.Firefly: return new Color32(220, 240, 120, 255);
                case PetKind.Turtle: return new Color32(90, 170, 90, 255);
                default: return Color.gray;
            }
        }

        void OnDestroy()
        {
            if (_arrow != null) Destroy(_arrow.gameObject);
        }

        void Update()
        {
            if (_game == null || _game.Player == null) return;

            // 플레이어 뒤를 부드럽게 따라간다
            Vector3 want = _game.Player.transform.position +
                           new Vector3(-0.55f, 0.15f, 0f) * GameConfig.CellSize;
            transform.position = Vector3.Lerp(
                transform.position, want, 1f - Mathf.Exp(-8f * Time.deltaTime));

            if (_kind == PetKind.Mole) TickMole();
        }

        void TickMole()
        {
            if (!_game.IsRunning || _game.Player.AtSurface)
            {
                if (_arrowSr != null) _arrowSr.enabled = false;
                return;
            }

            _scanTimer -= Time.deltaTime;
            if (_scanTimer <= 0f)
            {
                _scanTimer = 0.5f;
                _hasTarget = _game.Grid.FindBestTreasure(
                    _game.Player.Cell, Mathf.RoundToInt(GameConfig.MoleScanRadius), out _target);
            }

            if (!_hasTarget)
            {
                _arrowSr.enabled = false;
                return;
            }

            Vector3 from = _game.Player.transform.position;
            Vector3 to = WorldGrid.CellToWorld(_target.x, _target.y);
            Vector3 dir = (to - from);
            if (dir.sqrMagnitude < 0.6f) { _arrowSr.enabled = false; return; }

            dir.Normalize();
            _arrowSr.enabled = true;
            _arrow.position = from + dir * (1.1f * GameConfig.CellSize);
            _arrow.rotation = Quaternion.FromToRotation(Vector3.right, dir);

            // 목표가 가까울수록 밝게 깜빡인다
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 6f);
            _arrowSr.color = new Color(1f, 0.8f, 0.25f, pulse);
        }
    }
}
