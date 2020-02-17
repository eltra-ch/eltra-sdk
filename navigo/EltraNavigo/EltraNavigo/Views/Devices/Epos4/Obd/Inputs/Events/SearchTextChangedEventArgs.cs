using System;

namespace EltraNavigo.Views.Obd.Inputs.Events
{
    public class SearchTextChangedEventArgs : EventArgs
    {
        public SearchTextChangedEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}
