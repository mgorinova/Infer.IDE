using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infer.IDE
{
    class Context
    {
        private string label;
        private ExerciseType type;

        private bool visualisations;
        private bool furtherInfo;

        private string code;

        public Context(string label, ExerciseType type, bool visualisations, bool furtherInfo, string code)
        {
            this.label = label;
            this.type = type;
            this.visualisations = visualisations;
            this.furtherInfo = furtherInfo;
            this.code = code;
        }

        public string Label { get { return label; } }
        public bool Visualisations { get { return visualisations; } }
        public bool FurtherInfo { get { return furtherInfo; } }
        public string Code { get { return code; } }        

        public int getType()
        {
            if (type == ExerciseType.Example) return 1;
            else if (type == ExerciseType.End) return 2;
            else return 0;
        }

    }
}
