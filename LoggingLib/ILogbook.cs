using System.Runtime.CompilerServices;

namespace WF.LoggingLib
{
    /// <summary>
    /// Logging level for messages.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Logs all messages. Should only be on when debugging.
        /// </summary>
        Debug,
        /// <summary>
        /// Logs everything except Debug messages. Normal level.
        /// </summary>
        Information,
        /// <summary>
        /// Only logs Warning, Error and Fatal messages.
        /// </summary>
        Warning,
        /// <summary>
        /// Only logs Error and Fatal messages.
        /// </summary>
        Error,
        /// <summary>
        /// Only logs Fatal messages.
        /// </summary>
        Fatal
    }

    /// <summary>
    /// Service for logging messages to a message log.
    /// </summary>
    public interface ILogbook
    {
        /// <summary>
        /// Change the logging level of the logger.
        /// </summary>
        /// <param name="level"></param>
        public void ChangeLevel(LogLevel level);

        /// <summary>
        /// Create a typed version of this <see cref="ILogbook"/>. A typed version provides assigned type as 
        /// extra information when writing messages.
        /// </summary>
        /// <typeparam name="T">Type of the class to use this logger.</typeparam>
        /// <returns>A typed logger.</returns>
        public TypedLogbook<T> CreateTyped<T>();

        /// <summary>
        /// Log message and info.
        /// </summary>
        /// <param name="message">Message to write into log.</param>
        /// <param name="level">Logging level for the message.</param>
        /// <param name="exception">Optional exception in case of errors.</param>
        /// <param name="customContent">Custom extra info.</param>
        /// <param name="callerName">Name of the calling class.</param>
        /// <param name="callerMemberName">Name of the calling method.</param>
        public void Write(string message, LogLevel level, Exception? exception = null,
            string? callerName = "", [CallerMemberName] string callerMemberName = "", params object[]? customContent);
    }

    /// <summary>
    /// Default abstract base class for <see cref="ILogbook"/>.
    /// </summary>
    public abstract class Logbook : ILogbook
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="level"></param>
        public abstract void ChangeLevel(LogLevel level);

        /// <summary>
        /// Create a typed <see cref="ILogbook"/> with given type as container. <see cref="TypedLogbook{T}"/> 
        /// provides the name of <typeparamref name="T"/> as extra information when logging.
        /// </summary>
        /// <typeparam name="T">Type of the class to use this logger.</typeparam>
        /// <returns></returns>
        public virtual TypedLogbook<T> CreateTyped<T>()
        {
            return new TypedLogbook<T>(this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="exception"></param>
        /// <param name="callerName"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="customContent"></param>
        public abstract void Write(string message, LogLevel level, Exception? exception = null,
            string? callerName = "", [CallerMemberName] string callerMemberName = "", params object[]? customContent);
    }

    /// <summary>
    /// An <see cref="ILogbook"/> with info on the containing class.
    /// </summary>
    /// <typeparam name="T">Containing class type.</typeparam>
    public class TypedLogbook<T>
    {
        private readonly ILogbook logbook;

        /// <summary>
        /// Create a logger with information on the containing class.
        /// </summary>
        /// <param name="logbook"><see cref="ILogbook"/> to create the typed version from.</param>
        public TypedLogbook(ILogbook logbook)
        {
            this.logbook = logbook;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="message">Message to write into log.</param>
        /// <param name="level">Logging level for the message.</param>
        /// <param name="exception">Optional exception in case of errors.</param>
        /// <param name="callerMemberName">Name of the calling method.</param>
        /// <param name="customContent">Custom info for logging</param>
        public virtual void Write(string message, LogLevel level, Exception? exception = null,
            [CallerMemberName] string callerMemberName = "", params object[]? customContent)
        {
            logbook.Write(message, level, exception, typeof(T).FullName, callerMemberName, customContent);
        }

        /// <summary>
        /// Get the underlying <see cref="ILogbook"/> instance.
        /// </summary>
        public ILogbook BaseLogbook
        {
            get => logbook;
        }
    }
}