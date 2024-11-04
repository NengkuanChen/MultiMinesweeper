using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField]
        private VerticalLayoutGroup scoreList;
        
        [SerializeField]
        private PlayerScoreInfoUI playerScoreInfoPrefab;
        
        [SerializeField]
        private List<PlayerScoreInfoUI> playerScoreInfos = new List<PlayerScoreInfoUI>();

        private void Awake()
        {
            EventUtility.Subscribe(typeof(OnPlayerScoreSpawnedEventArgs), OnPlayerScoreSpawned);
            EventUtility.Subscribe(typeof(OnPlayerScoreChangedEventArgs), OnPlayerScoreChanged);
        }

        private void OnPlayerScoreChanged(object sender, GameEventArgs args)
        {
            var arg = (OnPlayerScoreChangedEventArgs) args;
            var rank = 0;
            foreach (var playerScoreInfo in playerScoreInfos)
            {
                if (playerScoreInfo.ClientId == arg.ClientId)
                {
                    continue;
                }
                if (playerScoreInfo.Score > arg.Score)
                {
                    rank++;
                }
            }
            var scoreInfoEntity = playerScoreInfos.Find(info => info.ClientId == arg.ClientId);
            scoreInfoEntity.transform.SetSiblingIndex(rank);
        }
        
        private void OnPlayerScoreSpawned(object sender, GameEventArgs args)
        {
            var scoreEntity = sender as Player.PlayerScore;
            var playerScoreInfo = Instantiate(playerScoreInfoPrefab, scoreList.transform);
            playerScoreInfos.Add(playerScoreInfo);
            playerScoreInfo.Initialize(scoreEntity.ClientId.Value, scoreEntity.PlayerColor.Value, this);
            
        }
        
        
        private void OnDestroy()
        {
            EventUtility.Unsubscribe(typeof(OnPlayerScoreSpawnedEventArgs), OnPlayerScoreSpawned);
        }
        
        public void RemovePlayerScoreInfo(ulong clientId)
        {
            var playerScoreInfo = playerScoreInfos.Find(info => info.ClientId == clientId);
            playerScoreInfos.Remove(playerScoreInfo);
            // Destroy(playerScoreInfo.gameObject);
        }
        
        
    }
}