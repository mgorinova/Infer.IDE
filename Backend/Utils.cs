using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
    static class Utils
    {
        static public void ClassifyNodes(DirectedGraph g)
        {
            foreach (Node n in g.Nodes)
            {
                switch(n.Background)
                {
                    case "#00ffffff":
                        switch(n.Foreground)
                        {
                            case "#ff0000ff":
                                if (string.Equals(n.NodeRadius, "100"))
                                    n.Type = NodeType.Variable;
                                else
                                    n.Type = NodeType.Distribution;
                                break;

                            case "#ff000000":
                                n.Type = NodeType.Distribution;
                                break;

                            default:
                                n.Type = NodeType.Other;
                                break;
                        }
                        break;

                    default: 
                        n.Type = NodeType.Other;
                        break;            
                }
            }

            //return g;
        }

        //public static 


    }
}
