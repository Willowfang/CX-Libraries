using CX.LoggingLib;
using System.Runtime.CompilerServices;

namespace LoggingLib.Defaults
{
    /// <summary>
    /// An <see cref="ILogbook"/> that records logs nowhere
    /// </summary>
    public class EmptyLogbook : Logbook
    {
        private EmptyLogbook() { }

        /// <summary>
        /// Create a non-recording logger
        /// </summary>
        /// <returns></returns>
        public static ILogbook Create()
        {
            return new EmptyLogbook();
        }

        /// <summary>
        /// Create a typed, non-recording logger
        /// </summary>
        /// <typeparam name="T">Type of the containing class</typeparam>
        /// <returns></returns>
        public static TypedLogbook<T> Create<T>()
        {
            return new EmptyLogbook<T>(Create());
        }

        public override void Write(string message, LogLevel level, Exception? exception = null,
            string? callerName = "", [CallerMemberName] string callerMemberName = "", params object[]? customContent) { }
    }

    /// <summary>
    /// A typed <see cref="ILogbook"/> that records nowhere
    /// </summary>
    /// <typeparam name="TCaller">Type of the containing class</typeparam>
    public class EmptyLogbook<TCaller> : TypedLogbook<TCaller>
    {
        internal EmptyLogbook(ILogbook log)
            : base(log) { }
    }
}
