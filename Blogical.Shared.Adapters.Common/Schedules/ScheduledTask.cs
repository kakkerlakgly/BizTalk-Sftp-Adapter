using System;
using Microsoft.BizTalk.Scheduler;

namespace Blogical.Shared.Adapters.Common.Schedules
{
	/// <summary>
	///  ScheduledTask Class implementing the Microsoft.BizTalk.Scheduler.ITask interface.
	/// </summary>
	public class ScheduledTask : ITask
	{
		// Events

        /// <summary>
        /// Triggerd on any task event
        /// </summary>
		public event TaskProgressHandler Progress;
        /// <summary>
        /// Event delegate
        /// </summary>
		public delegate void TaskDelegate();
		// Fields
	    private readonly TaskDelegate _taskDelegate;
		// Properties
        /// <summary>
        /// Allways false
        /// </summary>
		public bool CanPause
		{
			get
			{
				return false;
			}
		}
        /// <summary>
        /// Allways true
        /// </summary>
		public bool CanStop
		{
			get
			{
				return true;
			}
		}
        /// <summary>
        /// Describes task (not used)
        /// </summary>
		public string Description
		{
			get
			{
				return "";
			}
		}
        /// <summary>
        /// Name of task == URI
        /// </summary>
		public string Name { get; }

	    // Methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="taskDelegate"></param>
		public ScheduledTask(string name, TaskDelegate taskDelegate)
		{
			this.Name = name;
			this._taskDelegate = taskDelegate;
		}

		private void FireProgress(TaskProgress progress)
		{
		    Progress?.Invoke(this, new TaskProgressEventArgs(progress));
		}
        /// <summary>
        /// Pauses the location
        /// </summary>
		public void Pause()
		{
		}
        /// <summary>
        /// Resumes execution
        /// </summary>
		public void Resume()
		{
		}
        /// <summary>
        /// Starts task
        /// </summary>
		public void Start()
		{
			try
			{
				FireProgress(TaskProgress.Started);
				_taskDelegate();
				FireProgress(TaskProgress.Succeeded);
			}
			catch (Exception)
			{
				FireProgress(TaskProgress.Failed);
			}
		}
        /// <summary>
        /// Stops execution of task
        /// </summary>
		public void Stop()
		{
		}
	}
}
