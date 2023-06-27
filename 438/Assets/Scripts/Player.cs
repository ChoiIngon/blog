using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int x;
    public int y;
    public int radius;

    public void Move(int toX, int toY)
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

        GameManager.Instance.map.InitSight(this.x, this.y, radius + 1);

        SetPosition(toX, toY);
    }

    public void SetPosition(int x, int y)
    {
        Tile tile = GameManager.Instance.map.GetTile(x, y);
        if (null == tile)
        {
            return;
        }

        this.x = x;
        this.y = y;

        GameManager.Instance.map.CastLight(this.x, this.y, radius + 1);
        transform.position = tile.transform.position;
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }
}
