using System.Collections.Generic;
using Configs;
using Unity.Netcode;
using UnityEngine;

namespace Cell
{
    public class ClientCellManager
    {
        public Vector2Int GridSize { get; private set; } = Config.GameConfig.DefaultGridSize;

        public float MineDensity { get; private set; } = Config.GameConfig.MineDensity;

        public List<List<MineCell>> Cells { get; private set; }
        


        public void SpawnCells()
        {
            Cells = new List<List<MineCell>>();
            for (int i = 0; i < GridSize.x; i++)
            {
                Cells.Add(new List<MineCell>());
                for (int j = 0; j < GridSize.y; j++)
                {
                    var cell = Object.Instantiate(Config.GameConfig.CellPrefab);
                    cell.transform.position = new Vector3(i, j, 0);
                    Cells[i].Add(cell);
                    cell.SetPosition(new Vector2Int(i, j));
                }
            }
        }
        
        public bool CanRevealCell(Vector2Int position, out List<Vector2Int> quickRevealList)
        {
            quickRevealList = new List<Vector2Int>();
            var cell = Cells[position.x][position.y];
            if (cell.CellState.State == MineCell.MineCellState.CellState.Hidden)
            {
                return true;
            }

            if (cell.CellState.State == MineCell.MineCellState.CellState.Revealed)
            {
                if (cell.CellState.HasOwner && cell.CellState.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                {
                    // Don't allow quick revealing of other player's cells
                    return false;
                }

                var adjacentHiddenCells = cell.CellState.AdjacentMines;
                if (adjacentHiddenCells == 0)
                {
                    return false;
                }

                // Check if all adjacent mines are flagged
                var flaggedMines = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            continue;
                        }
                        if (position.x + i >= 0 && position.x + i < GridSize.x && position.y + j >= 0 && position.y + j < GridSize.y)
                        {
                            var adjacentCell = Cells[position.x + i][position.y + j];
                            if (adjacentCell.CellState.State == MineCell.MineCellState.CellState.Flagged)
                            {
                                flaggedMines++;
                            }
                            else if (adjacentCell.CellState.State == MineCell.MineCellState.CellState.Hidden)
                            {
                                quickRevealList.Add(new Vector2Int(position.x + i, position.y + j));
                            }
                        }
                    }
                }
                
                return flaggedMines == adjacentHiddenCells;
            }

            return false;
        }

        public void HighlightAdjacentCells(Vector2Int position)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    if (position.x + i >= 0 && position.x + i < GridSize.x && position.y + j >= 0 && position.y + j < GridSize.y)
                    {
                        var cell = Cells[position.x + i][position.y + j];
                        if (cell.CellState.State == MineCell.MineCellState.CellState.Hidden)
                        {
                            cell.Highlight();
                        }
                    }
                }
            }
        }
        
        public void UnhighlightAdjacentCells(Vector2Int position)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    if (position.x + i >= 0 && position.x + i < GridSize.x && position.y + j >= 0 && position.y + j < GridSize.y)
                    {
                        var cell = Cells[position.x + i][position.y + j];
                        cell.Unhighlight();
                    }
                }
            }
        }
        
        
        public void DestroyCells()
        {
            foreach (var row in Cells)
            {
                foreach (var cell in row)
                {
                    Object.Destroy(cell.gameObject);
                }
            }
        }
    }
}