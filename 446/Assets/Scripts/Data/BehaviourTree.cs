using System.Collections.Generic;

namespace Data
{
    public class BehaviourTree
    {
        public class Blackboard
        {
            private Blackboard parent;
            private Dictionary<string, object> data = new Dictionary<string, object>();

            public object Get(string name)
            {
                if (true == data.ContainsKey(name))
                {
                    return data[name];
                }

                if (null != parent)
                {
                    return parent.Get(name);
                }

                return null;
            }
        }

        public enum Result
        {
            Success,
            Failure,
            Abort,
            InProgress
        }

        public class Node
        {
            public string name;

            public Node(string name)
            {
                this.name = name;
            }

            public virtual Result Run()
            {
                return Result.Success;
            }
        }

        // 컴포짓 노드가 실행 되는 조건 지정
        public class Decorator : Node
        {
            public Node child;
            public Decorator(string name) : base(name) { }

            public void AddChild(Node child)
            {
                this.child = child;
            }
        }

        public class Composite : Node
        {
            public Composite(string name) : base(name) { }

            public List<Node> children = new List<Node>();
            public List<Decorator> decorators = new List<Decorator>();

            public void AddChild(Node child)
            {
                this.children.Add(child);
            }

            public void AddDecorator(Decorator decorator)
            {
                this.decorators.Add(decorator);
            }

            public Node GetChild(int index)
            {
                if (index >= children.Count)
                {
                    return null;
                }
                return children[index];
            }

            public List<Node> GetChildren()
            {
                return children;
            }

            public override Result Run()
            {
                foreach (Decorator decorator in decorators)
                {
                    if (Result.Failure == decorator.Run())
                    {
                        return Result.Failure;
                    }
                }

                return Result.Success;
            }
        }

        public class Selector : Composite
        {
            public Selector(string name) : base(name) { }

            public override Result Run()
            {
                if (Result.Failure == base.Run())
                {
                    return Result.Failure;
                }

                foreach (Node child in GetChildren())
                {
                    if (Result.Success == child.Run())
                    {
                        return Result.Success;
                    }

                    if (Result.InProgress == child.Run())
                    {
                        return Result.InProgress;
                    }
                }
                return Result.Failure;
            }
        }

        public class Sequence : Composite
        {
            public Sequence(string name) : base(name) { }

            public override Result Run()
            {
                if (Result.Failure == base.Run())
                {
                    return Result.Failure;
                }

                foreach (Node child in GetChildren())
                {
                    if (Result.Failure == child.Run())
                    {
                        return Result.Failure;
                    }
                }
                return Result.Success;
            }
        }

        public Node FindChild(string path)
        {
            return null;
        }

        public Node root;

        public BehaviourTree()
        {
        }
    }
}