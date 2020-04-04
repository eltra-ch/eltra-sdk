using System;

namespace EltraNotKauf.Helpers
{
    [AttributeUsage(System.AttributeTargets.All)]
    public class PreserveAttribute : Attribute
    {
        public PreserveAttribute() { }
        public bool AllMembers { get; set; }
        public bool Conditional { get; set; }
    }
}
