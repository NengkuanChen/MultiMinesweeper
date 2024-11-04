using UnityEngine;
using Utils;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [SerializeField]
        private GameObject mainMenu;
        public GameObject MainMenu => mainMenu;
        
        [SerializeField]
        private GameObject controlsPanel;
        public GameObject ControlsPanel => controlsPanel;
        
        
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EventUtility.Subscribe(typeof(OnGameStartedEventArgs), OnGameStarted);
        }

        private void OnGameStarted(object sender, GameEventArgs args)
        {
            mainMenu.SetActive(false);
            controlsPanel.SetActive(true);
        }
        
        private void OnDestroy()
        {
            EventUtility.Unsubscribe(typeof(OnGameStartedEventArgs), OnGameStarted);
        }
    }
}