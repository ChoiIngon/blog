using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public int mapWidth;
    public int mapHeight;

    public int sightRange;

    public GameObject blockPrefab;
    public GameObject tilePrefab;

    public Map map;
    public Player player;

    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        map.Init(mapWidth, mapHeight);

        Tile tile = map.GetTile(mapWidth / 2, mapHeight / 2);

        player.radius = sightRange;
        player.SetPosition(tile.x, tile.y);
    }

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject CreateBlock()
    {
        return Instantiate(GameManager.Instance.blockPrefab, transform.position, Quaternion.identity);
    }

    /*
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
	}
    */

    private void Update()
    {
        if (null == instance)
        {
            return;
        }

        if (true == Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPosition, transform.forward, 30.0f);

            if (true == hit && hit.transform.gameObject.tag == "Tile")
            {
                Tile tile = hit.transform.GetComponent<Tile>();

                map.InitSight(player.x, player.y, player.radius + 1);

                if (null != tile.block)
                {
                    GameObject block = tile.block;
                    block.transform.SetParent(null);
                    GameObject.Destroy(block);
                    tile.block = null;
                }
                else
                {
                    GameObject block = CreateBlock();
                    block.transform.position = tile.transform.position;
                    tile.block = block;
                    block.transform.SetParent(tile.transform);
                }

                map.CastLight(player.x, player.y, player.radius + 1);
            }
        }

        if (true == Input.GetKeyDown(KeyCode.UpArrow))
        {
            player.Move(player.x, player.y + 1);
        }

        if (true == Input.GetKeyDown(KeyCode.DownArrow))
        {
            player.Move(player.x, player.y - 1);
        }

        if (true == Input.GetKeyDown(KeyCode.LeftArrow))
        {
            player.Move(player.x - 1, player.y);
        }

        if (true == Input.GetKeyDown(KeyCode.RightArrow))
        {
            player.Move(player.x + 1, player.y);
        }
    }
}
