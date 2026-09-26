using UnityEngine;

namespace DigGame
{
    /// <summary>플레이어를 항상 따라다니는 카메라.</summary>
    public class CameraFollow : MonoBehaviour
    {
        Transform _target;
        WorldGrid _grid;
        Camera _cam;

        public void Setup(Transform target, WorldGrid grid)
        {
            _target = target;
            _grid = grid;
            _cam = GetComponent<Camera>();

            if (_target != null)
                transform.position = new Vector3(_target.position.x, _target.position.y, -10f);
        }

        void LateUpdate()
        {
            if (_target == null || _cam == null) return;

            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            Vector3 p = _target.position;

            float worldWidth = _grid.Width * GameConfig.CellSize;
            float worldHeight = _grid.Height * GameConfig.CellSize;
            float minX = halfW, maxX = worldWidth - halfW;
            float x = maxX > minX ? Mathf.Clamp(p.x, minX, maxX) : worldWidth * 0.5f;

            float minY = -worldHeight + halfH;
            float y = Mathf.Max(p.y, minY);

            Vector3 want = new Vector3(x, y, -10f);

            if ((transform.position - want).sqrMagnitude >
                GameConfig.CameraSnapDistance * GameConfig.CameraSnapDistance)
            {
                transform.position = want;
                return;
            }

            transform.position = Vector3.Lerp(
                transform.position, want, 1f - Mathf.Exp(-GameConfig.CameraLerp * Time.deltaTime));
        }
    }
}
