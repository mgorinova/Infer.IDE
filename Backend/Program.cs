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
            Node[] n = { new Node("0"), new Node("1"), new Node("2") };
            Link[] l = { new Link("0", "0", "1"), new Link("1", "0", "2") };

            var g = new DirectedGraph(n, l);

            Deserializer.WriteGraph(g);          

            DirectedGraph graphNew = Deserializer.ReadGraph();

            Utils.ClassifyNodes(graphNew);

            foreach (Node nd in graphNew.Nodes)
                Console.WriteLine(nd.Label + ", " + nd.Type);

            System.Threading.Thread.Sleep(20000);
            
        }
    }
}
