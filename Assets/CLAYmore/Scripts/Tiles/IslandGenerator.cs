using System.Collections.Generic;
using CLAYmore.ECS;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CLAYmore
{
    public class IslandGenerator : MonoBehaviour
    {
        [Header("References")]
        public Tilemap tilemap;
        public IslandTileSet tileSet;
        public Economy economy;

        [Header("Initial Size")]
        [Min(5)] public int islandWidth = 7;
        [Min(5)] public int islandHeight = 7;

        [Header("Origin")]
        public Vector3 origin = Vector3.zero;

        [Header("Expansion Cost")]
        public int initialExpansionCost = 5;
        [Min(1f)] public float expansionCostMultiplier = 1.5f;

        // Runtime state
        private Vector3Int _originCell;
        private int _width;
        private int _height;
        private int _currentExpansionCost;

        private readonly Dictionary<Vector2Int, TileData> _tiles = new();
        private readonly Dictionary<Vector3Int, TileBase> _decorCache = new();
        private Vector2Int _playerTileIndex = new(int.MinValue, int.MinValue);

        private readonly List<Vector2Int> _walkableCells  = new();
        private readonly List<Vector2Int> _freeCellsBuffer = new();

        private void Awake() => GenerateIsland();

        [ContextMenu("Generate Island")]
        public void GenerateIsland()
        {
            if (tilemap == null || tileSet == null)
            {
                Debug.LogError("IslandGenerator: tilemap or tileSet is not assigned.");
                return;
            }

            _originCell = tilemap.WorldToCell(origin);
            _width  = islandWidth;
            _height = islandHeight;
            _currentExpansionCost = initialExpansionCost;

            _tiles.Clear();
            _decorCache.Clear();
            _playerTileIndex = new(int.MinValue, int.MinValue);

            tilemap.ClearAllTiles();
            RedrawAll();
        }

        /// <summary>
        /// Attempts to move from worldPos in direction.
        /// Expands if target is water or outside — blocked if Economy cannot afford it.
        /// Returns new world position (or current position if blocked).
        /// </summary>
        public Vector3 TryMove(Vector3 worldPos, Vector2Int direction)
        {
            Vector3Int currentCell = tilemap.WorldToCell(worldPos);
            Vector3Int targetCell  = currentCell + new Vector3Int(direction.x, direction.y, 0);

            int relX = targetCell.x - _originCell.x;
            int relY = targetCell.y - _originCell.y;

            bool isOutside = relX < 0 || relX >= _width || relY < 0 || relY >= _height;
            bool isWater   = !isOutside && (relX == 0 || relX == _width - 1 || relY == 0);

            if (isWater || isOutside)
            {
                if (!TryExpand(direction))
                    return tilemap.GetCellCenterWorld(currentCell);
            }

            return tilemap.GetCellCenterWorld(targetCell);
        }

        /// <summary>Returns the world center of a random empty walkable cell with no player.
        /// If avoidPlayerNeighbours is true, also excludes the 4 orthogonal neighbours of the player.
        /// Returns false if no free cell exists.</summary>
        public bool TryGetRandomWalkableCellCenter(out Vector3 worldPos, bool avoidPlayerNeighbours = false)
        {
            _freeCellsBuffer.Clear();
            foreach (var key in _walkableCells)
            {
                if (_tiles.TryGetValue(key, out TileData tile)
                    && tile.State == CellState.Empty
                    && !tile.HasPlayer
                    && (!avoidPlayerNeighbours || !IsPlayerNeighbour(key)))
                    _freeCellsBuffer.Add(key);
            }

            if (_freeCellsBuffer.Count == 0)
            {
                worldPos = Vector3.zero;
                return false;
            }

            var chosen = _freeCellsBuffer[Random.Range(0, _freeCellsBuffer.Count)];
            worldPos = tilemap.GetCellCenterWorld(new Vector3Int(chosen.x, chosen.y, 0));
            return true;
        }

        /// <summary>Current cost to expand the island by one tile in any direction.</summary>
        public int ExpansionCost => _currentExpansionCost;

        /// <summary>
        /// Returns the world-space center and length (in world units) of the edge strip
        /// that blocks movement in the given direction.
        /// Used to position and scale the edge highlight line.
        /// </summary>
        public (Vector3 center, float length) GetEdgeLineWorld(Vector2Int direction)
        {
            Vector3Int cellA, cellB;

            if (direction.y < 0) // down — bottom water row
            {
                cellA = _originCell + new Vector3Int(0,          0, 0);
                cellB = _originCell + new Vector3Int(_width - 1, 0, 0);
            }
            else if (direction.y > 0) // up — row just above the island
            {
                cellA = _originCell + new Vector3Int(0,          _height, 0);
                cellB = _originCell + new Vector3Int(_width - 1, _height, 0);
            }
            else if (direction.x < 0) // left — left water column
            {
                cellA = _originCell + new Vector3Int(0, 0,          0);
                cellB = _originCell + new Vector3Int(0, _height - 1, 0);
            }
            else // right — right water column
            {
                cellA = _originCell + new Vector3Int(_width - 1, 0,          0);
                cellB = _originCell + new Vector3Int(_width - 1, _height - 1, 0);
            }

            Vector3 wA     = tilemap.GetCellCenterWorld(cellA);
            Vector3 wB     = tilemap.GetCellCenterWorld(cellB);
            Vector3 center = (wA + wB) * 0.5f;

            // distance between first and last cell centers + one cell to include both ends
            float cellSpan = direction.y != 0 ? tilemap.cellSize.x : tilemap.cellSize.y;
            float length   = Vector3.Distance(wA, wB) + cellSpan;

            return (center, length);
        }

        /// <summary>Returns the tilemap cell the player is currently registered on.</summary>
        public Vector3Int GetPlayerCell()
            => new Vector3Int(_playerTileIndex.x, _playerTileIndex.y, 0);

        /// <summary>Converts a world position to a tilemap cell coordinate.</summary>
        public Vector3Int GetCell(Vector3 worldPos) => tilemap.WorldToCell(worldPos);

        /// <summary>Returns the world center of a given cell.</summary>
        public Vector3 GetCellCenter(Vector3Int cell) => tilemap.GetCellCenterWorld(cell);

        // ── Tile data accessors ───────────────────────────────────────────

        public TileData GetTileFromWorldPos(Vector3 worldPos)
            => _tiles.TryGetValue(ToAbsKey(worldPos), out var t) ? t : null;

        public void SetPlayerTileFromWorldPos(Vector3 worldPos)
        {
            var newIndex = ToAbsKey(worldPos);
            if (_playerTileIndex == newIndex) return;

            if (_tiles.TryGetValue(_playerTileIndex, out var oldTile)) oldTile.HasPlayer = false;
            if (_tiles.TryGetValue(newIndex,          out var newTile)) newTile.HasPlayer = true;

            var prev = _playerTileIndex;
            _playerTileIndex = newIndex;
            World.Current?.Events.Publish(new PlayerTileChangedEvent { OldIndex = prev, NewIndex = newIndex });
        }

        // ── Cell state ───────────────────────────────────────────────────

        /// <summary>Reserves the cell for a falling pot. Returns false if already occupied or reserved.</summary>
        public bool TryReserveCell(Vector3 worldPos)
        {
            var key = ToAbsKey(worldPos);
            if (!_tiles.TryGetValue(key, out TileData tile) || tile.State != CellState.Empty)
                return false;

            var old = tile.State;
            tile.State = CellState.PotInFlight;
            World.Current?.Events.Publish(new TilePotStateChangedEvent { Index = key, OldState = old, NewState = CellState.PotInFlight });
            return true;
        }

        /// <summary>Promotes a reservation to a landed pot.</summary>
        public void MarkPotLanded(Vector3 worldPos)
        {
            var key = ToAbsKey(worldPos);
            if (!_tiles.TryGetValue(key, out TileData tile)) return;
            var old = tile.State;
            tile.State = CellState.HasPot;
            World.Current?.Events.Publish(new TilePotStateChangedEvent { Index = key, OldState = old, NewState = CellState.HasPot });
        }

        /// <summary>Marks a cell as occupied by a chest. Returns false if cell is not empty.</summary>
        public bool TryMarkChestLanded(Vector3 worldPos)
        {
            var key = ToAbsKey(worldPos);
            if (!_tiles.TryGetValue(key, out TileData tile) || tile.State != CellState.Empty)
                return false;
            tile.State = CellState.HasChest;
            return true;
        }

        /// <summary>Frees the cell when a chest is collected or removed.</summary>
        public void ClearChest(Vector3 worldPos)
        {
            var key = ToAbsKey(worldPos);
            if (!_tiles.TryGetValue(key, out TileData tile) || tile.State != CellState.HasChest) return;
            tile.State = CellState.Empty;
        }

        /// <summary>Frees the cell (pot was broken or removed).</summary>
        public void ClearCell(Vector3 worldPos)
        {
            var key = ToAbsKey(worldPos);
            if (!_tiles.TryGetValue(key, out TileData tile)) return;
            var old = tile.State;
            tile.State = CellState.Empty;
            World.Current?.Events.Publish(new TilePotStateChangedEvent { Index = key, OldState = old, NewState = CellState.Empty });
        }

        /// <summary>Returns the current state of a cell.</summary>
        public CellState GetCellState(Vector3 worldPos)
        {
            _tiles.TryGetValue(ToAbsKey(worldPos), out TileData tile);
            return tile?.State ?? CellState.Empty;
        }

        /// <summary>
        /// Returns true if moving from worldPos in direction would hit water or the island boundary.
        /// Does NOT trigger island expansion — use for dash/slide checks.
        /// </summary>
        public bool IsBlockedByEdge(Vector3 worldPos, Vector2Int direction)
        {
            Vector3Int currentCell = tilemap.WorldToCell(worldPos);
            Vector3Int targetCell  = currentCell + new Vector3Int(direction.x, direction.y, 0);

            int relX = targetCell.x - _originCell.x;
            int relY = targetCell.y - _originCell.y;

            bool isOutside = relX < 0 || relX >= _width || relY < 0 || relY >= _height;
            bool isWater   = !isOutside && (relX == 0 || relX == _width - 1 || relY == 0);

            return isWater || isOutside;
        }

        // ── Private ──────────────────────────────────────────────────────

        private bool IsPlayerNeighbour(Vector2Int key)
        {
            return key == _playerTileIndex + new Vector2Int( 1,  0)
                || key == _playerTileIndex + new Vector2Int(-1,  0)
                || key == _playerTileIndex + new Vector2Int( 0,  1)
                || key == _playerTileIndex + new Vector2Int( 0, -1);
        }

        private Vector2Int ToAbsKey(Vector3 worldPos)
        {
            var c = tilemap.WorldToCell(worldPos);
            return new Vector2Int(c.x, c.y);
        }

        private bool TryExpand(Vector2Int dir)
        {
            if (economy != null && !economy.TrySpend(_currentExpansionCost))
            {
                Debug.Log($"IslandGenerator: cannot expand — need {_currentExpansionCost} coins (have {economy.Coins}).");
                return false;
            }

            if (dir.x < 0)      { _originCell.x--; _width++; }
            else if (dir.x > 0) { _width++; }
            else if (dir.y < 0) { _originCell.y--; _height++; }
            else                { _height++; }

            _currentExpansionCost = Mathf.RoundToInt(_currentExpansionCost * expansionCostMultiplier);
            RedrawAll();
            return true;
        }

        private void RedrawAll()
        {
            int placed = 0, skipped = 0;

            for (int relY = 0; relY < _height; relY++)
            {
                for (int relX = 0; relX < _width; relX++)
                {
                    Vector3Int cell = _originCell + new Vector3Int(relX, relY, 0);
                    IslandTileType type = GetTileType(relX, relY);
                    bool isLight = (cell.x + cell.y) % 2 == 0;
                    TileBase tile;
                    if (type == IslandTileType.CenterInner)
                    {
                        if (!_decorCache.TryGetValue(cell, out tile))
                        {
                            tile = Random.value < 0.3f
                                ? tileSet.GetRandomDecorTile(isLight)
                                : tileSet.GetTile(type, isLight);
                            _decorCache[cell] = tile;
                        }
                    }
                    else
                    {
                        _decorCache.Remove(cell);
                        tile = tileSet.GetTile(type, isLight);
                    }

                    if (tile != null)
                    {
                        tilemap.SetTile(cell, tile);
                        placed++;
                    }
                    else
                    {
                        Debug.LogWarning($"IslandGenerator: no tile for {type} ({(isLight ? "light" : "dark")}) at rel({relX},{relY})");
                        skipped++;
                    }
                }
            }

            tilemap.RefreshAllTiles();
            RebuildWalkableCells();
            EagerlyPopulateTiles();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(tilemap);
#endif

            Debug.Log($"IslandGenerator: {_width}x{_height}, next expansion costs {_currentExpansionCost}. {placed} placed, {skipped} skipped.");
        }

        private void RebuildWalkableCells()
        {
            _walkableCells.Clear();
            for (int relY = 1; relY < _height; relY++)
                for (int relX = 1; relX < _width - 1; relX++)
                {
                    var abs = _originCell + new Vector3Int(relX, relY, 0);
                    _walkableCells.Add(new Vector2Int(abs.x, abs.y));
                }
        }

        private void EagerlyPopulateTiles()
        {
            foreach (var key in _walkableCells)
                if (!_tiles.ContainsKey(key))
                    _tiles[key] = new TileData(key);

            // Restore player flag after expand (existing tile object survives)
            if (_playerTileIndex.x != int.MinValue
                && _tiles.TryGetValue(_playerTileIndex, out var pt))
                pt.HasPlayer = true;
        }

        private IslandTileType GetTileType(int relX, int relY)
            => GetTileType(relX, relY, _width, _height);

        private static IslandTileType GetTileType(int relX, int relY, int width, int height)
        {
            int maxX = width - 1;
            int maxY = height - 1;

            bool isLeft   = relX == 0;
            bool isRight  = relX == maxX;
            bool isBottom = relY == 0;
            bool isTop    = relY == maxY;

            bool isInteriorLeft   = relX == 1;
            bool isInteriorRight  = relX == maxX - 1;
            bool isInteriorBottom = relY == 1;

            if (isLeft && isBottom) return IslandTileType.LeftBot;
            if (isLeft && isTop)    return IslandTileType.LeftTop;
            if (isLeft)             return IslandTileType.LeftMid;

            if (isRight && isBottom) return IslandTileType.RightBot;
            if (isRight && isTop)    return IslandTileType.RightTop;
            if (isRight)             return IslandTileType.RightMid;

            if (isBottom && isInteriorLeft)  return IslandTileType.BotLeft;
            if (isBottom && isInteriorRight) return IslandTileType.BotRight;
            if (isBottom)                    return IslandTileType.BotCenter;

            if (isTop && isInteriorLeft)  return IslandTileType.InteriorTopLeftCorner;
            if (isTop && isInteriorRight) return IslandTileType.InteriorTopRightCorner;
            if (isTop)                    return IslandTileType.InteriorTopCenter;

            if (isInteriorBottom && isInteriorLeft)  return IslandTileType.InteriorBotLeftCorner;
            if (isInteriorBottom && isInteriorRight) return IslandTileType.InteriorBotRightCorner;
            if (isInteriorBottom)                    return IslandTileType.InteriorBotCenter;

            return IslandTileType.CenterInner;
        }

        /// <summary>
        /// Returns the tiles that would be added (or changed) if the island expands in the given direction.
        /// Used to render a ghost preview without actually expanding.
        ///
        /// Up    — 1 row  (the island top has no separate water border)
        /// Down  — 2 rows (outer water border + inner cliff row)
        /// Left  — 2 cols (outer water border + inner cliff column)
        /// Right — 2 cols (outer water border + inner cliff column)
        /// </summary>
        public List<(Vector3Int cell, IslandTileType type, bool isLight)> GetExpansionGhostCells(Vector2Int dir)
        {
            var result = new List<(Vector3Int, IslandTileType, bool)>();

            int newOriginX = _originCell.x;
            int newOriginY = _originCell.y;
            int newWidth   = _width;
            int newHeight  = _height;

            if      (dir.x < 0) { newOriginX--; newWidth++; }
            else if (dir.x > 0) { newWidth++; }
            else if (dir.y < 0) { newOriginY--; newHeight++; }
            else                { newHeight++; }

            if (dir.y > 0) // UP — 1 row at the new top
            {
                int relY = newHeight - 1;
                for (int relX = 0; relX < newWidth; relX++)
                    AddGhostCell(result, newOriginX + relX, newOriginY + relY, relX, relY, newWidth, newHeight);
            }
            else if (dir.y < 0) // DOWN — 2 rows: outer water border (relY=0) + inner cliff (relY=1)
            {
                for (int relY = 0; relY <= 1; relY++)
                    for (int relX = 0; relX < newWidth; relX++)
                        AddGhostCell(result, newOriginX + relX, newOriginY + relY, relX, relY, newWidth, newHeight);
            }
            else if (dir.x < 0) // LEFT — 2 cols: outer water (relX=0) + inner cliff (relX=1)
            {
                for (int relX = 0; relX <= 1; relX++)
                    for (int relY = 0; relY < newHeight; relY++)
                        AddGhostCell(result, newOriginX + relX, newOriginY + relY, relX, relY, newWidth, newHeight);
            }
            else // RIGHT — 2 cols: inner cliff (relX=newWidth-2) + outer water (relX=newWidth-1)
            {
                for (int relX = newWidth - 2; relX <= newWidth - 1; relX++)
                    for (int relY = 0; relY < newHeight; relY++)
                        AddGhostCell(result, newOriginX + relX, newOriginY + relY, relX, relY, newWidth, newHeight);
            }

            return result;
        }

        private static void AddGhostCell(
            List<(Vector3Int, IslandTileType, bool)> list,
            int absX, int absY, int relX, int relY, int width, int height)
        {
            bool isLight = (absX + absY) % 2 == 0;
            list.Add((new Vector3Int(absX, absY, 0), GetTileType(relX, relY, width, height), isLight));
        }
    }
}
