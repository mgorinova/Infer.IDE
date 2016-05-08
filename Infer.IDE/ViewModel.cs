using System;
using System.Collections.Generic;
using System.ComponentModel;
using GraphSharp.Controls;
using Backend;


namespace Infer.IDE
{
    public class ModelGraphLayout : GraphLayout<ModelVertex, ModelEdge, ModelGraph> { }

    public class ViewModel : INotifyPropertyChanged
    {
        private ModelGraph graph;
        private bool graphEnabled;
        public int count;        

        public ViewModel()
        {
            graph = new ModelGraph();
            graphEnabled = true;
        }

        public ModelGraph Graph
        {
            get { return graph; }
            set { graph = value; }
        }

        public bool GraphEnabled
        {
            get { return graphEnabled; }
        }

        internal void ReLayoutGraph()
        {
            graphEnabled = false;
            NotifyPropertyChanged("GraphEnabled");

            NotifyPropertyChanged("Graph");

            graphEnabled = true;            
            NotifyPropertyChanged("GraphEnabled");
        }

        internal List<string> Update(string path)
        {
            //returns list of var names in the dgml file at location "path"

            var ret = new List<string>();

            ModelGraph g = Backend.Utils.getModel(path);
            Graph.Union(g);

            foreach(ModelVertex v in g.Vertices)
            {
                ret.Add(v.Label);              
            }

            return ret;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        internal void Reset()
        {
            Graph = new ModelGraph();
        }


        internal ModelVertex findNodeByName(string varName)
        {
            return graph.FindVertexByName(varName);
        }
    }

    public class FadeOut : FadeTransition
    {
        public FadeOut() : base(0.0, 0.0, 1) { }
    }

    public class FadeIn : FadeTransition
    {
        public FadeIn() : base(0.0, 1.0, 1) { }
    }

    public class VisabilityFadeIn
    {
        public VisabilityFadeIn() { }

    }   

}
