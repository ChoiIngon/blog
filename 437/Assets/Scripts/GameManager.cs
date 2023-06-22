using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public GameObject blockPrefab;
    public GameObject tilePrefab;

    public Map map;
    public Player player;

    public Button nextButton;

    public Dictionary<string, Sprite> tileSprites = new Dictionary<string, Sprite>();

    public List<GameObject> slopeLines = new List<GameObject>();

    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        Sprite[] tileSprites = Resources.LoadAll<Sprite>("Sprites/Tile");
        this.tileSprites.Clear();
        foreach (Sprite s in tileSprites)
        {
            this.tileSprites[s.name] = s;
        }

        map.Init();

        Tile tile = map.GetTile(map.width / 2, map.height / 2);

        player.x = tile.x;
        player.y = tile.y;
        player.transform.position = tile.transform.position;

        Camera.main.transform.position = new Vector3(player.x, player.y, Camera.main.transform.position.z);

        Tile blockTile = map.GetTile(GameManager.Instance.player.x-1, GameManager.Instance.player.y + 2);
        blockTile.CreateBlock();

        map.CastLight(GameManager.Instance.player.x, GameManager.Instance.player.y, GameManager.Instance.player.radius + 1);
    }

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    public void CreateSlopeLine(Color color, Vector3 start, Vector3 end)
    {
        LineRenderer line = new GameObject("line").AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("UI/Default"));
        line.positionCount = 2;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.startColor = color;
        line.endColor = color;
        line.useWorldSpace = true;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        slopeLines.Add(line.gameObject);
	}

    public void ClearSlopeLines()
    {
        foreach (GameObject obj in slopeLines)
        {
            GameObject.Destroy(obj);
        }
        slopeLines.Clear();
    }
}
