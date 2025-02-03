using UnityEngine;

public class Monster : MonoBehaviour
{
    public static int MonsterNoAllocator = 1;
    public SpriteRenderer spriteRenderer;
    public float sightRange = 10;
    public ActorAnimation actorAnimation;
    public Data.BehaviourTree behaviourTree;
    public Data.Actor actor;
    public int monsterNo;

    private void Start()
    {
        actorAnimation = gameObject.AddComponent<ActorAnimation>();
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder = Dungeon.SortingOrder.Actor;
        spriteRenderer.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        actorAnimation.skin = GameManager.Instance.resources.GetSkin("Player");
        actorAnimation.direction = ActorAnimation.Direction.Down;
        actorAnimation.Play(ActorAnimation.Action.Idle);

        behaviourTree = new Data.BehaviourTree();

        Data.BehaviourTree.Sequence search = new Data.BehaviourTree.Sequence("Search");
        search.AddDecorator(new FindPlayer("FindPlayer"));

        behaviourTree.root = search;
        /*
        actor.sightRange = 5;
        
        

        
        search.AddChild(new Idle("Idle"));
        search.AddChild(new SetTarget("SetTarget"));
        search.AddChild(new Move("Move"));

        
        */
    }

    private void Update()
    {
        //if (null != behaviourTree.root)
        //{
        //    behaviourTree.root.Run();
        //}
    }
    
    public class FindPlayer : Data.BehaviourTree.Decorator
    {
        public FindPlayer(string name) : base(name) { }

        public override Data.BehaviourTree.Result Run()
        {
            return Data.BehaviourTree.Result.Success;
        }
    }

    public class Idle : Data.BehaviourTree.Node
    {
        public Idle(string name) : base(name) { }

        public override Data.BehaviourTree.Result Run()
        {
            return Data.BehaviourTree.Result.Success;
        }
    }

    public class SetTarget : Data.BehaviourTree.Node
    {
        public SetTarget(string name) : base(name) { }
        public Data.BehaviourTree.Blackboard blackboard = new Data.BehaviourTree.Blackboard();

        public override Data.BehaviourTree.Result Run()
        {
            var self = blackboard.Get("Self") as Monster;
            if (null == self)
            {
                return Data.BehaviourTree.Result.Failure;
            }

            var player = GameManager.Instance.dungeon.player;
            if (null == player)
            {
                return Data.BehaviourTree.Result.Failure;
            }

            if (self.sightRange >= Vector3.Distance(self.transform.position, player.transform.position))
            {
                
            }

            return Data.BehaviourTree.Result.Success;
        }
    }

    public class Move : Data.BehaviourTree.Node
    {
        public Move(string name) : base(name) { }

        public override Data.BehaviourTree.Result Run()
        {
            return Data.BehaviourTree.Result.Success;
        }
    }
}
