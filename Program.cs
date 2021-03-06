﻿using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using System.Threading;
using Agent.Contrib.Hardware;

namespace Agent.HalfFull
{
    public class Program
    {
        static Bitmap _display;
        static Timer _updateClockTimer;

        private static bool FORMAT_24 = true;
        static DateTime currentTime = DateTime.Now;

        public event ButtonHelper.ButtonPress OnButtonPress;
        private static ButtonHelper buttonHelper;



        static Font fontDate = Resources.GetFont(Resources.FontResources.ubuntu12);
        static Font fontHour = Resources.GetFont(Resources.FontResources.AmericanCaptain);
        static Font fontTicks = Resources.GetFont(Resources.FontResources.ubuntu12c);

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
            //TimeSpan period = new TimeSpan(0, 0, 1, 0, 0); // update time every minute
            TimeSpan period = new TimeSpan(0, 0, 0, 1, 0); // update time every minute
            _updateClockTimer = new Timer(UpdateTime, null, dueTime, period); // start our update timer

            buttonHelper = ButtonHelper.Current;
            buttonHelper.OnButtonPress += buttonHelper_OnButtonPress;

            // go to sleep; time updates will happen automatically every minute
            Thread.Sleep(Timeout.Infinite);
        }

        static void UpdateTime(object state)
        {
            // obtain the current time
            //DateTime currentTime = DateTime.Now;
            currentTime = currentTime.AddMinutes(1);
            // clear our display buffer
            _display.Clear();


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

            // compute share to fill
            const int TICK_STEP = 14;
            const int OFFSET = 8;
            const float REAL_HEIGHT = 84f;
            int empty_part = (int)(REAL_HEIGHT * ( 1 - currentTime.Minute/60f));
            //int empty_part = (int)(REAL_HEIGHT * (1 - 30 / 60f));

            string hours = (hrs).ToString();

            _display.DrawTextInRect(hours, 10, -22, 90, 90, Bitmap.DT_AlignmentCenter | Bitmap.DT_IgnoreHeight, Color.White, fontHour);

            if (!FORMAT_24)
                if (currentTime.Hour < 12)
                    _display.DrawText("AM", fontDate, Color.White, 3, 60);
                else
                    _display.DrawText("PM", fontDate, Color.White, 3, 60);


            // start at top inverse all pixels except last and first
            bool OutIn = false;
            bool FirstRow = true;
            bool FoundFirstRow = false;

            for (int y = OFFSET; y < OFFSET + empty_part; ++y)
            {
                OutIn = false;
                if (FoundFirstRow) FirstRow = false;
                for (int x = 10; x < 100; ++x)
                {
                    if (_display.GetPixel(x, y) == Color.White)
                    {
                        if (!OutIn ) // "in edge" - leave as is
                        {
                            OutIn = true;
                            FoundFirstRow = true;
                        }
                        else if (_display.GetPixel(x, y + 2) == Color.White && _display.GetPixel(x - 1, y) == Color.White && _display.GetPixel(x + 2, y) == Color.White && !FirstRow)
                            _display.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                            OutIn = false;
                    }
                }
            }

            //_display.SetClippingRectangle(0, 0, 120, 128);

            // Draw minute rules
            for ( int m = 0; m < 6; ++m)
            {
                _display.DrawLine(Color.White, 1, 105, OFFSET + TICK_STEP * m, 115, OFFSET + TICK_STEP * m);
                _display.DrawLine(Color.White, 1, 110, OFFSET + TICK_STEP * m + 6, 115, OFFSET + TICK_STEP * m + 6);
                _display.DrawText((60 - m * 10).ToString(), fontTicks, Color.White, 118, TICK_STEP * m);
            }

            // last tick
            _display.DrawLine(Color.White, 1, 105, OFFSET + TICK_STEP * 6, 115, OFFSET + TICK_STEP * 6);
            _display.DrawText((60 - 6 * 10).ToString(), fontTicks, Color.White, 118, TICK_STEP * 6);


            
            

            // flush the display buffer to the display
            _display.Flush();
        }

        private static void buttonHelper_OnButtonPress(Buttons button, InterruptPort port, ButtonDirection direction, DateTime time)
        {
            if (button == Buttons.MiddleRight && direction == ButtonDirection.Up)
            {
                FORMAT_24 = !FORMAT_24;
                UpdateTime(null);
            }

        }


    }
}
