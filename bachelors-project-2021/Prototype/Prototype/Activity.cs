using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public class Activity
    {
        /// <summary>
        ///     Title of the activity
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        ///     Source for the activity image/emoji
        /// </summary>
        public string? ImageSource { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Activity other)
            {
                return string.Equals(Title, other.Title, StringComparison.Ordinal) &&
                       string.Equals(ImageSource, other.ImageSource, StringComparison.Ordinal);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, ImageSource);
        }
    }
}
