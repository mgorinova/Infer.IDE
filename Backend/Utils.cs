using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Backend
{
    static class Utils
    {

        static public NodeType ClassifyNode(Node n)
        {
            NodeType type;

            switch (n.Background.ToLower())
            {
                case "#00ffffff":
                    switch (n.Foreground.ToLower())
                    {
                        case "#ff0000ff":
                            if (string.Equals(n.NodeRadius, "100"))
                                type = NodeType.Variable;
                            else
                                type = NodeType.ObservedVariable;
                            break;

                        case "#ff000000":
                            type = NodeType.Distribution;
                            break;

                        default:
                            type = NodeType.Other;
                            break;
                    }
                    break;

                case "ff000000":
                    type = NodeType.Factor;
                    break;
                 
                default:
                    type = NodeType.Other;
                    break;            
             }

            return type;
        }

        private static void filter(System.Collections.Generic.IEnumerable<ModelVertex> enumerable, ModelGraph m)
        { 
            var ret = new List<ModelVertex>();

            foreach(ModelVertex v in enumerable)
            {
                if (v.Type == NodeType.Variable || v.Type == NodeType.ObservedVariable)
                    m.AddVertex(v);                   
            }            
        }
        

        public static ModelGraph GetModel(DirectedGraph graph)
        {
            var g = new ModelGraph(graph);
            var m = new ModelGraph();

            filter(g.Vertices, m);
            //var edges = new List<ModelEdge>();           

            foreach(ModelVertex v in m.Vertices)
            {
                var S = new Stack<ModelEdge>(g.OutEdges(v));

                while (S.Count > 0)
                {
                    var curEdge = S.Pop();

                    // If current neighbour is a variable, we've reached the 
                    // desired depth and we don't need to go any further.
                    // Add the desired edge: v to curEdge.Target.
                    if (curEdge.Target.Type == NodeType.Variable || curEdge.Target.Type == NodeType.ObservedVariable)
                        m.AddEdge(new ModelEdge(v, curEdge.Target));

                    else
                    {
                        var tmp = g.OutEdges(curEdge.Target);
                        foreach (var tv in tmp)
                            S.Push(tv);                        
                    }                    
                }
            }

            return m;
        }


    }
}
