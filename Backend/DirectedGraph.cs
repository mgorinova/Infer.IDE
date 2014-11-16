using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Backend
{
    public enum NodeType
    {
        Variable = 0,
        Distribution = 1,
        ObservedVariable = 2,
        Factor = 3,
        Other = 4
    }

    #region Node
    [Serializable]
    [XmlRoot("Node")]
    public class Node
    {
        [XmlAttribute("Id")]
        public string Id;

        [XmlAttribute("Label")]
        public string Label;

        [XmlAttribute("Background")]
        public string Background;

        [XmlAttribute("Foreground")]
        public string Foreground;


        //FIXME: we don't always have NodeRadius
        [XmlAttribute("NodeRadius")]
        public string NodeRadius;

        [XmlIgnore()]
        public NodeType Type;

        public Node(string i)
        {
            Id = i;
        }

        public Node()
        {
        }
    }
    #endregion 

    #region Link
    [Serializable]
    [XmlRoot("Link")]
    public class Link
    {
        [XmlAttribute("Label")]
        public string Label;

        [XmlAttribute("Source")]
        public string Source;

        [XmlAttribute("Target")]
        public string Target;

        public Link(string l, string s, string t)
        {
            Label = l;
            Source = s;
            Target = t;
        }

        public Link()
        {
        }
    }
    #endregion

    [Serializable]
    [XmlRoot("DirectedGraph")]
    public class DirectedGraph
    {

        [XmlArrayItem("Node")]
        public Node[] Nodes = new Node[3];

        [XmlArrayItem("Link")]
        public Link[] Links = new Link[2];

        public DirectedGraph() { }

        public DirectedGraph(Node[] n, Link[] l)
        {
            Nodes = n;
            Links = l;

        }

    }




}
