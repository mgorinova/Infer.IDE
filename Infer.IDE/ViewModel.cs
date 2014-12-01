﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            //Graph = Backend.Utils.getModel(@"d:\here.dgml\Model.dgml");
            NotifyPropertyChanged("Graph");

            graphEnabled = true;            
            NotifyPropertyChanged("GraphEnabled");

            // FIXME (if you can...) Program crashes when you put mouse 
            // over graph's visualisation during a refresh.
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

        internal void UpdateDistribution(string varName, string distribution)
        {
            var v = Graph.FindVertexByName(varName);
            v.Distribution = distribution;
        }

        internal void Reset()
        {
            Graph = new ModelGraph();
        }

    }

    

}
