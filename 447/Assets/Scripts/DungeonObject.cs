using UnityEngine;

public class DungeonObject
{
    public GameObject gameObject;

    public DungeonObject()
    {
        gameObject = new GameObject();
    }
}

public class Door : DungeonObject
{ 
}

public class Item : DungeonObject
{
}