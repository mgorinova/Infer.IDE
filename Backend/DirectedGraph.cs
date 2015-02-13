﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Backend
{
    public enum NodeType
    {
        Variable = 0,
        ArrayVariable = 1,
        IntermediateVariable = 2,
        IntermediateArrayVariable = 3,
        ObservedVariable = 4,
        ObservedArrayVariable = 5,
        Constant = 6,        
        Factor = 7,
        Gate = 8,
        Other = 9
    }

    
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

        [XmlAttribute("Group")]
        public string Group;

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
