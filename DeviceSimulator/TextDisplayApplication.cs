using System;

using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Input;

using Microsoft.SPOT.Presentation.Media;

namespace mSensorScanningErrorCount
{
    public class TextDisplayApplication : Microsoft.SPOT.Application
    {
        private Window mMainWindow;
        private Text mText;

        public TextDisplayApplication():this(String.Empty)
        {
        }

        public TextDisplayApplication(string startText)
        {
            mMainWindow = CreateWindow();
            DisplayText(startText);
        }

        internal void DisplayText(string text, params object[] args)
        {
            if (mText.CheckAccess())
                mText.TextContent = String.Format(text, args);
            else
                mText.Dispatcher.BeginInvoke(new DispatcherOperationCallback(delegate { mText.TextContent = text; return null; }), text);
        }

        internal void StartApplication()
        {
            this.Run(mMainWindow);
        }

        internal Window CreateWindow()
        {
            // Create a window object and set its size to the
            // size of the display.
            mMainWindow = new Window();
            mMainWindow.Height = SystemMetrics.ScreenHeight;
            mMainWindow.Width = SystemMetrics.ScreenWidth;
            mMainWindow.Background = new SolidColorBrush(Color.Black); 

            // Create a single text control.
            mText = new Text();

            mText.Font = Properties.Resources.GetFont(Properties.Resources.FontResources.small);
            mText.ForeColor = Microsoft.SPOT.Presentation.Media.Color.White;
            mText.TextContent = Properties.Resources.TextStart);
            mText.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Stretch;
            mText.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Stretch;
            mText.TextAlignment = Microsoft.SPOT.Presentation.Media.TextAlignment.Left;
            mText.TextWrap = true;

            // Add the text control to the window.
            mMainWindow.Child = mText;

            // Set the window visibility to visible.
            mMainWindow.Visibility = Visibility.Visible;

            return mMainWindow;
        }
    }
}
