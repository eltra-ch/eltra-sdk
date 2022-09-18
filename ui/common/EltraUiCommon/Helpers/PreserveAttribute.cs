using System;

namespace EltraWpfCommon.Helpers
{
    [AttributeUsage(AttributeTargets.All)]
    public class PreserveAttribute : Attribute
    {
        public PreserveAttribute() { }
        public bool AllMembers { get; set; }
        public bool Conditional { get; set; }
    }
}
