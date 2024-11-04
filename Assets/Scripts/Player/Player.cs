using System;
using System.Collections.Generic;
using Cell;
using Configs;
using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace Player
{
    public class Player : NetworkBehaviour
    {

        private Camera mainCamera;
        
        public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.white);
        
        public NetworkVariable<Dictionary<ulong, int>> PlayerScores = new NetworkVariable<Dictionary<ulong, int>>(new Dictionary<ulong, int>());
        
        
        public void SetPlayerColor(Color color)
        {
            PlayerColor.Value = color;
        }

        

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                ServerGameManager.Players.Add(OwnerClientId, this);
                ServerGameManager.LaunchServer();
            }

            if (IsClient || IsHost)
            {
                ClientGameManager.Players.Add(OwnerClientId, this);
            }
            if (IsOwner)
            {
                EventUtility.Subscribe(typeof(OnPlayerClickedEventArgs), OnPlayerClicked);
                EventUtility.Subscribe(typeof(OnPlayerDraggedEventArgs), OnPlayerDragged);
                EventUtility.Subscribe(typeof(OnPlayerRightClickedEventArgs), OnPlayerRightClicked);
                EventUtility.Subscribe(typeof(OnPlayerScrolledEventArgs), OnPlayerScrolled);
                PlayerColor.OnValueChanged = (oldValue, newValue) =>
                {
                    EventUtility.TriggerNow(this, new OnPlayerColorChangedEventArgs(newValue));
                };
                ClientGameManager.LaunchClient();
                // Debug.Log(OwnerClientId);
                RequestPlayerColorRpc(OwnerClientId);
                mainCamera = Camera.main;
            }
        }

        private void OnPlayerScrolled(object sender, GameEventArgs args)
        {
            var scrollArgs = (OnPlayerScrolledEventArgs) args;
            var orthoSize = mainCamera.orthographicSize - scrollArgs.ScrollDelta * Config.GameConfig.ZoomSpeed;
            orthoSize = Mathf.Clamp(orthoSize, 1, 10);
            mainCamera.orthographicSize = orthoSize;
        }

        private void OnPlayerRightClicked(object sender, GameEventArgs args)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, 1<<8);
            if (hit.collider)
            {
                var cell = hit.collider.GetComponent<MineCell>();
                if (cell)
                {
                    Debug.Log(cell.CellState.State);
                    if (cell.CellState.State == MineCell.MineCellState.CellState.Hidden)
                    {
                        FlagCellServerRpc(cell.Position, OwnerClientId);
                    }
                }
            }
        }

        private void OnPlayerDragged(object sender, GameEventArgs args)
        {
            var dragArgs = (OnPlayerDraggedEventArgs) args;
            var dragDelta = dragArgs.DragDelta;
            mainCamera.transform.position += new Vector3(-dragDelta.x * Config.GameConfig.Sensitivity,
                -dragDelta.y * Config.GameConfig.Sensitivity, 0);
        }

        private void OnPlayerClicked(object sender, GameEventArgs args)
        {
            //screen to 2d ray
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, 1<<8);
            if (hit.collider)
            {
                var cell = hit.collider.GetComponent<MineCell>();
                if (cell)
                {
                    // var canReveal = ClientGameManager.CellManager.CanRevealCell(cell.Position, out var quickRevealList);
                    // if (canReveal)
                    // {
                    //     if (quickRevealList.Count > 0)
                    //     {
                    //         RevealCellServerRpc(quickRevealList.ToArray(), OwnerClientId);
                    //     }
                    //     else
                    //     {
                    //         RevealCellServerRpc(cell.Position, OwnerClientId);
                    //     }
                    // }
                }
            }
        }
        
        
        

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsServer)
            {
                ServerGameManager.ShutdownServer();
            }
            if (IsOwner)
            {
                ClientGameManager.ShutdownClient();
                EventUtility.Unsubscribe(typeof(OnPlayerClickedEventArgs), OnPlayerClicked);
                EventUtility.Unsubscribe(typeof(OnPlayerDraggedEventArgs), OnPlayerDragged);
                EventUtility.Unsubscribe(typeof(OnPlayerRightClickedEventArgs), OnPlayerRightClicked);
            }
        }
        
        [Rpc(SendTo.Server)]
        public void RequestPlayerColorRpc(ulong clientId)
        {
            var color = ServerGameManager.RequestPlayerColor(clientId);
            SetPlayerColor(color);
        }
        
        
        
        [Rpc(SendTo.ClientsAndHost)]
        public void GameStartedRpc()
        {
            EventUtility.TriggerNow(this, new OnGameStartedEventArgs());
        }
        
        [Rpc(SendTo.Server)]
        public void RevealCellServerRpc(Vector2Int position, ulong clientId)
        {
            ServerGameManager.ServerCellManager.RevealCell(position, clientId);
        }
        
        [Rpc(SendTo.Server)]
        public void RevealCellServerRpc(Vector2Int[] positions, ulong clientId)
        {
            ServerGameManager.ServerCellManager.RevealCells(positions, clientId);
        }
        
        [Rpc(SendTo.Server)]
        public void FlagCellServerRpc(Vector2Int position, ulong clientId)
        {
            ServerGameManager.ServerCellManager.FlagCell(position, clientId);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void SyncCellStatesRpc(Vector2Int position, MineCell.MineCellState cellState)
        {
            EventUtility.TriggerNow(this, new OnCellStateChangedEventArgs(position, cellState));
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void SyncCellStatesRpc(Vector2Int[] cellPositions,
            MineCell.MineCellState[] cellStates)
        {
            // EventUtility.TriggerNow(this, new OnMultipleCellsStateChangedEventArgs(cellPositions, cellStates));
            for (int i = 0; i < cellPositions.Length; i++)
            {
                EventUtility.TriggerNow(this, new OnCellStateChangedEventArgs(cellPositions[i], cellStates[i]));
            }
        }
        
    }
}