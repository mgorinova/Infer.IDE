using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Backend
{
    public static class Utils
    {
        #region Classify
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
                            {
                                type = NodeType.Variable;
                                if (n.Label.Contains("[")) type = NodeType.ArrayVariable;
                                if (n.Label.StartsWith("v"))
                                    if (n.Label.StartsWith("vbool") ||
                                        n.Label.StartsWith("vdouble") ||
                                        n.Label.StartsWith("vint"))
                                    {
                                        if (type == NodeType.ArrayVariable) type = NodeType.IntermediateArrayVariable;
                                        else type = NodeType.IntermediateVariable;
                                    }
                            }
                            else
                            {
                                type = NodeType.ObservedVariable;
                                if (n.Label.Contains("="))
                                {
                                    type = NodeType.ObservedVariable;
                                    n.Label = n.Label.Split('=')[0].TrimEnd(' ');
                                    Console.WriteLine("Observed {0}", n.Label);
                                }
                                if (n.Label.Contains("[")) type = NodeType.ObservedArrayVariable;
                            }
                            break;

                        case "#ff000000":
                            
                            type = NodeType.Constant;

                           /*
                            * FIXME: some observed variables are also black (unfortunatelly)
                            * When uncommented, the line below kinnda fixes this problem in
                            * some cases (e.g. "BugsAndRats"), but introduces other problems
                            * with other cases (e.g. "MixtureOfGaussians")... 
                            */
                            // if (n.Label.Contains("[")) type = NodeType.ObservedArrayVariable;

                            break;

                        default:
                            type = NodeType.Other;
                            break;
                    }
                    break;

                case "ff000000":
                    type = NodeType.Factor;
                    if (n.Group == "Expanded" || n.Group == "Collapsed")
                        type = NodeType.Gate;
                    break;
                 
                default:
                    type = NodeType.Other;
                    break;            
             }

            return type;
        }
        #endregion

        private static void filter(System.Collections.Generic.IEnumerable<ModelVertex> enumerable, ModelGraph m)
        { 
            var ret = new List<ModelVertex>();

            foreach(ModelVertex v in enumerable)
            {
                if (v.Type == NodeType.Variable ||
                    v.Type == NodeType.ArrayVariable ||
                    v.Type == NodeType.ObservedVariable ||
                    v.Type == NodeType.ObservedArrayVariable ||
                    v.Type == NodeType.IntermediateVariable ||
                    v.Type == NodeType.IntermediateArrayVariable)
                    m.AddVertex(v);                   
            }            
        }
        

        /*
         * Transform a Factor Graph to a Graphical Model
         */ 
        public static ModelGraph FactorToModel(DirectedGraph graph)
        {
            var g = new ModelGraph(graph);
            var m = new ModelGraph();

            filter(g.Vertices, m); //FIXME: ugly - change it to m.AddRelevantVertices(g.Vertices) or something
            //var edges = new List<ModelEdge>();           

            foreach(ModelVertex v in m.Vertices)
            {
                var S = new Stack<ModelEdge>(g.OutEdges(v));
                var used = new HashSet<ModelVertex>();

                while (S.Count > 0)
                {
                    var curEdge = S.Pop();

                    // If current neighbour is a variable, we've reached the 
                    // desired depth and we don't need to go any further.
                    // Add the desired edge: v to curEdge.Target.
                    if (curEdge.Target.Type == NodeType.Variable ||
                        curEdge.Target.Type == NodeType.ArrayVariable ||
                        curEdge.Target.Type == NodeType.ObservedVariable ||
                        curEdge.Target.Type == NodeType.ObservedArrayVariable ||
                        curEdge.Target.Type == NodeType.IntermediateVariable ||
                        curEdge.Target.Type == NodeType.IntermediateArrayVariable)
                    {
                        if (!used.Contains(curEdge.Target))
                        {
                            m.AddEdge(new ModelEdge(v, curEdge.Target));
                            used.Add(curEdge.Target);
                        }
                    }
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

        public static ModelGraph getModel(string dest)
        {
            Console.Write("deserializing and processing graph...");

            DirectedGraph graphNew = Deserializer.ReadGraph(dest);
            ModelGraph model = Utils.FactorToModel(graphNew);

            Console.WriteLine(" OK\n");

            return model;
        }

    }
}
