using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace YourCheese.GameAgent.TaskSolvers
{
    class ManifoldSolver : TaskSolver
    {
        private int xOffset = 566;
        private int yOffset = 378;

        public void Solve(DirectBitmap screen)
        {
            using (Bitmap screenImage = GameCapture.getGameScreenAsImage(new Rectangle(xOffset, yOffset, 793, 329)))
            {
                List<Vector2> buttons = new List<Vector2>();
                for (int i=1; i<11; i++)
                {
                    buttons.Add(posOfImage(i, screenImage));
                }
                TaskInput taskInput = new TaskInput();
                foreach (var button in buttons)
                {
                    float x = button.x + xOffset;
                    float y = button.y + yOffset;
                    taskInput.mouseClick(new Vector2(x, y));
                    System.Threading.Thread.Sleep(20);
                }
            }
        }

        private Vector2 posOfImage(int templateNum, Bitmap screen)
        {
            using (Image<Bgr, byte> Image1 = screen.ToImage<Bgr, byte>())
            using (Image<Bgr, byte> Image2 = new Image<Bgr, byte>(System.IO.Path.Combine(Constants.FILE_LOCATION, "templates", "manifolds", templateNum.ToString() + ".png")))
            using (Image<Gray, float> result_Matrix = Image1.MatchTemplate(Image2, TemplateMatchingType.CcoeffNormed))
            {
                Point[] MAX_Loc, Min_Loc;
                double[] min, max;
                //Limit ROI to look for Match

                //result_Matrix.ROI = new Rectangle(image_object.Width, image_object.Height, Area_Image.Width - image_object.Width, Area_Image.Height - image_object.Height);

                result_Matrix.MinMax(out min, out max, out Min_Loc, out MAX_Loc);

                Vector2 location = new Vector2((MAX_Loc[0].X), (MAX_Loc[0].Y));
                return location;
            }

            /*for (int y = 0; y < Matches.Data.GetLength(0); y++)
            {
                for (int x = 0; x < Matches.Data.GetLength(1); x++)
                {
                    if (Matches.Data[y, x, 0] >= Threshold) //Check if its a valid match
                    {
                        //Image2 found within Image1
                        Vector2 pos = new Vector2(y, x);
                        return pos;
                    }
                }
            }*/
            
        }

        public void abort()
        {

        }
    }
}
