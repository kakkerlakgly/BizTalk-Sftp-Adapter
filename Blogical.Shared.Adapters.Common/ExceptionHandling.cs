using System;
using System.Text;
using System.Diagnostics;
using System.Security.Permissions;

namespace Blogical.Shared.Adapters.Common
{
    /// <summary>
    /// Contains functions to handle exceptions.
    /// </summary>
    public static class ExceptionHandling
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodId">String that identifies the method.</param>
        /// <param name="ex">The exception that occured.</param>
        /// <returns></returns>
        public static Exception HandleComponentException(string methodId, Exception ex)
        {
            CreateEventLogMessage(ex, methodId, null);
            return ex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="methodId"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception HandleComponentException(int eventId, string methodId, Exception ex)
        {
            CreateEventLogMessage(ex, methodId, null,eventId);
            return ex;
        }

        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodBase">Received by new StackFrame().GetMethod();
        /// <code>Sample: throw Blogical.Shared.Adapters.Common.ExceptionHandling.HandleComponentException(System.Reflection.MethodBase.GetCurrentMethod(), ex);</code>
        /// </param>
        /// <param name="ex"></param>
        /// <returns>The exception that occured.</returns>
        public static Exception HandleComponentException(System.Reflection.MethodBase methodBase, Exception ex)
        {
            string s = methodBase.DeclaringType.FullName + "." + methodBase.Name;
            return HandleComponentException(methodBase.DeclaringType.FullName + "." + methodBase.Name, ex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="methodBase"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception HandleComponentException(int eventId, System.Reflection.MethodBase methodBase, Exception ex)
        {
            string s = methodBase.DeclaringType.FullName + "." + methodBase.Name;
            return HandleComponentException(eventId, methodBase.DeclaringType.FullName + "." + methodBase.Name, ex);
        }

        public static void CreateEventLogMessage(Exception ex, string methodId, string messageName)
        {
            CreateEventLogMessage(CreateLogMessage(ex, methodId, messageName), 0, 0, EventLogEntryType.Error);
        }

        public static void CreateEventLogMessage(Exception ex, string methodId, string messageName, int eventId)
        {
            CreateEventLogMessage(CreateLogMessage(ex, methodId, messageName), eventId, 0, EventLogEntryType.Error);
        }

        public static void CreateEventLogMessage(Exception ex, string methodId, string messageName, int eventId, EventLogEntryType entryType)
        {
            CreateEventLogMessage(CreateLogMessage(ex, methodId, messageName), eventId, 0, entryType);
        }

        [EventLogPermission(SecurityAction.Demand, PermissionAccess=EventLogPermissionAccess.Write)]
        public static void CreateEventLogMessage(string message,
            int eventId, short category, EventLogEntryType entryType)
        {
            EventLog eventLog = new EventLog {Source = EventLogSources.SftpAdapter};
            eventLog.WriteEntry(message, entryType, eventId, category);
        }

        /// <summary>
        /// Creates a message to write to event-log.
        /// </summary>
        /// <param name="ex">The Exception to log</param>
        /// <param name="methodId">Name of the method that handled this exception.</param>
        /// <param name="messageName">The name of the message that caused the exception. (null if not available)</param>
        /// <returns>A string to write to the event-log</returns>
        private static string CreateLogMessage(Exception ex, string methodId, string messageName) {
            
            StringBuilder message = new StringBuilder();
            // Add info about log-entry
            message.AppendFormat("Method: {0}\r\n", methodId);
            if (ex != null)
                message.AppendFormat("Error: {0}\r\n", ex.Message);
            if (messageName != null)
                message.AppendFormat("Message name: {0}\r\n", messageName);
            // Add info about exception
            message.Append("\r\n------------------------------\r\nInformation:\r\n");
            message.Append(ExceptionMessage(ex));
            // Add info about inner exceptions
            Exception e = ex;
            while (e.InnerException != null)
            {
                message.Append("------------------------------\r\n");
                message.Append(ExceptionMessage(e.InnerException));
                message.Append("\r\n");
                e = e.InnerException;
            }
            // Return message
            return message.ToString();
        }

        /// <summary>
        /// Creates a message describing an Exception.
        /// </summary>
        /// <param name="ex">The exception to describe</param>
        /// <returns>A message describing an Exception.</returns>
        private static string ExceptionMessage(Exception ex)
        {
            // Add exception info
            StringBuilder message = new StringBuilder();
            message.AppendFormat("Type: {0}\r\nTarget: {1}\r\nMessage: {2}\r\nStacktrace:\r\n{3}\r\n\r\n",
                                    ex.GetType().FullName,
                                    ex.TargetSite,
                                    ex.Message,
                                    ex.StackTrace);

            if (ex.Data.Count > 0)
            {
                // Add data info
                message.Append("Data:\r\n");
                foreach (System.Collections.DictionaryEntry item in ex.Data)
                {
                    message.AppendFormat("{0}:{1}\r\n", item.Key, item.Value);
                }
            }
            // Return message
            return message.ToString();
        }

    }
}
