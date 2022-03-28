using LoggingLib.Defaults;

namespace CX.LoggingLib
{
    /// <summary>
    /// A base class for classes that have logging capabilites
    /// </summary>
    public abstract class LoggingEnabled
    {
        protected ILogbook logbook;

        public LoggingEnabled(ILogbook logbook)
        {
            if (logbook == null)
                this.logbook = EmptyLogbook.Create();
            else
                this.logbook = logbook;
        }

        public virtual void ChangeLogbook(ILogbook logbook)
        {
            this.logbook = logbook;
        }
    }

    public abstract class LoggingEnabled<T>
    {
        protected TypedLogbook<T> logbook;

        public LoggingEnabled(ILogbook logbook)
        {
            if (logbook == null)
                this.logbook = EmptyLogbook.Create<T>();
            else
                this.logbook = logbook.CreateTyped<T>();
        }

        public virtual void ChangeLogbook(ILogbook logbook)
        {
            this.logbook = logbook.CreateTyped<T>();
        }
    }
}
