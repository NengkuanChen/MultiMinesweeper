using System.Collections.Generic;
using Configs;
using DefaultNamespace;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

namespace Cell
{
    
    //only exists on the server to avoid cheating
    public class ServerCellManager
    { 
        public List<List<bool>> IsMine { get; private set; }
        
        public List<List<int>> AdjacentMines { get; private set; } // -1 if mine
        
        public Vector2Int GridSize { get; private set; }
        
        public float MineDensity { get; private set; }
        
        public List<List<MineCell.MineCellState>> CellStates { get; private set; }

        public ServerCellManager()
        {
            GridSize = Config.GameConfig.DefaultGridSize;
            MineDensity = Config.GameConfig.MineDensity;
            IsMine = new List<List<bool>>();
            for (int i = 0; i < GridSize.x; i++)
            {
                IsMine.Add(new List<bool>());
                for (int j = 0; j < GridSize.y; j++)
                {
                    IsMine[i].Add(Random.value < MineDensity);
                }
            }
            
            AdjacentMines = new List<List<int>>();
            for (int i = 0; i < GridSize.x; i++)
            {
                AdjacentMines.Add(new List<int>());
                for (int j = 0; j < GridSize.y; j++)
                {
                    AdjacentMines[i].Add(IsMine[i][j] ? -1 : 0);
                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {
                            if (k == 0 && l == 0)
                            {
                                continue;
                            }
                            
                            if (i + k >= 0 && i + k < GridSize.x && j + l >= 0 && j + l < GridSize.y)
                            {
                                if (IsMine[i + k][j + l])
                                {
                                    AdjacentMines[i][j]++;
                                }
                            }
                        }
                    }
                }
            }
            
            CellStates = new List<List<MineCell.MineCellState>>();
            for (int i = 0; i < GridSize.x; i++)
            {
                CellStates.Add(new List<MineCell.MineCellState>());
                for (int j = 0; j < GridSize.y; j++)
                {
                    var cellState = new MineCell.MineCellState();
                    cellState.State = MineCell.MineCellState.CellState.Hidden;
                    cellState.HasOwner = false;
                    cellState.AdjacentMines = -2;
                    CellStates[i].Add(new MineCell.MineCellState());
                }
            }
            
            
        }

        public void RevealCellRecursive(Vector2Int position, ref List<Vector2Int> revealedCellPositions, ulong clientId, 
            ref List<MineCell.MineCellState> result)
        {
            if (position.x < 0 || position.x >= GridSize.x || position.y < 0 || position.y >= GridSize.y)
            {
                return;
            }
            if (CellStates[position.x][position.y].State != MineCell.MineCellState.CellState.Hidden)
            {
                return;
            }
            if (CellStates[position.x][position.y].HasOwner)
            {
                return;
            }
            revealedCellPositions.Add(position);
            var cellState = CellStates[position.x][position.y];
            if (IsMine[position.x][position.y])
            {
                cellState.State = MineCell.MineCellState.CellState.Exploded;
                cellState.HasOwner = true;
                cellState.OwnerClientId = clientId;
                CellStates[position.x][position.y] = cellState;
                result.Add(cellState);
                //subtract score from player
                ServerGameManager.PlayerScores[clientId].SubtractScore(1);
                return;
            }
            cellState.State = MineCell.MineCellState.CellState.Revealed;
            cellState.HasOwner = true;
            cellState.OwnerClientId = clientId;
            cellState.AdjacentMines = AdjacentMines[position.x][position.y];
            CellStates[position.x][position.y] = cellState;
            result.Add(cellState);
            if (cellState.AdjacentMines == 0)
            {
                RevealCellRecursive(new Vector2Int(position.x - 1, position.y - 1), ref revealedCellPositions, clientId, ref result);
                RevealCellRecursive(new Vector2Int(position.x - 1, position.y), ref revealedCellPositions, clientId, ref result);
                RevealCellRecursive(new Vector2Int(position.x - 1, position.y + 1), ref revealedCellPositions, clientId, ref result);
                RevealCellRecursive(new Vector2Int(position.x, position.y - 1), ref revealedCellPositions, clientId, ref result);
                RevealCellRecursive(new Vector2Int(position.x, position.y + 1), ref revealedCellPositions, clientId, ref result);
                RevealCellRecursive(new Vector2Int(position.x + 1, position.y - 1), ref revealedCellPositions, clientId, ref result);
                RevealCellRecursive(new Vector2Int(position.x + 1, position.y), ref revealedCellPositions, clientId, ref result);
                RevealCellRecursive(new Vector2Int(position.x + 1, position.y + 1), ref revealedCellPositions, clientId, ref result);
            }
        }

        public void FlagCell(Vector2Int position, ulong clientId)
        {
            Debug.Log($"Player {clientId} flagged cell {position}");
            if (CellStates[position.x][position.y].HasOwner)
            {
                return;
            }
            if (IsMine[position.x][position.y])
            {
                var mineCellState = CellStates[position.x][position.y];
                mineCellState.State = MineCell.MineCellState.CellState.Flagged;
                mineCellState.HasOwner = true;
                mineCellState.OwnerClientId = clientId;
                CellStates[position.x][position.y] = mineCellState;
                ServerGameManager.PlayerScores[clientId].AddScore(1);
            }
            else
            {
                var mineCellState = CellStates[position.x][position.y];
                mineCellState.State = MineCell.MineCellState.CellState.WrongFlag;
                mineCellState.HasOwner = true;
                mineCellState.OwnerClientId = clientId;
                mineCellState.AdjacentMines = AdjacentMines[position.x][position.y];
                CellStates[position.x][position.y] = mineCellState;
                ServerGameManager.PlayerScores[clientId].SubtractScore(1);
            }
            SyncCellStates(position);
        }

        public void SyncCellStates(Vector2Int cellPosition)
        {
            var cellState = CellStates[cellPosition.x][cellPosition.y];
            foreach (var player in ServerGameManager.Players)
            {
                player.Value.SyncCellStatesRpc(cellPosition, cellState);
            }
        }
        
        public void RevealCells(Vector2Int[] cellPositions, ulong clientId)
        {
            var revealedCellPositions = new List<Vector2Int>();
            var result = new List<MineCell.MineCellState>();
            
            foreach (var cellPosition in cellPositions)
            {
                RevealCellRecursive(cellPosition, ref revealedCellPositions, clientId, ref result);
            }
            
            SyncCellStates(revealedCellPositions, result);
        }
        
        public void RevealCell(Vector2Int cellPosition, ulong clientId)
        {
            var result = new List<MineCell.MineCellState>();
            var revealedCellPositions = new List<Vector2Int>();
            RevealCellRecursive(cellPosition, ref revealedCellPositions, clientId, ref result);
            SyncCellStates(revealedCellPositions, result);
        }
        
        
        public void SyncCellStates(List<Vector2Int> cellPositions, List<MineCell.MineCellState> cellStates)
        {
            foreach (var player in ServerGameManager.Players)
            {
                player.Value.SyncCellStatesRpc(cellPositions.ToArray(), cellStates.ToArray());
            }
        }
        
    }
}