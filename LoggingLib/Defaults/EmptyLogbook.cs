using System.Runtime.CompilerServices;

namespace WF.LoggingLib.Defaults
{
    /// <summary>
    /// An <see cref="ILogbook"/> that records logs nowhere. Used to fill dependencies requiring logging
    /// services when there is no actual service to fill them.
    /// </summary>
    public class EmptyLogbook : Logbook
    {
        private EmptyLogbook() { }

        /// <summary>
        /// Create a non-recording logger without a type.
        /// </summary>
        /// <returns>Empty logger.</returns>
        public static ILogbook Create()
        {
            return new EmptyLogbook();
        }

        /// <summary>
        /// Create a typed, non-recording logger.
        /// </summary>
        /// <typeparam name="T">Type of class that uses this logger.</typeparam>
        /// <returns>Empty typed logger.</returns>
        public static TypedLogbook<T> Create<T>()
        {
            return new EmptyLogbook<T>(Create());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="level"></param>
        public override void ChangeLevel(LogLevel level) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="message"><inheritdoc/></param>
        /// <param name="level"><inheritdoc/></param>
        /// <param name="exception"><inheritdoc/></param>
        /// <param name="callerName"><inheritdoc/></param>
        /// <param name="callerMemberName"><inheritdoc/></param>
        /// <param name="customContent"><inheritdoc/></param>
        public override void Write(string message, LogLevel level, Exception? exception = null,
            string? callerName = "", [CallerMemberName] string callerMemberName = "", params object[]? customContent)
        { }
    }

    /// <summary>
    /// A typed <see cref="ILogbook"/> that records nowhere. Used to fill dependencies requiring logging
    /// services when there is no actual service to fill them.
    /// </summary>
    /// <typeparam name="TCaller">Type of the class that uses this logger.</typeparam>
    public class EmptyLogbook<TCaller> : TypedLogbook<TCaller>
    {
        internal EmptyLogbook(ILogbook log)
            : base(log) { }
    }
}
