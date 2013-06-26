using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using System.Threading;

namespace Agent.HalfFull
{
    public class Program
    {
        static Bitmap _display;
        static Timer _updateClockTimer;

        const bool FORMAT_24 = true;

        public static void Main()
        {
            // initialize our display buffer
            _display = new Bitmap(Bitmap.MaxWidth, Bitmap.MaxHeight);

            // display the time immediately
            UpdateTime(null);

            // obtain the current time
            DateTime currentTime = DateTime.Now;
            // set up timer to refresh time every minute
            TimeSpan dueTime = new TimeSpan(0, 0, 0, 59 - currentTime.Second, 1000 - currentTime.Millisecond); // start timer at beginning of next minute
            TimeSpan period = new TimeSpan(0, 0, 1, 0, 0); // update time every minute
            _updateClockTimer = new Timer(UpdateTime, null, dueTime, period); // start our update timer

            // go to sleep; time updates will happen automatically every minute
            Thread.Sleep(Timeout.Infinite);
        }

        static void UpdateTime(object state)
        {
            // obtain the current time
            DateTime currentTime = DateTime.Now;
            // clear our display buffer
            _display.Clear();

            Font fontDate = Resources.GetFont(Resources.FontResources.ubuntu12);
            Font fontHour = Resources.GetFont(Resources.FontResources.awake72o);
            Font fontTicks = Resources.GetFont(Resources.FontResources.ubuntu12c);

            // draw date 
            _display.DrawTextInRect(
                currentTime.ToString("dd MMM, ddd"),
                2, 108, 126, 26,
                Bitmap.DT_AlignmentCenter,
                Color.White,
                fontDate);
            // draw separator
            _display.DrawLine(Color.White, 1, 0, 106, 128, 106);

            //Draw  time
            int hrs = FORMAT_24?currentTime.Hour:currentTime.Hour%12;
            int hrs_w, hrs_h;
            fontHour.ComputeExtent(hrs.ToString("D2"), out hrs_w, out hrs_h);

            // compute share to fill
            int empty_part = (int)(hrs_h - (float)hrs_h/60f * currentTime.Minute);

            _display.DrawText(hrs.ToString(),
                fontHour,
                Color.White,
                10, 5);

            // start at top inverse all pixels except last and first
            bool OutIn = false;

            for (int y = 5; y < 5 + empty_part; ++y)
            {
                OutIn = false;
                for (int x = 10; x < 100; ++x)
                {
                    if (_display.GetPixel(x, y) == Color.White)
                    {
                        if (!OutIn) // "in edge" - leave as is
                        {
                            OutIn = true;
                        }
                        else
                            _display.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        if (OutIn && _display.GetPixel(x, y+1) == Color.White) // out edge
                        {
                            _display.SetPixel(x - 1, y, Color.White);
                            OutIn = false;
                        }
                    }
                }
            }

            _display.SetClippingRectangle(0, 0, 120, 128);

            // Draw minute rules
            for ( int m = 0; m <= 6; ++m)
            {
                _display.DrawLine(Color.White, 1, 105, 5 + 14 * m, 115, 5 + 14 * m);
                _display.DrawLine(Color.White, 1, 110, 5 + 14 * m + 7, 115, 5 + 14 * m + 7);
                _display.DrawText((60 - m * 10).ToString(), fontTicks, Color.White, 118, 14 * m);
            }


            
            

            // flush the display buffer to the display
            _display.Flush();
        }

    }
}
