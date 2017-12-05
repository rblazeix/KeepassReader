// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelayCommand.cs" company="Mar3ek">
//   This code is licensed under the Mozilla Public License 2.0.
//   You can find the full license text at the following address:
//   https://winkee.codeplex.com/license
// </copyright>
// <summary>
//   A command whose sole purpose is to relay its functionality
//   to other objects by invoking delegates.
//   The default return value for the CanExecute method is 'true'.
//   <see cref="RaiseCanExecuteChanged" /> needs to be called whenever
//   <see cref="CanExecute" /> is expected to return a different value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KeePassW10.Common
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// A command whose sole purpose is to relay its functionality 
    /// to other objects by invoking delegates. 
    /// The default return value for the CanExecute method is 'true'.
    /// <see cref="RaiseCanExecuteChanged"/> needs to be called whenever
    /// <see cref="CanExecute"/> is expected to return a different value.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        /// <summary>
        /// The _can execute.
        /// </summary>
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// The _execute.
        /// </summary>
        private readonly Action _execute;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class. 
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">
        /// The execution logic.
        /// </param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class. 
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">
        /// The execution logic.
        /// </param>
        /// <param name="canExecute">
        /// The execution status logic.
        /// </param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this._execute = execute;
            this._canExecute = canExecute;
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Raised when RaiseCanExecuteChanged is called.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Determines whether this <see cref="RelayCommand"/> can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            return this._canExecute == null ? true : this._canExecute();
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand"/> on the current command target.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            this._execute();
        }

        /// <summary>
        /// Method used to raise the <see cref="CanExecuteChanged"/> event
        /// to indicate that the return value of the <see cref="CanExecute"/>
        /// method has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}