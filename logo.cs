using System.IO;
using System;
using System.Drawing;

namespace ConsoleApp5
{
    public class logo
    {
        public logo()
        {//constructor

            //get full location of the project
            string full_location = AppDomain.CurrentDomain.BaseDirectory;
            //replace the debug and bin   bin/Debug
            string new_location = full_location.Replace("bin\\Debug\\", "");
            //COMBINE PATH
            string full_path = Path.Combine(new_location, "images.png");
            //time to use ascii
            //creating the bitmap class
            Bitmap image = new Bitmap(full_path);
            //then set size
            image = new Bitmap(image, new Size(150, 140));



            //outer inner loop
            for (int height = 0; height < image.Height; height++)
            {

                for (int width = 0; width < image.Width; width++)
                {
                    Color pixelColor = image.GetPixel(width, height);
                    int gray = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    char assciChar = gray > 200 ? '.' : gray > 150 ? '*' : gray > 100 ? 'O' : gray > 50 ? '#' : '@';
                    Console.Write(assciChar);
                }
                Console.WriteLine();
            }
        }//end of costructor        

    }
}//end of class