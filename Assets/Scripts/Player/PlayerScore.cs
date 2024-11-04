using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerScore : NetworkBehaviour
    {
        public NetworkVariable<int> Score { get; private set; } = new NetworkVariable<int>(0);
        
        
        
        public NetworkVariable<ulong> ClientId { get; private set; } = new NetworkVariable<ulong>(0);
        
        public NetworkVariable<Color> PlayerColor { get; private set; } = new NetworkVariable<Color>(Color.white);
        // public NetworkVariable<Vector4> PlayerColor { get; private set; } = new NetworkVariable<Vector4>(Color.white);
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient)
            {
                Score.OnValueChanged += (oldValue, newValue) =>
                {
                    EventUtility.TriggerNow(this, new OnPlayerScoreChangedEventArgs(newValue, ClientId.Value));
                };
                // EventUtility.TriggerNow(this, new OnPlayerScoreChangedEventArgs(0, ClientId));
                PlayerColor.OnValueChanged += OnPlayerColorOnValueChanged;
                
            }
        }

        private async void OnPlayerColorOnValueChanged(Color oldValue, Color newValue)
        {
            await Task.Delay(200);
            EventUtility.TriggerNow(this, new OnPlayerScoreSpawnedEventArgs());
        }

        public void SetClientId(ulong clientId, Color playerColor)
        {
            ClientId.Value = clientId;
            PlayerColor.Value = playerColor;
            // ScoreEntitySpawnedRpc();
        }
        
        public void AddScore(int score)
        {
            Score.Value += score;
        }
        
        public void SubtractScore(int score)
        {
            Score.Value -= score;
        }
        
        public void SetScore(int score)
        {
            Score.Value = score;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            EventUtility.TriggerNow(this, new OnPlayerScoreDespawnedEventArgs());
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void ScoreEntitySpawnedRpc()
        {
            EventUtility.TriggerNow(this, new OnPlayerScoreSpawnedEventArgs());
            EventUtility.TriggerNow(this, new OnPlayerScoreChangedEventArgs(0, ClientId.Value));
        }
    }
}