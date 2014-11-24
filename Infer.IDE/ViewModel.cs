using System;
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
        public int count;

        public ViewModel()
        {
            
        }

        public ModelGraph Graph
        {
            get { return graph; }
            set { graph = value; }
        }

        internal void ReLayoutGraph(string path)
        {
            Graph = Backend.Utils.getModel(path);
            NotifyPropertyChanged("Graph");
            // FIXME (if you can...) Program crashes when you put mouse 
            // over graph's visualisation during a refresh.
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
    
}
