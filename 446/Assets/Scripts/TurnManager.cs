using System.Collections.Generic;

public class TurnManager
{
    public class Action
    {
        public Actor actor { get; protected set; }

        public Action(Actor actor)
        {
            this.actor = actor;
        }

        public virtual void Update()
        {
        }
    }

    public class Move : Action
    {
        public int x { get; private set; } = 0;
        public int y { get; private set; } = 0;

        public Move(Actor actor, int x, int y) : base(actor)
        {
            this.x = x;
            this.y = y;
        }

        public override void Update()
        {
            actor.Move(x, y);
        }
    }

    public class Attack : Action
    {
        public int x { get; private set; } = 0;
        public int y { get; private set; } = 0;

        public Attack(Actor actor, int x, int y) : base(actor)
        {
            this.x = x;
            this.y = y;
        }

        public override void Update()
        {
            Dungeon dungeon = GameManager.Instance.dungeon;
            var tile = dungeon.GetTile(x, y);
            var target = tile.actor;
            if (null == target)
            {
                return;
            }

            if (null == target.occupyTile)
            {
                return;
            }

            actor.Attack(target);
        }
    }

    public List<Action> actions = new List<Action>();
    public Action current = null;

    public void Update()
    {
        if (null != current)
        {
            if (Actor.Action.Idle == current.actor.action)
            {
                current = null;
            }
            else
            {
                return;
            }
        }

        if (0 == actions.Count)
        {
            return;
        }

        current = actions[0];
        actions.RemoveAt(0);

        if (null == current.actor.occupyTile)
        {
            return;
        }

        current.Update();

        GameManager.Instance.dungeon.player.FieldOfView();
    }
}