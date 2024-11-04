using System.Collections.Generic;
using Cell;
using Configs;
using Player;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace DefaultNamespace
{
    public static class ServerGameManager
    {
        public static Dictionary<ulong, Color> PlayerColors = new Dictionary<ulong, Color>();
        
        public static ServerCellManager ServerCellManager { get; private set; }
        
        public static Dictionary<ulong, PlayerScore> PlayerScores = new Dictionary<ulong, PlayerScore>();
        
        public static Dictionary<ulong, Player.Player> Players = new Dictionary<ulong, Player.Player>();
        
        public static bool Launched { get; private set; }
        
        
        public static void LaunchServer()
        {
            //start server
            ServerCellManager = new ServerCellManager();
            Launched = true;
            EventUtility.Subscribe(typeof(OnStartButtonClickedEventArgs), OnStartButtonClicked);
        }
        
        public static void ShutdownServer()
        {
            ServerCellManager = null;
            Launched = false;
            EventUtility.Unsubscribe(typeof(OnStartButtonClickedEventArgs), OnStartButtonClicked);
        }

        private static void OnStartButtonClicked(object sender, GameEventArgs args)
        {
            GameStartedRpc();
        }
        
        public static Color RequestPlayerColor(ulong clientId)
        {
            var color = Config.GameConfig.GetPlayerColor(PlayerColors.Count);
            PlayerColors.Add(clientId, color);
            return color;
        }
        
        // [Rpc(SendTo.ClientsAndHost)]
        public static void GameStartedRpc()
        {
            // EventUtility.TriggerNow(null, new OnGameStartedEventArgs());
            foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(player.ClientId);
                playerObject.GetComponent<Player.Player>().GameStartedRpc();
                var playerScore = NetworkManager.Singleton.SpawnManager
                    .InstantiateAndSpawn(Config.GameConfig.PlayerScorePrefab).GetComponent<PlayerScore>();
                PlayerScores.Add(player.ClientId, playerScore);
                playerScore.SetClientId(player.ClientId, PlayerColors[player.ClientId]);
                playerScore.SetScore(0);
            }
        }
        
        
    }
}