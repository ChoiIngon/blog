using NDungeon;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dungeon dungeon;
    public Player player;

    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = (GameManager)GameObject.FindFirstObjectByType<GameManager>();
                if (null == _instance)
                {
                    GameObject container = new GameObject();
                    container.name = typeof(GameManager).Name;
                    _instance = container.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }
}