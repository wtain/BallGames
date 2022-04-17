using System;
using System.Collections.Generic;
using System.Linq;

namespace bub.Visual.Detail
{
    public class ScoreCalculator
    {
        public class StateNode
        {
            private int score;
            private Field field;
            private LinkedList<StateNode> children;

            public StateNode(Field field, int score = 0)
            {
                this.field = field;
                this.score = score;
                children = new LinkedList<StateNode>();
                //BuildChildren();
            }

            private void BuildChildren()
            {
                var visited = new Matrix<bool>(field.Width, field.Height, false);
                for (int y = 0; y < field.Height; ++y)
                {
                    for (int x = 0; x < field.Width; ++x)
                    {
                        if (!field.IsBall(x, y))
                            continue;
                        if (visited[x, y])
                            continue;
                        var sel = field.GetSelection(x, y);
                        foreach (var b in sel.Items)
                            visited[b.X, b.Y] = true;
                        if (sel.Count < 2)
                            continue;
                        var newField = new Field(field);
                        newField.EraseBalls(sel);
                        children.AddLast(new StateNode(newField, score + sel.Points));
                    }
                }
            }

            public int MaxScore()
            {
                int result = 0;
                var nodes = new Stack<StateNode>();
                nodes.Push(this);
                while (nodes.Count > 0)
                {
                    var n = nodes.Pop();
                    result = Math.Max(result, n.score);
                    n.children.ToList().ForEach(nc => nodes.Push(nc));
                }
                return result;
            }
        }

        public int CalculateMaxScore(Field field)
        {
            StateNode root = new StateNode(field);
            return root.MaxScore();
        }
    }
}
