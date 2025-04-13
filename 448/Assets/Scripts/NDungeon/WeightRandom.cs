using System.Collections.Generic;

namespace NDungeon
{
    public class WeightRandom<T>
    {
        private class Element
        {
            public T value;

            public int weight = 0;
            public int min = 0;
            public int max = 0;

            public Element left = null;
            public Element right = null;
        }

        private int total_weight = 0;
        private Element root = null;
        private List<Element> elements = new List<Element>();

        public WeightRandom()
        {
            this.total_weight = 0;
            this.root = null;
            this.elements.Clear();
        }

        public WeightRandom(List<KeyValuePair<int, T>> elements)
        {
            this.total_weight = 0;
            this.root = null;
            this.elements.Clear();
            foreach (var element in elements)
            {
                int weight = element.Key;
                T value = element.Value;
                AddElement(weight, value);
            }
        }

        public void AddElement(int weight, T value)
        {
            if (0 == weight)
            {
                return;
            }

            Element elmt = new Element();
            elmt.value = value;
            elmt.weight = weight;

            elements.Add(elmt);

            root = null;
        }

        public T Random()
        {
            if (null == root)
            {
                BuildTree();
            }

            int weight = UnityEngine.Random.Range(1, total_weight + 1);
            return Search(weight).value;
        }

        private void BuildTree()
        {
            elements.Sort((Element lhs, Element rhs) =>
            {
                if (lhs.weight == rhs.weight)
                {
                    return 0;
                }
                else if (lhs.weight > rhs.weight)
                {
                    return -1;
                }
                return 1;
            });

            int j = 1;
            for (int i = 0; i < elements.Count; i++)
            {
                Element elmt = elements[i];

                if (i + j < elements.Count)
                {
                    elmt.left = elements[i + j];
                }

                if (i + j + 1 < elements.Count)
                {
                    elmt.right = elements[i + j + 1];
                }

                j++;
            }

            root = elements[0];
            elements.Clear();

            SortByTreeOrder(root);
        }

        private void SortByTreeOrder(Element element)
        {
            if (null == element)
            {
                return;
            }

            SortByTreeOrder(element.left);

            this.total_weight += element.weight;
            element.min = this.total_weight - element.weight + 1;
            element.max = this.total_weight;
            elements.Add(element);

            SortByTreeOrder(element.right);
        }

        private Element Search(int weight)
        {
            Element curr = root;

            while (null != curr)
            {
                if (curr.min <= weight && weight <= curr.max)
                {
                    return curr;
                }

                if (curr.min > weight)
                {
                    curr = curr.left;
                }
                else
                {
                    curr = curr.right;
                }
            }

            return null;
        }
    }
}