using BehaviourTree;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public sealed class MonsterManager
{
    private int         MonsterNoAllocator = 1;
    private Transform   parent;

    public Dictionary<int, Monster> monsters = new Dictionary<int, Monster>();

    public MonsterManager(Transform parent)
    {
        this.parent = parent;
    }

    public Monster Create(int index, Vector3 position)
    {
        GameObject go = new GameObject($"Monster_{index}_{MonsterNoAllocator}");
        go.transform.parent = this.parent;
        go.transform.position = position;
        Monster monster = go.AddComponent<Monster>();
        monster.monsterNo = MonsterNoAllocator++;
        monsters.Add(monster.monsterNo, monster);
        return monster;
    }

    public void Remove(Monster monster)
    {
        monsters.Remove(monster.monsterNo);
        monster.Destroy();
    }

    public void Clear()
    {
        while (0 < monsters.Count)
        {
            var pair = monsters.First();
            var monster = pair.Value;
            Remove(monster);
        }
    }

    public void Update()
    {
        var player = GameManager.Instance.dungeon.player;

        foreach (var pair in monsters)
        {
            Monster monster = pair.Value;
            monster.actionPoint += (float)monster.agility / (float)player.agility;
            if (1.0f > monster.actionPoint)
            {
                continue;
            }

            monster.behaviour.blackboard.Set("Self", monster);
            monster.behaviour.Update();

            monster.actionPoint -= 1.0f;
        }
    }

}
