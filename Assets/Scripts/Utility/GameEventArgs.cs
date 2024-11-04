using Cell;
using Unity.Netcode;
using UnityEngine;

namespace Utils
{
    public class GameEventArgs
    {
        
    }
    
    public class OnPlayerClickedEventArgs : GameEventArgs
    {
        
    }
    
    
    public class OnPlayerDraggedEventArgs : GameEventArgs
    {
        public Vector3 DragDelta { get; }
        
        public OnPlayerDraggedEventArgs(Vector3 dragDelta)
        {
            DragDelta = dragDelta;
        }
    }
    
    public class OnPlayerScrolledEventArgs : GameEventArgs
    {
        public float ScrollDelta { get; }
        
        public OnPlayerScrolledEventArgs(float scrollDelta)
        {
            ScrollDelta = scrollDelta;
        }
    }
    
    public class OnStartButtonClickedEventArgs : GameEventArgs
    {
        
    }
    
    public class OnPlayerColorChangedEventArgs : GameEventArgs
    {
        public Color Color { get; }
        
        public OnPlayerColorChangedEventArgs(Color color)
        {
            Color = color;
        }
    }
    
    public class OnGameStartedEventArgs : GameEventArgs
    {
        
    }
    
    public class OnPlayerRightClickedEventArgs : GameEventArgs
    {
    }
    
    public class OnPlayerScoreChangedEventArgs : GameEventArgs
    {
        public int Score { get; }
        public ulong ClientId { get; }
        
        public OnPlayerScoreChangedEventArgs(int score, ulong clientId)
        {
            Score = score;
            ClientId = clientId;
        }
    }

    public class OnPlayerScoreSpawnedEventArgs : GameEventArgs
    {
  
    }
    
    public class OnPlayerScoreDespawnedEventArgs : GameEventArgs
    {

    }
    
    public class OnCellStateChangedEventArgs : GameEventArgs
    {
        public MineCell.MineCellState CellState { get; }
        public Vector2Int Position { get; }
        
        public OnCellStateChangedEventArgs(Vector2Int position, MineCell.MineCellState cellState)
        {
            Position = position;
            CellState = cellState;
        }
    }
    
    public class OnMultipleCellsStateChangedEventArgs : GameEventArgs
    {
        public Vector2Int[] CellPositions { get; }
        public MineCell.MineCellState[] CellStates { get; }
        
        public OnMultipleCellsStateChangedEventArgs(Vector2Int[] cellPositions, MineCell.MineCellState[] cellStates)
        {
            CellPositions = cellPositions;
            CellStates = cellStates;
        }
    }
}