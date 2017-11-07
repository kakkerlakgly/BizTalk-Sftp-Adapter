using System;
using System.Windows.Forms;
using System.Xml;

namespace Blogical.Shared.Adapters.Common.Schedules.UI
{
	/// <summary>
	/// ScheduleDialog: dialog for selecting schedule type and parameters
	/// </summary>
	public partial class ScheduleDialog : Form
	{
	    /// <summary>
        /// Schedule properties
        /// </summary>
		public XmlDocument ConfigXml { get; private set; }
        
	    /// <summary>
        /// Constructor
        /// </summary>
		public ScheduleDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
			ConfigXml = new XmlDocument();
			
			//default
			tabPages.SelectedTab = tabDaily;
			radioDayofMonth.Checked = true;
			radioDayInterval.Checked = true;
			DateTime now =   DateTime.Now;
			dateStart.Value = now;
			timeStart.Value = new DateTime(now.Year, now.Month, now.Day, 0,0,0);
		}


		//
		// Load ConfigXml into controls
		//
		private void ScheduleDialog_Load(object sender, EventArgs e)
		{
			if (ConfigXml.ChildNodes.Count > 0)
			{
				ScheduleType scheduleType = Schedule.IfExistsExtractScheduleType(ConfigXml);
				dateStart.Value = Schedule.IfExistsExtractDate(ConfigXml, "/schedule/startdate", DateTime.Now);
				timeStart.Value = Schedule.IfExistsExtractTime(ConfigXml, "/schedule/starttime", DateTime.Now);

				switch(scheduleType)
				{
					case ScheduleType.Daily:
						LoadDailySchedule();
						break;
					case ScheduleType.Weekly:
						LoadWeeklySchedule();
						break;
					case ScheduleType.Monthly:
						LoadMonthlySchedule();
						break;
                    case ScheduleType.Timely:
                        LoadTimelySchedule();
                        break;
					default:
						ConfigXml = new XmlDocument();
						break;
				}
			}
		}
		private void LoadDailySchedule()
		{
			tabPages.SelectedTab = tabDaily;
			dayInterval.Value =Convert.ToDecimal( Schedule.IfExistsExtractInt(ConfigXml, "/schedule/interval", 0));
			if (dayInterval.Value == 0)
			{
				ScheduleDay days = Schedule.ExtractScheduleDay(ConfigXml, "/schedule/days", false);
				if ((days & ScheduleDay.Sunday) > 0) daySunday.Checked = true;
				if ((days & ScheduleDay.Monday) > 0) dayMonday.Checked = true;
				if ((days & ScheduleDay.Tuesday) > 0) dayTuesday.Checked = true;
				if ((days & ScheduleDay.Wednesday) > 0) dayWednesday.Checked = true;
				if ((days & ScheduleDay.Thursday) > 0) dayThursday.Checked = true;
				if ((days & ScheduleDay.Friday) > 0) dayFriday.Checked = true;
				if ((days & ScheduleDay.Saturday) > 0) daySaturday.Checked = true;
				radioDayInterval.Checked = false;
			}
			else
			{
				radioDayInterval.Checked = true;
			}
		}
		private void LoadWeeklySchedule()
		{
			tabPages.SelectedTab = tabWeekly;
			weekInterval.Value = Convert.ToDecimal(Schedule.IfExistsExtractInt(ConfigXml, "/schedule/interval", 1));
			ScheduleDay days = Schedule.ExtractScheduleDay(ConfigXml, "/schedule/days", false);
			if ((days & ScheduleDay.Sunday) > 0) weekSunday.Checked = true;
			if ((days & ScheduleDay.Monday) > 0) weekMonday.Checked = true;
			if ((days & ScheduleDay.Tuesday) > 0) weekTuesday.Checked = true;
			if ((days & ScheduleDay.Wednesday) > 0) weekWednesday.Checked = true;
			if ((days & ScheduleDay.Thursday) > 0) weekThursday.Checked = true;
			if ((days & ScheduleDay.Friday) > 0) weekFriday.Checked = true;
			if ((days & ScheduleDay.Saturday) > 0) weekSaturday.Checked = true;
		}
		private void LoadMonthlySchedule()
		{
			tabPages.SelectedTab = tabMonthly;
			_dayofmonth.Value = Convert.ToDecimal(Schedule.IfExistsExtractInt(ConfigXml, "/schedule/dayofmonth", 0));
			if (_dayofmonth.Value == 0)
			{
				ScheduleOrdinal ordinal = Schedule.ExtractScheduleOrdinal(ConfigXml, "/schedule/ordinal", false);
				int index = ordinalDropDown.Items.IndexOf(ordinal.ToString());
				ordinalDropDown.SelectedIndex = index;
				ScheduleDay weekDay = Schedule.ExtractScheduleDay(ConfigXml, "/schedule/weekday", false);
				string strWeekday = weekDay.ToString();
				if (strWeekday == "All"){strWeekday = "Day";}
				index = weekdayDropDown.Items.IndexOf(strWeekday);
				weekdayDropDown.SelectedIndex = index;
				radioDayofMonth.Checked = false;
			}
			else
			{
				radioDayofMonth.Checked = true;
			}
			ScheduleMonth months =Schedule. ExtractScheduleMonth(ConfigXml, "/schedule/months", false);
			if ((months & ScheduleMonth.February) > 0) monthFebruary.Checked = true;
			if ((months & ScheduleMonth.March) > 0) monthMarch.Checked = true;
			if ((months & ScheduleMonth.April) > 0) monthApril.Checked = true;
			if ((months & ScheduleMonth.May) > 0) monthMay.Checked = true;
			if ((months & ScheduleMonth.June) > 0) monthJune.Checked = true;
			if ((months & ScheduleMonth.July) > 0) monthJuly.Checked = true;
			if ((months & ScheduleMonth.August) > 0) monthAugust.Checked = true;
			if ((months & ScheduleMonth.September) > 0) monthSeptember.Checked = true;
			if ((months & ScheduleMonth.October) > 0) monthOctober.Checked = true;
			if ((months & ScheduleMonth.November) > 0) monthNovember.Checked = true;
			if ((months & ScheduleMonth.December)> 0) monthDecember.Checked = true;
		}
        private void LoadTimelySchedule()
        {
            tabPages.SelectedTab = tabTimely;
            timeInterval.Value = Convert.ToDecimal(Schedule.IfExistsExtractInt(ConfigXml, "/schedule/interval", 0));

            ScheduleTimeType scheduleTimeType = Schedule.ExtractScheduleTimeType(ConfigXml, "/schedule/timeintervalltype", false);

            if (scheduleTimeType == ScheduleTimeType.Seconds) 
                timeType.Text = "Seconds";
            if (scheduleTimeType == ScheduleTimeType.Minutes)
                timeType.Text="Minutes";
            if (scheduleTimeType == ScheduleTimeType.Hours) 
                timeType.Text = "Hours";
            

        }
		//
		// Unload controls into ConfigXml
		//
		private void ScheduleDialog_UnLoad(object sender, EventArgs e)
		{
			ConfigXml = new XmlDocument();
			XmlNode root = ConfigXml.CreateNode("element", "schedule", "");
			XmlNode startdate = ConfigXml.CreateNode("element", "startdate", "");
			XmlNode starttime = ConfigXml.CreateNode("element", "starttime", "");
			startdate.InnerText = dateStart.Value.ToString("yyyy-MM-dd");
			starttime.InnerText = timeStart.Value.ToString("HH:mm");
			root.AppendChild(startdate);
			root.AppendChild(starttime);
			ConfigXml.AppendChild(root);
			
			switch(tabPages.SelectedTab.Text)
			{
				case "Daily":
					UnloadDailySchedule();
					break;
				case "Weekly":
					UnloadWeeklySchedule();
					break;
				case "Monthly":
					UnloadMonthlySchedule();
					break;
                case "Timely":
                    UnloadTimelySchedule();
                    break;
			}	
		}
		private void UnloadDailySchedule()
		{
			ConfigXml.DocumentElement.SetAttribute("type", "", "Daily");
			if (radioDayInterval.Checked)
			{
				if (dayInterval.Value == 0)
				{
					throw(new ApplicationException("Must select a daily interval"));
				}
				XmlNode interval = ConfigXml.CreateNode("element", "interval","");
				interval.InnerText = dayInterval.Value.ToString();
				ConfigXml.DocumentElement.AppendChild(interval);
			}
			else
			{
				ScheduleDay result = ScheduleDay.None;
				if (daySunday.Checked) {result = result | ScheduleDay.Sunday;}
				if (dayMonday.Checked) {result = result | ScheduleDay.Monday;}
				if (dayTuesday.Checked) {result = result | ScheduleDay.Tuesday;}
				if (dayWednesday.Checked) {result = result | ScheduleDay.Wednesday;}
				if (dayThursday.Checked) {result = result | ScheduleDay.Thursday;}
				if (dayFriday.Checked) {result = result | ScheduleDay.Friday;}
				if (daySaturday.Checked) {result = result | ScheduleDay.Saturday;}
				if (result == ScheduleDay.None)
				{
					throw(new ApplicationException("Must select one or more days of the week"));
				}
				else
				{
					XmlNode days = ConfigXml.CreateNode("element", "days","");
					days.InnerText = result.ToString();
					ConfigXml.DocumentElement.AppendChild(days);
				}
			}
		}
		private void UnloadWeeklySchedule()
		{
			ConfigXml.DocumentElement.SetAttribute("type", "", "Weekly");
			XmlNode interval = ConfigXml.CreateNode("element", "interval","");
			interval.InnerText = weekInterval.Value.ToString();
			ConfigXml.DocumentElement.AppendChild(interval);

			ScheduleDay result = ScheduleDay.None;
			if (weekSunday.Checked) {result = result | ScheduleDay.Sunday;}
			if (weekMonday.Checked) {result = result | ScheduleDay.Monday;}
			if (weekTuesday.Checked) {result = result | ScheduleDay.Tuesday;}
			if (weekWednesday.Checked) {result = result | ScheduleDay.Wednesday;}
			if (weekThursday.Checked) {result = result | ScheduleDay.Thursday;}
			if (weekFriday.Checked) {result = result | ScheduleDay.Friday;}
			if (weekSaturday.Checked) {result = result | ScheduleDay.Saturday;}
			if (result == ScheduleDay.None)
			{
				throw(new ApplicationException("Must select one or more days of the week"));
			}
			else
			{
				XmlNode days = ConfigXml.CreateNode("element", "days","");
				days.InnerText = result.ToString();
				ConfigXml.DocumentElement.AppendChild(days);
			}
		}
		private void UnloadMonthlySchedule()
		{
			ConfigXml.DocumentElement.SetAttribute("type", "", "Monthly");
			if (radioDayofMonth.Checked)
			{
				if (_dayofmonth.Value == 0)
				{
					throw(new ApplicationException("Must select a day of the month"));
				}
				XmlNode dayofmonth = ConfigXml.CreateNode("element", "dayofmonth","");
				dayofmonth.InnerText = _dayofmonth.Value.ToString();
				ConfigXml.DocumentElement.AppendChild(dayofmonth);
			}
			else
			{
				if (ordinalDropDown.SelectedItem == null)
				{
					throw(new ApplicationException("Must select an ordinal day"));
				}
				XmlNode ordinal = ConfigXml.CreateNode("element", "ordinal","");
				ordinal.InnerText = ordinalDropDown.SelectedItem.ToString();
				ConfigXml.DocumentElement.AppendChild(ordinal);
				
				if (weekdayDropDown.SelectedItem == null)
				{
					throw(new ApplicationException("Must select an ordinal week day"));
				}
				XmlNode weekday = ConfigXml.CreateNode("element", "weekday","");
				weekday.InnerText = weekdayDropDown.SelectedItem.ToString();
				if (weekday.InnerText == "Day"){	weekday.InnerText = "All";}
				ConfigXml.DocumentElement.AppendChild(weekday);

			}
			ScheduleMonth result = ScheduleMonth.None;
			if (monthJanuary.Checked) {result = result | ScheduleMonth.January;}
			if (monthFebruary.Checked) {result = result | ScheduleMonth.February;}
			if (monthMarch.Checked) {result = result | ScheduleMonth.March;}
			if (monthApril.Checked) {result = result | ScheduleMonth.April;}
			if (monthMay.Checked) {result = result | ScheduleMonth.May;}
			if (monthJune.Checked) {result = result | ScheduleMonth.June;}
			if (monthJuly.Checked) {result = result | ScheduleMonth.July;}
			if (monthAugust.Checked) {result = result | ScheduleMonth.August;}
			if (monthSeptember.Checked) {result = result | ScheduleMonth.September;}
			if (monthOctober.Checked) {result = result | ScheduleMonth.October;}
			if (monthNovember.Checked) {result = result | ScheduleMonth.November;}
			if (monthDecember.Checked) {result = result | ScheduleMonth.December;}
			if (result == ScheduleMonth.None)
			{
				throw(new ApplicationException("Must select one or more months"));
			}
			else
			{
				XmlNode months = ConfigXml.CreateNode("element", "months","");
				months.InnerText = result.ToString();
				ConfigXml.DocumentElement.AppendChild(months);
			}
		}
        private void UnloadTimelySchedule()
        {
            ConfigXml.DocumentElement.SetAttribute("type", "", "Timely");

            XmlNode intervalNode = ConfigXml.CreateNode("element", "interval", "");
            intervalNode.InnerText = timeInterval.Value.ToString();
            ConfigXml.DocumentElement.AppendChild(intervalNode);
        
            ScheduleTimeType result;
            switch (timeType.Text.ToUpper())
            {
                case "HOURS":
                    result = ScheduleTimeType.Hours;
                    break;
                case "MINUTES":
                    result = ScheduleTimeType.Minutes;
                    break;
                default:
                    result = ScheduleTimeType.Seconds;
                    break;
            }
        
            XmlNode intervalType = ConfigXml.CreateNode("element", "timeintervalltype", "");
            intervalType.InnerText = result.ToString();
            ConfigXml.DocumentElement.AppendChild(intervalType);
        
        }
		//
		private void radioDayInterval_CheckedChanged(object sender, EventArgs e)
		{
			if (radioDayInterval.Checked)
			{
				dayInterval.Enabled = true;
				daySunday.Enabled = false;
				dayMonday.Enabled = false;
				dayTuesday.Enabled = false;
				dayWednesday.Enabled = false;
				dayThursday.Enabled = false;
				dayFriday.Enabled = false;
				daySaturday.Enabled = false;
				
			}
			else
			{
				dayInterval.Enabled = false;
				daySunday.Enabled = true;
				dayMonday.Enabled = true;
				dayTuesday.Enabled = true;
				dayWednesday.Enabled = true;
				dayThursday.Enabled = true;
				dayFriday.Enabled = true;
				daySaturday.Enabled = true;
			}
		}
		private void radioDayofMonth_CheckedChanged(object sender, EventArgs e)
		{
			if (radioDayofMonth.Checked)
			{
				_dayofmonth.Enabled = true;
				weekdayDropDown.Enabled = false;
				ordinalDropDown.Enabled = false;
			}
			else
			{
				_dayofmonth.Enabled = false;
				weekdayDropDown.Enabled = true;
				ordinalDropDown.Enabled = true;
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.None;
			ScheduleDialog_UnLoad(sender,  e);
			DialogResult = DialogResult.OK;
		}
	}
}
