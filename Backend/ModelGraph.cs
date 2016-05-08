using System;
using System.Collections.Generic;
using System.Windows.Forms.Integration;
using QuickGraph;

namespace Backend
{        
    public class ModelVertex
    {
        private int id;
        private string label;
        private string programLabel;
        private NodeType type;

        private string distribution;
        private int location;

        private bool observed;
        
        private int hostID; 
        private WindowsFormsHost winHost;

        public ModelVertex(int i, string l, NodeType t)
        {
            id = i;
            label = l;
            programLabel = ParseLabel(l);
            type = t;
        }

        public WindowsFormsHost Chart
        {
            get { Console.WriteLine("getting chart!"); return new WindowsFormsHost() { Child = winHost.Child }; } 
        }

        public override string ToString()
        {
            return string.Format("{0}", Label);
        }

        private string ParseLabel(string l)
        {
            // FIXME: hmm, that might break (as well as the whole concept). 
            // Take a look on the "Mixture of Gaussians" example - 
            // we have several "array" nodes, all with the same "programLabel" 

            var ifObs = l.Split('=')[0].TrimEnd(' ');

            string[] splitArr = new string[] { @"[" };
            var sub = ifObs.Split(splitArr, StringSplitOptions.RemoveEmptyEntries);

            //if (sub.Length > 1) Console.WriteLine("Split in {0} and {1}", sub[0], sub[1]);
            return sub[0];
        }

        public int Id { get{ return id; } }
        public string Label { get { return label; } }
        public string ProgramLabel { get { return programLabel; } }
        public NodeType Type { get { return type; } set { type = value; } }
        public WindowsFormsHost WinHost { get { return winHost; } set { winHost = value; } }
        public string Distribution { get { return distribution; } set { distribution = value; } }
        public bool Observed { get { return observed; } set { observed = value; } }
        public int HostID { get { return hostID; } set { hostID = value; } }
        public int Location { get { return location; } set { location = value; } }
    }
    

    
    public class ModelEdge : Edge<ModelVertex>
    {

        public ModelEdge(ModelVertex s, ModelVertex t) : base(s, t) { }

    }
    

    public class ModelGraph : BidirectionalGraph<ModelVertex, ModelEdge>
    {         

        public ModelGraph(DirectedGraph g)
            : base()
        {
            foreach (Node n in g.Nodes)
            {
                int id = ParseID(n.Id);                
                NodeType type = Utils.ClassifyNode(n);
                string label = n.Label;

                ModelVertex add = new ModelVertex(id, label, type);

                this.AddVertex(add);
            }

            foreach (Link e in g.Links)
            {
                int id_source = ParseID(e.Source);
                int id_target = ParseID(e.Target);

                // FIXME: sort vertices by their ID number, so we can perform a
                // binary search here, instead of naive one. Should provide better
                // performance for large graphs.

                this.AddEdge(new ModelEdge(FindVertex(id_source), FindVertex(id_target)));              
            }
           
        }
       

        #region Other Constructors
        public ModelGraph() { }

        public ModelGraph(bool allowParallelEdges)
            : base(allowParallelEdges) { }

        public ModelGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity) { }
        #endregion

        private int ParseID(string s) { return int.Parse((s).Remove(0, 4)); }

        private ModelVertex FindVertex(int id)
        {
            foreach (ModelVertex v in this.Vertices)
            {
                if (v.Id == id) return v;
            }

            throw new Exception("Node " + id + " doesn't exist in the graph.");
        }

        public ModelVertex FindVertexByName(string name)
        {
            foreach (ModelVertex v in this.Vertices)
            {
                if (v.Label == name) return v;
                if (v.ProgramLabel == name) return v;
            }
            return null;
        }


        public void Union(ModelGraph g)
        {
            foreach (ModelVertex v in g.Vertices)
            {
                var v1 = this.FindVertexByName(v.Label);
               // if (v1 != null) v1 = v; // could potentially break... however, I think the only way this situation could happen  
                                        // is when we've started searching the dgml file from an observed variable, in which case
                                        // there won't be any edges going out of it.
                if(v1==null) this.AddVertex(v);
            }

            foreach (ModelEdge e in g.Edges)
            {
                try { this.AddEdge(e); }
                catch (KeyNotFoundException)
                {
                    var source = this.FindVertexByName(e.Source.Label);
                    var target = this.FindVertexByName(e.Target.Label);

                    if (source != null && target != null)
                    {
                       this.AddEdge(new ModelEdge(source, target));
                    }
                    else
                    { 
                        throw new KeyNotFoundException();
                    }

                }
                
            }
        }
    }
        
}
