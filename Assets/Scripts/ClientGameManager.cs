using System.Collections.Generic;
using Cell;

namespace DefaultNamespace
{
    public static class ClientGameManager
    {
        public static ClientCellManager CellManager { get; private set; }
        
        public static bool Launched { get; private set; }
        
        public static Dictionary<ulong, Player.Player> Players = new Dictionary<ulong, Player.Player>();
        
        
        public static void LaunchClient()
        {
            CellManager = new ClientCellManager();
            CellManager.SpawnCells();
            Launched = true;
        }
        
        
        
        public static void ShutdownClient()
        {
            CellManager.DestroyCells();
            CellManager = null;
            Launched = false;
        }
    }
}