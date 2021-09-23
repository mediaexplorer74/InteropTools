﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intense.Presentation
{
    /// <summary>
    /// Provides a base implementation for objects that are displayed in the UI.
    /// </summary>
    public class Displayable
        : NotifyPropertyChanged
    {
        private string displayName;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName
        {
            get { return this.displayName; }
            set
            {
                if (Set(ref this.displayName, value)) {
                    OnPropertyChanged("DisplayNameUppercase");
                }
            }
        }

        /// <summary>
        /// Get the uppercase variant of the display name.
        /// </summary>
        public string DisplayNameUppercase
        {
            get { return this.displayName?.ToUpper(); }
        }

        /// <summary>
        /// Gets a string representation of this instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.displayName;
        }
    }
}
