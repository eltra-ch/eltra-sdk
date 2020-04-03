﻿using EltraNotKauf.Controls.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EltraNotKauf.Views.Contact
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContactView : ContentView
    {
        public ContactView()
        {
            InitializeComponent();

            var entries = new List<Entry>();
            
            UwpHelper.DeepSearch(Children.ToList(), ref entries);
            
            UwpHelper.FixElements(entries);
        }        
    }
}