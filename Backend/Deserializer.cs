using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Backend
{
    class Deserializer
    {
        public static DirectedGraph ReadGraph()
        {
            var xmlns = new XmlSerializerNamespaces();
            xmlns.Add(string.Empty, string.Empty);
            xmlns.Add(string.Empty, "http://schemas.microsoft.com/vs/2009/dgml");

            var reader = new XmlSerializer(typeof(DirectedGraph), "http://schemas.microsoft.com/vs/2009/dgml");
            System.IO.StreamReader file = new System.IO.StreamReader("TwoCoins.dgml");
            DirectedGraph ret = new DirectedGraph();
            ret = (DirectedGraph)reader.Deserialize(file);

            return ret;
        }

        public static void WriteGraph(DirectedGraph g)
        {

            var xmlns = new XmlSerializerNamespaces();
            xmlns.Add(string.Empty, string.Empty);
            xmlns.Add(string.Empty, "http://schemas.microsoft.com/vs/2009/dgml");

            var writer = new XmlSerializer(typeof(DirectedGraph), "http://schemas.microsoft.com/vs/2009/dgml");

            System.IO.StreamWriter file = new System.IO.StreamWriter("graph.xml");
            writer.Serialize(file, g, xmlns);
            file.Close();

        }

    }
}