using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace BuddhaBrotMT
{
    class Program : Form
    {

        private readonly int NX, NY;
        private const double startX = -2.0f;
        private const double endX = 1.0f;
        private const double startY = 1.5f;
        private const double endY = -1.5f;
        private readonly double deltaX, deltaY;
        private readonly int smallest, biggest = 0;
        private readonly int[] pFractal;

        public Program(int w, int h) : base()
        {
            int i = 0;
            const int numThreads = 2;
            NX = w;
            NY = h;
            deltaX = (endX - startX) / (double)w;
            deltaY = (endY - startY) / (double)h;
            

            Text = "BuddhaBrotMT";
            Size = new Size(h, w);

            double inc = deltaY * h / (double)numThreads;
            pFractal = new int[NX * NY];

            Thread[] myThreads = new Thread[numThreads];

            for( double yy = startY; yy > endY && i < numThreads; yy+= inc, i++)
            {
                //ThreadKernel(yy, yy + inc - deltaY);
                
                // You can't send the same variables with thread, you must declare new ones - not necessarily consts
                double y0 = yy;
                myThreads[i] = new Thread(() => ThreadKernel(y0, y0 + inc - deltaY));
                myThreads[i].Start();
            }

            for(i=0; i < numThreads; i++)
                myThreads[i].Join();

            for (i = 0; i < NY; i++)
                for (int j = 0; j < NX; j++)
                    biggest = Math.Max(biggest, pFractal[i * NX + j]);

            for (i = 0; i < NY; i++)
                for (int j = 0; j < NX; j++)
                    smallest = Math.Min(biggest, pFractal[i * NX + j]);

            Console.WriteLine("Density value range:{0} to {1}", smallest, biggest);

        }

        public void ThreadKernel( double startPointY, double endPointY)
        {
            const int NMAX = 1000;
            int y_frac_start = (int)((startPointY - startY) / deltaY);
            int y_frac_end = (int)((endPointY - startY) / deltaY);
            int iter;
            int p_x, p_y;
            double x, y, xnew, ynew;

            for (double y0 = startPointY; y0 > endPointY; y0 += 0.1f * deltaY)
            {
                for(double x0 = startX; x0 < endX; x0 += 0.1f * deltaX)
                {
                    y = 0.0f;
                    x = 0.0f;
                    iter = 0;
                    while((x * x + y * y) < 4.0f && iter < NMAX)
                    {
                        xnew = x * x - y * y + x0;
                        ynew = 2.0f * x * y + y0;

                        x = xnew;
                        y = ynew;
                        iter++;
                    }

                    if( iter < NMAX )
                    {
                        x = 0;
                        y = 0;

                        for( int i = 0; i < iter; i++)
                        {
                            xnew = x * x - y * y + x0;
                            ynew = 2.0f * x * y + y0;

                            x = xnew;
                            y = ynew;

                            p_x = (int)((x - startX) / deltaX);
                            p_y = (int)((y - startY) / deltaY);

                            if( p_x >= 0 && p_y >= y_frac_start && p_x < NX && p_y <= y_frac_end )
                            {
                                pFractal[p_y * NX + p_x]++;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Render(e);
        }

        public void Render(PaintEventArgs e)
        {
            int temp1, temp2, col;
            double a;

            e.Graphics.FillRectangle(Brushes.Black, 0, 0, this.Width, this.Height);
            var pixel = new Bitmap(this.Width, this.Height, e.Graphics);
            
            for (int y =0; y < NY; y++)
            {
                for(int x = 0; x < NX; x++)
                {
                    temp1 = 2 * pFractal[y * NX + x] - smallest;
                    temp2 = biggest - smallest;
                    a = (float)temp1 / (float)temp2;
                    if (a > 1.0f)
                        a = 1.0f;
                    col = (int)(255.0f * Math.Pow(a, 0.5f));
                    pixel.SetPixel(y , x, Color.FromArgb(col, col, 255 - col / 2));
                }
            }
            e.Graphics.DrawImage(pixel, 0, 0);
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new Program( 1200, 1200 ));
        }
    }
}
