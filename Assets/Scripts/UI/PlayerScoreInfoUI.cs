using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class PlayerScoreInfoUI : MonoBehaviour
    {
        [SerializeField]
        private Image playerColorImage;
        
        [SerializeField]
        private TextMeshProUGUI playerScoreText;
        
        private ulong clientId;
        
        public ulong ClientId => clientId;
        
        public int Score { get; private set; }
        
        private ScoreUI scoreUI;


        public void Initialize(ulong clientId, Color playerColor, ScoreUI scoreUI)
        {
            this.clientId = clientId;
            playerColorImage.color = playerColor;
            this.scoreUI = scoreUI;
            EventUtility.Subscribe(typeof(OnPlayerScoreChangedEventArgs), OnPlayerScoreChanged);
            EventUtility.Subscribe(typeof(OnPlayerScoreDespawnedEventArgs), OnPlayerScoreDespawned);
        }

        private void OnPlayerScoreDespawned(object sender, GameEventArgs args)
        {
            var eventArgs = (OnPlayerScoreDespawnedEventArgs) args;
            var playerScoreEntity = sender as PlayerScore;
            scoreUI.RemovePlayerScoreInfo(clientId);
            if (playerScoreEntity.ClientId.Value == clientId)
            {
                Destroy(gameObject);
            }
            
        }

        private void OnPlayerScoreChanged(object sender, GameEventArgs args)
        {
            var eventArgs = (OnPlayerScoreChangedEventArgs) args;
            if (eventArgs.ClientId == clientId)
            {
                Score = eventArgs.Score;
                playerScoreText.text = eventArgs.Score.ToString();
            }
        }
    }
}