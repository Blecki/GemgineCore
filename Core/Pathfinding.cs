using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class Pathfinding<Node> where Node: class
    {
        private Func<Node, List<Node>> enumerateNeighbors;

        public Pathfinding(Func<Node, List<Node>> enumerateNeighbors)
        {
            this.enumerateNeighbors = enumerateNeighbors;
        }

        public class PathfindingResult
        {
            public List<Node> Path;
            public bool FoundPath = false;
        }

        internal enum NodeState
        {
            Open,
            Closed
        }

        internal class PathNode
        {
            internal PathNode parent;
            internal Node realNode;
            internal NodeState state = NodeState.Open;
        }

        public PathfindingResult FindPath(Node from, Func<Node, bool> goal, Func<Node, float> hueristic)
        {
            var nodes = new Dictionary<Node, PathNode>();
            var head = new PathNode { parent = null, realNode = from, state = NodeState.Closed };
            nodes.Add(from, head);
            var openNodes = new List<PathNode>();

            var result = new PathfindingResult();
            result.FoundPath = false;

            while (head != null)
            {
                if (goal(head.realNode))
                {
                    var pathEnd = head;
                    result.Path = new List<Node>();
                    while (pathEnd != null)
                    {
                        result.Path.Add(pathEnd.realNode);
                        pathEnd = pathEnd.parent;
                    }
                    result.Path.Reverse();
                    result.FoundPath = true;
                    return result;
                }

                foreach (var newOpenNode in enumerateNeighbors(head.realNode))
                {
                    if (nodes.ContainsKey(newOpenNode)) continue;
                    var newNode = new PathNode { parent = head, realNode = newOpenNode, state = NodeState.Open };
                    nodes.Add(newOpenNode, newNode);
                    openNodes.Add(newNode); //TODO: Sort addition based on heuristic
                }

                if (openNodes.Count == 0) head = null;
                else
                {
                    head = openNodes[0];
                    head.state = NodeState.Closed;
                    openNodes.RemoveAt(0);
                }
            }

            result.FoundPath = false;
            return result;
        }
    }
}
