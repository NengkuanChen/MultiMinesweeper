using System.Collections.Generic;
using Cell;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] 
		private Vector2Int defaultGridSize = new Vector2Int(10, 10);
        public Vector2Int DefaultGridSize => defaultGridSize;

        [SerializeField]
	    private float mineDensity = 0.1f;
        public float MineDensity => mineDensity;
        
        [SerializeField]
        private List<Sprite> numberSprites = new List<Sprite>();
        
        public Sprite GetNumberSprite(int number)
        {
            return numberSprites[number];
        }
        
        [SerializeField]
        private Sprite mineSprite;
        
        public Sprite MineSprite => mineSprite;
        
        [SerializeField]
        private Sprite flagSprite;
        
        public Sprite FlagSprite => flagSprite;
        
        [SerializeField]
        private float sensitivity = 5f;
        
        public float Sensitivity => sensitivity;

        [SerializeField]
        private List<Color> playerColors = new List<Color>();
        
        public Color GetPlayerColor(int index)
        {
            if (index >= playerColors.Count)
            {
                var randomColor = Random.ColorHSV();
                randomColor.a = 1;
                return randomColor;
            }
            return playerColors[index];
        }
        
        [SerializeField]
        private MineCell cellPrefab;
        
        public MineCell CellPrefab => cellPrefab;
        
        [SerializeField]
        private NetworkObject playerScorePrefab;
        
        public NetworkObject PlayerScorePrefab => playerScorePrefab;
        
        [SerializeField]
        private float zoomSpeed = 1f;
        
        public float ZoomSpeed => zoomSpeed;

    }
}