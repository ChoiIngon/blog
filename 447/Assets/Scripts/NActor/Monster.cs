using System.Collections.Generic;
using UnityEngine;

public class Monster : Actor
{
    private static int MonsterNoAllocator = 0;
    public int monsterNo { get; private set; }
    public float actionPoint = 0;
    public BehaviourTree.Root behaviourTreeRoot = null;

    public static Monster Create(TileMap tileMap, Vector3 position)
    {
        if (null == tileMap.startTile)
        {
            return null;
        }

        int monsterNo = ++MonsterNoAllocator;

        Actor.Meta meta = new Actor.Meta();
        meta.name = $"Monster_{monsterNo}";
        meta.skin = GameManager.Instance.Resources.GetSkin("Actor");
        meta.health = 5;
        meta.agility = Random.Range(3, 12);
        meta.sight = 5;

        var behaviourTreeRoot = new BehaviourTree.Root("Root");
        behaviourTreeRoot.AddChild(new BehaviourTree.Sequence("Sequence"));
        var sequence = behaviourTreeRoot.FindChild("Sequence") as BehaviourTree.Sequence;
        sequence.AddChild(new Search("Search"));
        sequence.AddChild(new Approch("Approch"));
        sequence.AddChild(new MeleeAttack("MeleeAttack"));

        var monster = Actor.Create<Monster>(meta, tileMap, position);
        monster.monsterNo = monsterNo;
        monster.behaviourTreeRoot = behaviourTreeRoot;
        monster.spriteRenderer.color = Color.red;
        monster.Visible(false);
        
        return monster;
    }

    public override void Move(int x, int y)
    {
        base.Move(x, y);
        if(null != tile)
        {
            Visible(tile.visible);
        }
    }

    public override void Attack(Actor target)
    {
        base.Attack(target);

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NActor.Attack(this, target, target.health, 0));
    }


    public class Search : BehaviourTree.Node
    {
        public Search(string name) : base(name) { }

        public override BehaviourTree.Result Update()
        {
            var self = blackboard.Get("Self") as Monster;
            if (null == self)
            {
                return BehaviourTree.Result.Failure;
            }

            var player = blackboard.Get("Player") as Player;
            if (null == player)
            {
                return BehaviourTree.Result.Failure;
            }

            if (self.meta.sight < Vector3.Distance(self.position, player.position))
            {
                return BehaviourTree.Result.Failure;
            }

            var to = self.tileMap.GetTile((int)player.position.x, (int)player.position.y);
            var fov = self.tileMap.CastLight((int)self.position.x, (int)self.position.y, self.meta.sight);
            foreach (var tile in fov.tiles)
            {
                if (tile == to)
                {
                    blackboard.parent.Set("Target", player);
                    return BehaviourTree.Result.Success;
                }
            }

            return BehaviourTree.Result.Failure;
        }
    }

    public class Approch : BehaviourTree.Node
    {
        public Approch(string name) : base(name) { }

        public override BehaviourTree.Result Update()
        {
            var self = blackboard.Get("Self") as Monster;
            if (null == self)
            {
                return BehaviourTree.Result.Failure;
            }

            var target = blackboard.Get("Target") as Player;
            if (null == target)
            {
                return BehaviourTree.Result.Failure;
            }
  
            var from = self.tileMap.GetTile((int)self.position.x, (int)self.position.y);
            var to = self.tileMap.GetTile((int)target.position.x, (int)target.position.y);

            var path = self.tileMap.FindPath(from, to);
            path.RemoveAt(0);
            path.RemoveAt(path.Count - 1);
            while (1 <= path.Count && 1.0f <= self.actionPoint)
            {
                Tile tile = path[0];
                self.Move((int)tile.rect.x, (int)tile.rect.y);
                path.RemoveAt(0);

                self.actionPoint -= 1.0f;
            }

            blackboard.parent.Set("Path", path);
            return BehaviourTree.Result.Success;
        }
    }

    public class MeleeAttack : BehaviourTree.Node
    {
        public MeleeAttack(string name) : base(name) { }

        public override BehaviourTree.Result Update()
        {
            var self = blackboard.Get("Self") as Monster;
            if (null == self)
            {
                return BehaviourTree.Result.Failure;
            }

            var target = blackboard.Get("Target") as Player;
            if (null == target)
            {
                return BehaviourTree.Result.Failure;
            }

            var path = blackboard.Get("Path") as List<Tile>;
            if (null == path)
            {
                return BehaviourTree.Result.Failure;
            }

            if (1 < path.Count)
            {
                return BehaviourTree.Result.Failure;
            }

            while (1.0f <= self.actionPoint)
            {
                self.Attack(target);
                self.actionPoint -= 1.0f;
            }

            return BehaviourTree.Result.Success;
        }
    }

    public class MonsterManager
    {
        private Dictionary<int, Monster> monsters = new Dictionary<int, Monster>();

        public void Add(Monster monster)
        {
            monsters.Add(monster.monsterNo, monster);
        }

        public void Update(Player player)
        {
            foreach (var pair in monsters)
            {
                Monster monster = pair.Value;
                float addActionPoint = (float)monster.meta.agility / (float)player.meta.agility;
                monster.actionPoint += Random.Range(addActionPoint/2, addActionPoint * 1.5f);
                monster.behaviourTreeRoot.blackboard.Set("Self", monster);
                monster.behaviourTreeRoot.blackboard.Set("Player", player);

                while(1.0f <= monster.actionPoint)
                {
                    monster.behaviourTreeRoot.Update();
                    monster.actionPoint = Mathf.Max(0.0f, monster.actionPoint - 1.0f);
                }
            }
        }

        public void Remove(Monster monster)
        {
            monsters.Remove(monster.monsterNo);
        }
    }
}