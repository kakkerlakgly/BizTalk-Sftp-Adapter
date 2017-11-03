using System;
using System.Threading;
using System.Xml;

namespace Blogical.Shared.Adapters.Common.Schedules
{
    /// <summary>
    /// Daily Schedule class supporting  Microsoft.Biztalk.Scheduler.ISchedule interface.
    /// Allows scheduling by interval (e.g. every 3 days)  or by  weekday (e.g. on Mondays and Fridays)
    /// </summary>
    [Serializable()]
    public class TimeSchedule : Schedule
    {
        //Fields
		private int _interval = 0;					//polling interval
        private object _scheduleTime = 0;			//hours, minutes, seconds
		
        // Properties
        /// <summary>
        /// The number of units between polling request
        /// </summary>
        public int Interval 
		{
			get
			{
                return _interval;
			}
			set
			{
                if (value != Interlocked.Exchange(ref _interval, value))
				{
					FireChangedEvent();
				}
			}
		}
        /// <summary>
        /// Time Unit definition
        /// </summary>
        public ScheduleTimeType ScheduleTime 
		{
			get
			{
                return (ScheduleTimeType)_scheduleTime;
			}
			set
			{
                if (value != (ScheduleTimeType)Interlocked.Exchange(ref _scheduleTime, value))
				{
					FireChangedEvent();
				}
			}
		}
        long totalNumdebrOfSeconds
        {
            get 
            {
                switch (ScheduleTime)
                { 
                    case ScheduleTimeType.Hours:
                        return _interval * 3600;
                    case ScheduleTimeType.Minutes:
                        return _interval*60;
                    default:
                        return _interval;
                }
            }
        }

		//Methods
        /// <summary>
        /// Constructor
        /// </summary>
		public TimeSchedule()
		{
		}
        /// <summary>
        /// Constructor
        /// </summary>
        public TimeSchedule(string configxml)
		{
			XmlDocument configXml = new XmlDocument();
			configXml.LoadXml(configxml);
			type = ExtractScheduleType(configXml);

			if (type != ScheduleType.Timely)
			{
				throw (new ApplicationException("Invalid Configuration Type"));
			}
			StartDate = ExtractDate(configXml, "/schedule/startdate", true);
			StartTime = ExtractTime(configXml, "/schedule/starttime", true);
			
			_interval = IfExistsExtractInt(configXml, "/schedule/interval", 0);
            ScheduleTime = ExtractScheduleTimeType(configXml, "/schedule/timeintervalltype", true);

            //if (this.Interval == 0)
            //{
            //    this.ScheduleTime = ExtractScheduleTimeType(configXml, "/schedule/timeintervalltype", true);
            //}
		}
        /// <summary>
        /// Returns the next time the schedule will be triggerd
        /// </summary>
        /// <returns></returns>
		public override DateTime GetNextActivationTime()
		{
            TraceMessage("[TimeSchedule]Executing GetNextActivationTime");
            if (Interval == 0)
			{
				throw(new ApplicationException("Uninitialized timely schedule")); 
			}

            return DateTime.Now.AddSeconds(totalNumdebrOfSeconds);

		}
    }
}
