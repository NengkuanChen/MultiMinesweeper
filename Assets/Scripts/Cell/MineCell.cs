using System;
using Configs;
using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace Cell
{
    
    public class MineCell : MonoBehaviour, IPointerClickHandler, IDragHandler, 
        IScrollHandler, IPointerEnterHandler, IPointerExitHandler
    {
        
        // public bool IsMine { get; private set; }
        
        public Vector2Int Position { get; private set; }
        
        [SerializeField]
        private SpriteRenderer maskSpriteRenderer;
        
        [SerializeField]
        private SpriteRenderer cellSpriteRenderer;
        
        public MineCellState CellState { get; private set; }
        
        [SerializeField]
        private GameObject wrongMark;
        
        [SerializeField]
        private GameObject highlight;
        
        
        [Serializable]
        public struct MineCellState : INetworkSerializable, IEquatable<MineCellState>
        {
            public ulong OwnerClientId;
            public CellState State;
            public bool HasOwner;
            public enum CellState
            {
                Hidden,
                Revealed,
                Flagged,
                Exploded,
                WrongFlag
            }
            public int AdjacentMines; // -1 if mine, -2 if not revealed
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref OwnerClientId);
                serializer.SerializeValue(ref State);
                serializer.SerializeValue(ref HasOwner);
                serializer.SerializeValue(ref AdjacentMines);
            }

            public bool Equals(MineCellState other)
            {
                return OwnerClientId == other.OwnerClientId && State == other.State && HasOwner == other.HasOwner &&
                       AdjacentMines == other.AdjacentMines;
            }
        }

        private void Awake()
        {
            EventUtility.Subscribe(typeof(OnCellStateChangedEventArgs), OnCellStateChanged);
            EventUtility.Subscribe(typeof(OnMultipleCellsStateChangedEventArgs), OnMultipleCellsStateChanged);
        }

        private void OnMultipleCellsStateChanged(object sender, GameEventArgs args)
        {
            var arg = (OnMultipleCellsStateChangedEventArgs) args;
            
        }

        private void OnCellStateChanged(object sender, GameEventArgs args)
        {
            var arg = (OnCellStateChangedEventArgs) args;
            if (arg.Position != Position)
            {
                return;
            }
            CellState = arg.CellState;
            UpdateSprites();
        }

        public void UpdateSprites()
        {
            if (CellState.HasOwner)
            {
                var ownerPlayer = ClientGameManager.Players[CellState.OwnerClientId];
                var ownerColor = ownerPlayer.PlayerColor.Value;
                ownerColor.a = 0.35f;
                maskSpriteRenderer.color = ownerColor;
                maskSpriteRenderer.enabled = true;
            }


            switch (CellState.State)
            {
                case MineCellState.CellState.Revealed:
                    cellSpriteRenderer.sprite = Config.GameConfig.GetNumberSprite(CellState.AdjacentMines);
                    break;
                case MineCellState.CellState.Flagged:
                    cellSpriteRenderer.sprite = Config.GameConfig.FlagSprite;
                    break;
                case MineCellState.CellState.Exploded:
                    cellSpriteRenderer.sprite = Config.GameConfig.MineSprite;
                    break;
                case MineCellState.CellState.WrongFlag:
                    cellSpriteRenderer.sprite = Config.GameConfig.GetNumberSprite(CellState.AdjacentMines);
                    wrongMark.SetActive(true);
                    break;
            }
        }


        public void SetPosition(Vector2Int position)
        {
            Position = position;
            transform.position = new Vector3(position.x, position.y, 0);
        }

        private void OnDestroy()
        {
            EventUtility.Unsubscribe(typeof(OnCellStateChangedEventArgs), OnCellStateChanged);
            EventUtility.Unsubscribe(typeof(OnMultipleCellsStateChangedEventArgs), OnMultipleCellsStateChanged);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var canReveal = ClientGameManager.CellManager.CanRevealCell(Position, out var quickRevealList);
                if (canReveal)
                {
                    var localClientId = NetworkManager.Singleton.LocalClientId;
                    var player = ClientGameManager.Players[localClientId];
                    if (quickRevealList.Count > 0)
                    {
                        player.RevealCellServerRpc(quickRevealList.ToArray(), localClientId);
                    }
                    else
                    {
                        player.RevealCellServerRpc(Position, localClientId);
                    }
                    ClientGameManager.CellManager.UnhighlightAdjacentCells(Position);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                var localClientId = NetworkManager.Singleton.LocalClientId;
                var player = ClientGameManager.Players[localClientId];
                player.FlagCellServerRpc(Position, localClientId);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            EventUtility.TriggerNow(this,
                new OnPlayerDraggedEventArgs(eventData.delta * Config.GameConfig.Sensitivity));
        }

        public void OnScroll(PointerEventData eventData)
        {
            EventUtility.TriggerNow(this,
                new OnPlayerScrolledEventArgs(eventData.scrollDelta.y));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CellState.State == MineCellState.CellState.Revealed  || CellState.State == MineCellState.CellState.Exploded)
            {
                ClientGameManager.CellManager.HighlightAdjacentCells(Position);
            }
        }
        
        
        
        public void Highlight()
        {
            highlight.SetActive(true);
        }
        
        public void Unhighlight()
        {
            highlight.SetActive(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ClientGameManager.CellManager.UnhighlightAdjacentCells(Position);
        }
    }
}