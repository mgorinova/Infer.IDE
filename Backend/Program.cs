using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
    class Program
    {
        static void Main(string[] args)
        {

            String dest = "TwoCoins.dgml";
            //String dest = "Sprinkler-Mine.dgml";
            //String dest = "TwoCoins-obs.dgml";
            //String dest = "Sprinkler.dgml";


            DirectedGraph graphNew = Deserializer.ReadGraph(dest);

            ModelGraph model = Utils.GetModel(graphNew);

            foreach (ModelVertex v in model.Vertices)
            {
                Console.WriteLine("Vertex: " + v.Label);
            }

            foreach (ModelEdge e in model.Edges)
            {
                Console.WriteLine("Edge {0} -> {1}", e.Source.Label, e.Target.Label);
            }
            
        }
    }
}
