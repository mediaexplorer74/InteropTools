﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Intense.Presentation
{
    /// <summary>
    /// An <see cref="ICommand"/> implementation that relays its functionality by invoking delegates.
    /// </summary>
    public class RelayCommand
        : Command
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The delegate performing the command execution.</param>
        /// <param name="canExecute">The optional delegate determining whether command execution is enabled.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute));
            }
            this.execute = o => execute();
            if (canExecute != null) {
                this.canExecute = o => canExecute();
            }
            else {
                this.canExecute = o => true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute delegate accepting a command parameter.</param>
        /// <param name="canExecute">The optional delegate determining whether command execution is enabled.</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute));
            }
            this.execute = execute;
            this.canExecute = canExecute ?? (o => true);
        }
        
        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns></returns>
        public override bool CanExecute(object parameter)
        {
            return this.canExecute(parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter)) {
                return;
            }
            this.execute(parameter);
        }
    }
}
