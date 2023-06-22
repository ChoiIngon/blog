using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public int x;
    public int y;
    public int radius;

    // Update is called once per frame
    void Update()
    {
        if (true == Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(x, y + 1);
        }

        if (true == Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(x, y - 1);
        }

        if (true == Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(x - 1, y);
        }

        if (true == Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(x + 1, y);
        }
    }

    private void Move(int toX, int toY)
    {
        Tile toTile = GameManager.Instance.map.GetTile(toX, toY);
        if (null == toTile)
        {
            return;
        }

        if (null != toTile.block)
        {
            return;
        }

        Tile fromTile = GameManager.Instance.map.GetTile(this.x, this.y);
        GameManager.Instance.map.InitSight(this.x, this.y, radius + 1);
        GameManager.Instance.ClearSlopeLines();

        fromTile.block = null;
        toTile.block = this.gameObject;

        this.x = toX;
        this.y = toY;

        GameManager.Instance.map.CastLight(this.x, this.y, radius + 1);
        transform.position = toTile.transform.position;
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }
}
