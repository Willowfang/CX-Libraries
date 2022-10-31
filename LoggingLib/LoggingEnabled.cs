using LoggingLib.Defaults;

namespace CX.LoggingLib
{
    /// <summary>
    /// A base class for classes that have logging capabilites.
    /// </summary>
    public abstract class LoggingEnabled
    {
        /// <summary>
        /// The logging service to record logs with.
        /// </summary>
        protected ILogbook logbook;

        /// <summary>
        /// Create a new logging enabled instance.
        /// </summary>
        /// <param name="logbook">The logging service to use. If null, logs will not be recorded
        /// anywhere.</param>
        public LoggingEnabled(ILogbook logbook)
        {
            if (logbook == null)
                this.logbook = EmptyLogbook.Create();
            else
                this.logbook = logbook;
        }

        /// <summary>
        /// Replace the current logging service with another service.
        /// </summary>
        /// <param name="logbook">Logging service to replace the previous with.</param>
        public virtual void ChangeLogbook(ILogbook logbook)
        {
            this.logbook = logbook;
        }
    }

    /// <summary>
    /// A base class for classes implementing typed logging.
    /// </summary>
    /// <typeparam name="T">Type of the implementing class.</typeparam>
    public abstract class LoggingEnabled<T>
    {
        /// <summary>
        /// Service used for logging.
        /// </summary>
        protected TypedLogbook<T> logbook;

        /// <summary>
        /// Create a new instance of a logging enabled class.
        /// </summary>
        /// <param name="logbook">Logging service to use. If null, will not log anywhere.</param>
        public LoggingEnabled(ILogbook logbook)
        {
            if (logbook == null)
                this.logbook = EmptyLogbook.Create<T>();
            else
                this.logbook = logbook.CreateTyped<T>();
        }

        /// <summary>
        /// Replace current logging service with another.
        /// </summary>
        /// <param name="logbook">Logging service to replace with.</param>
        public virtual void ChangeLogbook(ILogbook logbook)
        {
            this.logbook = logbook.CreateTyped<T>();
        }
    }
}
