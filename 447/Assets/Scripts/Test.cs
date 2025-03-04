using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    List<Room> rooms = new List<Room>();

    
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateRoomGizmo(Room room)
    {
        DungeonGizmo.Block block = new DungeonGizmo.Block($"Block_{room.index}", Color.blue, room.rect.width, room.rect.height);
        block.position = new Vector3(room.x, room.y, 0.0f);
    }

}
