using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms.Integration;
using QuickGraph;
using System.Windows.Controls;

namespace Backend
{
        
    public class ModelVertex
    {
        private int id;
        private string label; 
        private NodeType type;

        private string distribution;

        private WindowsFormsHost winHost;

        public ModelVertex(int i, string l, NodeType t)
        {
            id = i;
            label = l;
            type = t;
        }

        public ModelVertex(int i, string l, NodeType t, StackPanel hostsParent)
        {
            id = i;
            label = l;
            type = t;

            winHost = new WindowsFormsHost();
            winHost.Height = 150.0;
            hostsParent.Children.Add(winHost);
        }

        public override string ToString()
        {
            return string.Format("{0}", Label);
        }

        public int Id { get{ return id; } }
        public string Label { get { return label; } }
        public NodeType Type { get { return type; } }
        public WindowsFormsHost WinHost { get { return winHost; } }
        public string Distribution { get { return distribution; } set { distribution = value; } }
    }
    

    
    public class ModelEdge : Edge<ModelVertex>//, INotifyPropertyChanged
    {

        public ModelEdge(ModelVertex s, ModelVertex t)
            : base(s, t)
        { 
        }
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
                this.AddVertex(new ModelVertex(id, n.Label, type));
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
            }

            throw new Exception("Node with name " + name + " doesn't exist in the graph.");
        }


        public void Union(ModelGraph g)
        {
            foreach (ModelVertex v in g.Vertices)
            {
                this.AddVertex(v);
            }

            foreach (ModelEdge e in g.Edges)
            {
                this.AddEdge(e);
            }
        }
    }
        
}
