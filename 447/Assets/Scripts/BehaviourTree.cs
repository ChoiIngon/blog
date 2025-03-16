using System.Collections.Generic;

namespace BehaviourTree
{
    public enum Result
    {
        Success,
        Failure,
        Abort,
        InProgress
    }

    public class Blackboard
    {
        private Dictionary<string, object> objects = new Dictionary<string, object>();
        public Blackboard parent;

        public object Get(string name)
        {
            object data;
            if (false == objects.TryGetValue(name, out data))
            {
                if (null == parent)
                {
                    return null;
                }

                return parent.Get(name);
            }

            return data;
        }

        public void Set(string name, object data)
        {
            if (true == objects.ContainsKey(name))
            {
                objects[name] = data;
                return;
            }

            objects.Add(name, data);
        }

        public void Clear()
        {
            objects.Clear();
        }
    }

    public class Node
    {
        public readonly string name;
        public Node parent;
        public Blackboard blackboard = new Blackboard();

        public Node(string name)
        {
            this.name = name;
        }

        public virtual Result Update()
        {
            return Result.Success;
        }

        public virtual void Abort()
        {
        }

        public virtual Node FindChild(string name)
        {
            return null;
        }
    }

    public class Root : Node
    {
        private Node child;

        public Root(string name) : base(name)
        {
            this.child = null;
        }

        public void AddChild(Node child)
        {
            child.parent = this;
            child.blackboard.parent = this.blackboard;

            this.child = child;
        }

        public override Node FindChild(string name)
        {
            if (null == child)
            {
                return null;
            }

            int index = name.IndexOf('/');
            string targetName = -1 != index ? name.Substring(0, index) : name;

            if (targetName != child.name)
            {
                return null;
            }

            if (-1 != index)
            {
                string nextPath = name.Substring(index + 1);
                if ("" != nextPath)
                {
                    return child.FindChild(nextPath);
                }
            }
            return child;
        }

        public override void Abort()
        {
            base.Abort();

            if (null == child)
            {
                return;
            }

            child.Abort();
        }

        public override Result Update()
        {
            if (null == child)
            {
                return Result.Failure;
            }

            var ret = child.Update();
            if (Result.InProgress != ret)
            {
                Abort();
            }
            return ret;
        }
    }

    public class Composite : Node
    {
        public Composite(string name) : base(name)
        {
        }

        public List<Node> children = new List<Node>();

        public void AddChild(Node child)
        {
            child.parent = this;
            child.blackboard.parent = this.blackboard;

            children.Add(child);
        }

        public override Node FindChild(string name)
        {
            if (0 == children.Count)
            {
                return null;
            }

            int index = name.IndexOf('/');
            string targetName = -1 != index ? name.Substring(0, index) : name;

            foreach (Node child in children)
            {
                if (targetName != child.name)
                {
                    continue;
                }

                if (-1 != index)
                {
                    string nextPath = name.Substring(index + 1);
                    if ("" != nextPath)
                    {
                        return child.FindChild(nextPath);
                    }
                }

                return child;
            }

            return null;
        }

        public override void Abort()
        {
            base.Abort();
            foreach (Node child in children)
            {
                child.Abort();
            }
        }
    }

    public class Sequence : Composite
    {
        public Sequence(string name) : base(name)
        {
        }

        public int currentIndex;

        public override Result Update()
        {
            for (; currentIndex < children.Count; currentIndex++)
            {
                Node child = children[currentIndex];

                Result result = child.Update();

                if (Result.InProgress == result)
                {
                    return Result.InProgress;
                }

                if (Result.Failure == result)
                {
                    currentIndex = 0;
                    return Result.Failure;
                }
            }

            currentIndex = 0;
            return Result.Success;
        }

        public override void Abort()
        {
            base.Abort();
            currentIndex = 0;
        }
    }

    public class Selector : Composite
    {
        public Selector(string name) : base(name)
        {
        }

        public int currentIndex;

        public override Result Update()
        {
            for (; currentIndex < children.Count; currentIndex++)
            {
                Node child = children[currentIndex];
                Result result = child.Update();
                if (Result.InProgress == result)
                {
                    return Result.InProgress;
                }

                if (Result.Success == result)
                {
                    currentIndex = 0;
                    return Result.Success;
                }
            }

            currentIndex = 0;
            return Result.Failure;
        }

        public override void Abort()
        {
            base.Abort();
            currentIndex = 0;
        }
    }
}