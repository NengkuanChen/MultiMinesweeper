using System.Threading.Tasks;
using UnityEngine;

namespace Configs
{
    public static class Config 
    {
        public static GameConfig GameConfig { get; private set; }

        public static async Task LoadConfigAsync()
        {
            var gameConfigLoadTask = Resources.LoadAsync<GameConfig>("GameConfig");
            while (!gameConfigLoadTask.isDone)
            {
                await Task.Yield();
            }
            GameConfig = (GameConfig) gameConfigLoadTask.asset;
        }
    }
}