using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>파낸 통로(걸을 수 있는 칸) 위에서만 이동하는 BFS 경로 탐색.</summary>
    public static class Pathfinder
    {
        static readonly Vector2Int[] Dirs =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        /// <summary>
        /// start 에서 goal 까지의 경로. 없으면 null.
        /// start / goal 자체는 걸을 수 없어도 허용한다(광산 칸 등).
        /// </summary>
        public static List<Vector2Int> Find(WorldGrid grid, Vector2Int start, Vector2Int goal)
        {
            if (!grid.InBounds(start.x, start.y) || !grid.InBounds(goal.x, goal.y)) return null;
            if (start == goal) return new List<Vector2Int> { goal };

            int w = grid.Width, h = grid.Height;
            var prev = new int[w * h];
            for (int i = 0; i < prev.Length; i++) prev[i] = -1;

            int startIdx = start.y * w + start.x;
            int goalIdx = goal.y * w + goal.x;
            prev[startIdx] = startIdx;

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    var nx = cur + Dirs[d];
                    if (!grid.InBounds(nx.x, nx.y)) continue;

                    int ni = nx.y * w + nx.x;
                    if (prev[ni] != -1) continue;

                    bool passable = grid.IsWalkable(nx.x, nx.y) || ni == goalIdx;
                    if (!passable) continue;

                    prev[ni] = cur.y * w + cur.x;
                    if (ni == goalIdx) return Rebuild(prev, w, startIdx, goalIdx);
                    queue.Enqueue(nx);
                }
            }
            return null;
        }

        static List<Vector2Int> Rebuild(int[] prev, int w, int startIdx, int goalIdx)
        {
            var path = new List<Vector2Int>();
            int cur = goalIdx;
            int guard = 0;
            while (cur != startIdx && guard++ < 100000)
            {
                path.Add(new Vector2Int(cur % w, cur / w));
                cur = prev[cur];
            }
            path.Add(new Vector2Int(startIdx % w, startIdx / w));
            path.Reverse();
            return path;
        }
    }
}
