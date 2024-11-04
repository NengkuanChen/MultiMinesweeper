using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField]
        private Button startButton;
        
        [SerializeField]
        private Image playerColorImage;


        private void Awake()
        {
            startButton.onClick.AddListener(() =>
            {
                EventUtility.TriggerNow(this, new OnStartButtonClickedEventArgs());
            });
            EventUtility.Subscribe(typeof(OnPlayerColorChangedEventArgs), OnPlayerColorChanged);
            EventUtility.Subscribe(typeof(OnGameStartedEventArgs), OnGameStarted);
        }

        private void OnGameStarted(object sender, GameEventArgs args)
        {
            gameObject.SetActive(false);
        }

        private void OnPlayerColorChanged(object sender, GameEventArgs args)
        {
            var colorArgs = (OnPlayerColorChangedEventArgs) args;
            playerColorImage.color = colorArgs.Color;
        }

        private void OnDestroy()
        {
            EventUtility.Unsubscribe(typeof(OnPlayerColorChangedEventArgs), OnPlayerColorChanged);
            EventUtility.Unsubscribe(typeof(OnGameStartedEventArgs), OnGameStarted);
        }
    }
}