using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Infer.IDE
{
    class User
    {
        private string userID;
        private string groupID;

        private int currExercise;

        private Exercise[] exercises = new Exercise[16];
        private Context[] context = new Context[16];

        public User(string userID, string groupID)
        {
            this.userID = userID;
            this.groupID = groupID;

            currExercise = 0;

            System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + "\\Results\\" + userID + "\\");

            contextIni();
        }

        private void contextIni()
        {
            if (groupID.StartsWith("A"))
            {
                var c = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + 0 + ".fsx");
                context[0] = new Context("Example " + (0), ExerciseType.Example, false, false, c);

                c = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + 1 + ".fsx");
                context[1] = new Context("Exercise " + 0, ExerciseType.Workbook, false, false, c);

                for (int i = 2; i <=6; i+=2)
                {
                    var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + i + ".fsx");
                    context[i] = new Context("Example " + (i/2), ExerciseType.Example, true, false, code);
                }

                c = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + 2 + ".fsx");
                context[2] = new Context("Example 1", ExerciseType.Example, true, true, c);

                for (int i = 3; i <= 7; i += 2)
                {
                    var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + i + ".fsx");
                    context[i] = new Context("Exercise " + (i / 2), ExerciseType.Workbook, true, false, code);
                }
            }
            else
            {           

                for (int i = 0; i <= 6; i += 2)
                {
                    var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + i + ".fsx");
                    context[i] = new Context("Example " + (i / 2), ExerciseType.Example, false, false, code);
                }
                for (int i = 1; i <= 7; i += 2)
                {
                    var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + i + ".fsx");
                    context[i] = new Context("Exercise " + (i / 2), ExerciseType.Workbook, false, false, code);
                }
            }

            switch (groupID)
            {
                case "A00":

                    for (int i = 8; i < 12; i++)
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + i + ".fsx");
                        context[i] = new Context("Exercise " + (i - 4), ExerciseType.Exercise, true, false, code); 
                    }
                    for (int i = 12; i < 16; i++)
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + i + ".fsx");
                        context[i] = new Context("Exercise " + (i - 4), ExerciseType.Exercise, false, false, code);
                    }

                    if (groupID == "B00")
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + 8 + ".fsx");
                        context[8] = new Context("Exercise " + (4), ExerciseType.Exercise, true, true, code); 
                    }

                    break;
                case "B00":
                    goto case "A00";

                case "A01":
                    for (int i = 8; i < 12; i++)
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + i + ".fsx");
                        context[i] = new Context("Exercise " + (i - 4), ExerciseType.Exercise, false, false, code); 
                    }
                    for (int i = 12; i < 16; i++)
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + i + ".fsx");
                        context[i] = new Context("Exercise " + (i - 4), ExerciseType.Exercise, true, false, code);
                    }
                    
                    if (groupID == "B01")
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + 12 + ".fsx");
                        context[12] = new Context("Exercise " + (8), ExerciseType.Exercise, true, true, code);
                    }

                    break;
                case "B01":
                    goto case "A01";

                case "A10":
                    for (int i = 8; i < 12; i++)
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + (i+4) + ".fsx");
                        context[i] = new Context("Exercise " + (i - 4), ExerciseType.Exercise, true, false, code); 
                    }
                    for (int i = 12; i < 16; i++)
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + (i-4) + ".fsx");
                        context[i] = new Context("Exercise " + (i - 4), ExerciseType.Exercise, false, false, code);
                    }

                    if (groupID == "B10")
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + 12 + ".fsx");
                        context[8] = new Context("Exercise " + (4), ExerciseType.Exercise, true, true, code);
                    }

                    break;
                case "B10":
                    goto case "A10";

                case "A11":
                    for (int i = 8; i < 12; i++)
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + (i+4) + ".fsx");
                        context[i] = new Context("Exercise " + (i - 4), ExerciseType.Exercise, false, false, code); 
                    }
                    for (int i = 12; i < 16; i++)
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + (i-4) + ".fsx");
                        context[i] = new Context("Exercise " + (i - 4), ExerciseType.Exercise, true, false, code);
                    }

                    if (groupID == "B11")
                    {
                        var code = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\Exercises\\" + 8 + ".fsx");
                        context[12] = new Context("Exercise " + (8), ExerciseType.Exercise, true, true, code);
                    }

                    break;
                case "B11":
                    goto case "A11";
            }

            if (groupID.StartsWith("A00") || groupID.StartsWith("B00"))
            { 
            }
            

        }

        public void Keystroke()
        {
            if (exercises[currExercise] != null) exercises[currExercise].Keystroke();
        }

        public void StartExercise()
        {
            exercises[currExercise] = new Exercise(userID, groupID, currExercise);
        }

        public Context FinishCurrent(string code)
        {
            exercises[currExercise].FinishWith(code);
            if (currExercise < 15) return context[++currExercise];
            else return new Context("Thank you for your participation!", ExerciseType.End, true, false, "");
        }

        public Context InitialContext()
        {
            return context[0];
        }

        public int CurrExercise { get { return currExercise; } set { currExercise = value; } }

        internal void Backspace()
        {
            exercises[currExercise].Backspace();
        }
    }
}
