using System;
using System.Collections.Generic;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BlindsUp
{
    class ChopEntry
    {
        public int playerIndex;
        public StackLayout container;
        private Entry stackEntry;
        private Label playerLabel;
        private Label valueLabel;
        

        // stackLayout,consists of a stack entry, label for playerName, label for chop value, checkbox
        public ChopEntry(string playerName, int index)
        {
            playerIndex = index;
            stackEntry = new Entry
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                WidthRequest = 80,
                Placeholder = "stack"
            };

            playerLabel = new Label
            {
                Text = playerName,
                HorizontalOptions = LayoutOptions.Start,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                WidthRequest = 100,
            };

            valueLabel = new Label
            {
                Text = " ",
                HorizontalOptions = LayoutOptions.Start,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                FontAttributes = FontAttributes.Bold,
                WidthRequest = 120,
            };

         

            container = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,

                Children =
                { stackEntry, playerLabel, valueLabel }
            };

        }
        public void setChopString( string chopString)
        {
            valueLabel.Text = chopString;
        }
        
        public void setVisible( bool onoff)
        {
            container.IsVisible = onoff;
        }
        public string getStackString()
        {
            return stackEntry.Text;
        }
    }
}
