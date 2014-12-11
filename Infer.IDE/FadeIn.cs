using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphSharp;
using GraphSharp.Controls;

namespace Infer.IDE
{
    public class FadeOut : FadeTransition
    {
        public FadeOut()
            : base(0.0, 0.0, 1)
        {

        }
    }

    public class FadeIn : FadeTransition
    {
        public FadeIn()
            : base(0.0, 1.0, 1)
        {

        }
    }

    public class VisabilityFadeIn
    {
        public VisabilityFadeIn()
        { 

        }
    }
}
