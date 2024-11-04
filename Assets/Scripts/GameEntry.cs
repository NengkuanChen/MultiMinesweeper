using System;
using Configs;
using UnityEngine;

namespace DefaultNamespace
{
    
    
    public class GameEntry : MonoBehaviour
    {
        [SerializeField]
        private Camera mainCamera;
        
        public Camera MainCamera => mainCamera;
        
        
        public static GameEntry Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAsync();
        }

        public async void InitializeAsync()
        {
            await Config.LoadConfigAsync();
        }
        
    }
}