using Data;
using UnityEngine;

public class Monster : Actor
{
    public int monsterNo = 0;
    public float actionPoint = 0;
    public BehaviourTree.Root behaviour = null;
    
    private void Start()
    {
        this.health.max = 5;
        this.health.value = this.health.max;
        this.agility = 1;
        this.sight = GameManager.Instance.maxRoomSize;

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Dungeon.SortingOrder.Actor;

        this.behaviour = new BehaviourTree.Root("Root");
        behaviour.AddChild(new BehaviourTree.Sequence("Sequence"));

        var sequence = behaviour.FindChild("Sequence") as BehaviourTree.Sequence;
        
        if (0 == Random.Range(0, 100) % 2)
        {
            sequence.AddChild(new Search("Search"));// 시야에서 보이지 않으면 포기
            spriteRenderer.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);   // 파란색 몬스터

            this.agility = 2;
        }
        else
        {
            sequence.AddChild(new Chase("Chase")); // 시야에서 사라져도 끝까지 추적
            spriteRenderer.color = new Color(1.0f, 0.0f, 0.0f, 1.0f); // 빨간색 몬스터

            this.agility = 1;
        }
        sequence.AddChild(new Approch("Approch")); // 플레이어에게 접근
        sequence.AddChild(new Attack_1("Attack"));

        this.skin = GameManager.Instance.resources.GetSkin("Player");
        this.direction = Direction.Down;
        this.SetAction(Action.Idle);
        this.Visible(false);

        Move((int)transform.position.x, (int)transform.position.y);
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

            var path = dungeon.FindPath(from, to);
            if (2 < path.tiles.Count)
            {
                Data.Tile tile = path.tiles[1];
                GameManager.Instance.dungeon.turnManager.actions.Add(new TurnManager.Move(self, (int)tile.rect.x, (int)tile.rect.y));
            }

            blackboard.parent.Set("Path", path);
            return BehaviourTree.Result.Success;
        }
    }

    public class Attack_1 : BehaviourTree.Node
    {
        public Attack_1(string name) : base(name) {}

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

            var path = blackboard.Get("Path") as AStarPathFinder;
            if (null == path)
            {
                return BehaviourTree.Result.Failure;
            }

            if (1 < path.tiles.Count - 2)
            {
                return BehaviourTree.Result.Failure;
            }

            GameManager.Instance.dungeon.turnManager.actions.Add(new TurnManager.Attack(self, (int)target.transform.position.x, (int)target.transform.position.y));
            
            return BehaviourTree.Result.Success;
        }
    }
}
