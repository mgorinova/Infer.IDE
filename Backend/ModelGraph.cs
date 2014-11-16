using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using QuickGraph;

namespace Backend
{

    #region ModelVertex
    public class ModelVertex
    {
        public int ID;
        public string Label; 
        public NodeType Type;

        public ModelVertex(int i, string l, NodeType t)
        {
            ID = i;
            Label = l;
            Type = t;
        }

        public override string ToString()
        {
            return string.Format("{0}", Label);
        }
    }
    #endregion

    #region ModelEdge
    public class ModelEdge : Edge<ModelVertex>//, INotifyPropertyChanged
    {

        public ModelEdge(ModelVertex s, ModelVertex t)
            : base(s, t)
        { 
        }
    }
    #endregion

    public class ModelGraph : BidirectionalGraph<ModelVertex, ModelEdge>
    {
        public ModelGraph(DirectedGraph g)
            : base()
        {
            foreach (Node n in g.Nodes)
            {
                int id = ParseID(n.Id);
                NodeType type = Utils.ClassifyNode(n);
                this.AddVertex(new ModelVertex(id, n.Label, type));
            }

            foreach (Link e in g.Links)
            {
                int id_source = ParseID(e.Source);
                int id_target = ParseID(e.Target);

                // FIXME: sort vertices by their ID number, so we can perform a
                // binary search here, instead of naive one. Should provide better
                // performance for large graphs.

                this.AddEdge(new ModelEdge(findVertex(id_source), findVertex(id_target)));              
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

        private ModelVertex findVertex(int id)
        {
            foreach (ModelVertex v in this.Vertices)
            {
                if (v.ID == id) return v;
            }

            //FIXME: put an exception here

            System.IO.TextWriter tw = Console.Error;
            tw.WriteLine("Something went wrong... Node " + id + " doesn't exist in the graph." );
            tw.Close();

            return null;
        }
        
    }
        
}
