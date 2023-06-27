using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private const int mapWidth = 41;
    private const int mapHeight = 41;
    private const int playerSight = 30;
    private const int playerX = 20;
    private const int playerY = 20;

    public Block blockPrefab;
    public Tile tilePrefab;

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

        // 사전 설정 블록
        Vector2[] blockPositions = {
            new Vector2(11, 35), new Vector2(12, 35), new Vector2(13, 35),
            new Vector2(11, 34), new Vector2(12, 34), new Vector2(13, 34),
            new Vector2(11, 33), new Vector2(12, 33), new Vector2(13, 33), new Vector2(16, 33), new Vector2(19, 33), new Vector2(20, 33),
            new Vector2(11, 32), new Vector2(12, 32)
        };

        foreach (Vector2 blockPosition in blockPositions)
        {
            Tile tile = map.GetTile((int)blockPosition.x, (int)blockPosition.y);
            if (null == tile)
            {
                continue;
            }

            Block block = CreateBlock();
            tile.SetBlock(block);
        }

        player.radius = playerSight;
        player.SetPosition(playerX, playerY);
    }

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    public Block CreateBlock()
    {
        Block block = Instantiate<Block>(GameManager.Instance.blockPrefab, transform.position, Quaternion.identity);
        block.Init();
        return block;
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
	}

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
                    Block block = tile.block;
                    block.transform.SetParent(null);
                    GameObject.Destroy(block.gameObject);
                    tile.block = null;
                }
                else
                {
                    Block block = CreateBlock();
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
