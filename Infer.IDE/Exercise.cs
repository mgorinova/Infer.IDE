using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Infer.IDE
{
    public enum ExerciseType
    {
        Example = 0,
        Workbook = 1,
        Exercise = 2,
        End = 3
    }

    class Exercise
    {
        private string userID;
        private string groupID;
        private int exerciseID;

        private ExerciseType exerciseType;
        private string name;

        private string folderPath;

        private string code;
        private bool lastCompilationSuccessful;

        private int keystrokes;
        private int backspaceKeystrokes;
        private Stopwatch stopwatch;

        public Exercise(string userID, string groupID, int exerciseID)
        {
            this.userID = userID;
            this.groupID = groupID;
            this.exerciseID = exerciseID;

            folderPath = System.IO.Directory.GetCurrentDirectory() + "\\Results\\" + userID + "\\";

            keystrokes = 0;
            backspaceKeystrokes = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void FinishWith(string code)
        {
            stopwatch.Stop();
            string output = String.Format(@"UserID: {0}
Group: {1}
Exercise: {2}

Completion Time: {3}
Number of Keystrokes: {4}
Number of Backspaces: {5}

*************** Exercise {2} -- {6} -- {7} ***************
{8}"
                , userID, groupID, exerciseID, stopwatch.Elapsed,
                keystrokes, backspaceKeystrokes, exerciseType, name, code);

            var path = folderPath + exerciseID;
            File.WriteAllText(path, output);

        }


        public void Keystroke()
        {
            keystrokes++;
        }


        internal void Backspace()
        {
            backspaceKeystrokes++;
        }
    }
}
