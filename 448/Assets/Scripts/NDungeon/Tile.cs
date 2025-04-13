using UnityEngine;

namespace NDungeon
{
    public class Tile : MonoBehaviour
    {
        public const float TileSize = 4.0f; // Tile size in Unity units
        public int index
        {
            get => data.index;
        }

        public NTileMap.TileMap.Tile.Type type
        {
            get => data.type;
        }

        public Tile[] neighbors = new Tile[(int)NDungeon.NTileMap.TileMap.Tile.Direction.Max];
        public NTileMap.TileMap.Tile data;
    }
}