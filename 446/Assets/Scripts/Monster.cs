using Data;
using UnityEngine;

public class Monster : Actor
{
    public static int MonsterNoAllocator = 1;

    public int monsterNo = 0;
    public float actionGauge = 0;
    public BehaviourTree.Root behaviour = null;
    public BehaviourTree.Node search = null;
    public BehaviourTree.Node approch = null;
    public BehaviourTree.Node attack = null;

    private void Start()
    {
        this.agility = 1;
        this.sight = GameManager.Instance.maxRoomSize;

        gameObject.name = $"Monster_{monsterNo}";
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Dungeon.SortingOrder.Actor;

        this.search = null;
        if (0 == Random.Range(0, 100) % 2)
        {
            this.search = new Search("Search"); // �þ߿��� ������ ������ ����
            spriteRenderer.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);   // �Ķ��� ����
        }
        else
        {
            this.search = new Chase("Chase"); // �þ߿��� ������� ������ ����
            this.spriteRenderer.color = new Color(1.0f, 0.0f, 0.0f, 1.0f); // ������ ����
        }
        this.approch = new Approch("Approch"); // �÷��̾�� ����
        this.attack = new Attack("Attack");

        this.skin = GameManager.Instance.resources.GetSkin("Player");
        this.direction = Direction.Down;
        this.SetAction(Action.Idle);
        this.Visible(false);

        Move((int)transform.position.x, (int)transform.position.y);

        this.behaviour = new BehaviourTree.Root("Root");
        behaviour.AddChild(new BehaviourTree.Sequence("Sequence"));

        var sequence = behaviour.FindChild("Sequence") as BehaviourTree.Sequence;
        sequence.AddChild(this.search);
        sequence.AddChild(this.approch);
        sequence.AddChild(this.attack);

        Data.MonsterManager.Instance.monsters.Add(monsterNo, this);
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

            self.SetAction(Action.Idle);

            var player = GameManager.Instance.dungeon.player;
            if (null == player)
            {
                return BehaviourTree.Result.Failure;
            }

            if (self.sight < Vector3.Distance(self.transform.position, player.transform.position))
            {
                return BehaviourTree.Result.Failure;
            }

            var dungeon = GameManager.Instance.dungeon.data;
            var to = dungeon.GetTile((int)player.transform.position.x, (int)player.transform.position.y);

            Data.ShadowCast sight = dungeon.CastLight((int)self.transform.position.x, (int)self.transform.position.y, self.sight);
            foreach (var tile in sight.tiles)
            {
                if (tile == to)
                {
                    blackboard.parent.Set("Target", player);
                    self.actionGauge += (float)self.agility / (float)player.agility;
                    return BehaviourTree.Result.Success;
                }
            }
            
            return BehaviourTree.Result.Failure;
        }
    }

    public class Chase : BehaviourTree.Node
    {
        public Chase(string name) : base(name) { }

        public override BehaviourTree.Result Update()
        {
            var self = blackboard.Get("Self") as Monster;
            if (null == self)
            {
                return BehaviourTree.Result.Failure;
            }

            self.SetAction(Action.Idle);

            var player = blackboard.Get("Target") as Player;
            if (null != player)
            {
                self.actionGauge += (float)self.agility / (float)player.agility;
                return BehaviourTree.Result.Success;
            }

            player = GameManager.Instance.dungeon.player;
            if (null == player)
            {
                return BehaviourTree.Result.Failure;
            }

            if (self.sight < Vector3.Distance(self.transform.position, player.transform.position))
            {
                return BehaviourTree.Result.Failure;
            }

            var dungeon = GameManager.Instance.dungeon.data;
            var to = dungeon.GetTile((int)player.transform.position.x, (int)player.transform.position.y);

            Data.ShadowCast sight = dungeon.CastLight((int)self.transform.position.x, (int)self.transform.position.y, self.sight);
            foreach (var tile in sight.tiles)
            {
                if (tile == to)
                {
                    blackboard.parent.Set("Target", player);
                    self.actionGauge += (float)self.agility / (float)player.agility;
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

            var dungeon = GameManager.Instance.dungeon.data;
            var from = dungeon.GetTile((int)self.transform.position.x, (int)self.transform.position.y);
            var to = dungeon.GetTile((int)target.transform.position.x, (int)target.transform.position.y);

            AStarPathFinder path = dungeon.FindPath(from, to);
            if (null == path)
            {
                return BehaviourTree.Result.Failure;
            }

            int index = 1;
            while(1.0f <= self.actionGauge)
            {
                if (index + 1 > path.tiles.Count)
                {
                    break;
                }

                Tile tile = path.tiles[index];
                self.Move((int)tile.rect.x, (int)tile.rect.y);
                self.actionGauge -= 1.0f;
                index++;
            }

            return BehaviourTree.Result.Success;
        }
    }

    public class Attack : BehaviourTree.Node
    {
        public Attack(string name) : base(name) {}

        public override BehaviourTree.Result Update()
        {
            var self = blackboard.Get("Self") as Monster;
            if (null == self)
            {
                return BehaviourTree.Result.Failure;
            }

            return BehaviourTree.Result.Success;
        }
    }
}
