using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blogical.Shared.Adapters.Common.Schedules.UI
{
    partial class ScheduleDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleDialog));
            this.monthDecember = new System.Windows.Forms.CheckBox();
            this.monthNovember = new System.Windows.Forms.CheckBox();
            this.monthOctober = new System.Windows.Forms.CheckBox();
            this.monthSeptember = new System.Windows.Forms.CheckBox();
            this.monthAugust = new System.Windows.Forms.CheckBox();
            this.monthJuly = new System.Windows.Forms.CheckBox();
            this.monthJune = new System.Windows.Forms.CheckBox();
            this.monthMay = new System.Windows.Forms.CheckBox();
            this.monthApril = new System.Windows.Forms.CheckBox();
            this.monthMarch = new System.Windows.Forms.CheckBox();
            this.monthFebruary = new System.Windows.Forms.CheckBox();
            this.monthJanuary = new System.Windows.Forms.CheckBox();
            this.weekdayDropDown = new System.Windows.Forms.ComboBox();
            this.ordinalDropDown = new System.Windows.Forms.ComboBox();
            this.radioOrdinal = new System.Windows.Forms.RadioButton();
            this._dayofmonth = new System.Windows.Forms.NumericUpDown();
            this.radioDayofMonth = new System.Windows.Forms.RadioButton();
            this.daySaturday = new System.Windows.Forms.CheckBox();
            this.dayFriday = new System.Windows.Forms.CheckBox();
            this.dayThursday = new System.Windows.Forms.CheckBox();
            this.dayWednesday = new System.Windows.Forms.CheckBox();
            this.dayTuesday = new System.Windows.Forms.CheckBox();
            this.dayMonday = new System.Windows.Forms.CheckBox();
            this.daySunday = new System.Windows.Forms.CheckBox();
            this.radioSelectDays = new System.Windows.Forms.RadioButton();
            this.dayInterval = new System.Windows.Forms.NumericUpDown();
            this.labelDays = new System.Windows.Forms.Label();
            this.radioDayInterval = new System.Windows.Forms.RadioButton();
            this.weekSaturday = new System.Windows.Forms.CheckBox();
            this.weekFriday = new System.Windows.Forms.CheckBox();
            this.weekThursday = new System.Windows.Forms.CheckBox();
            this.weekWednesday = new System.Windows.Forms.CheckBox();
            this.weekTuesday = new System.Windows.Forms.CheckBox();
            this.weekMonday = new System.Windows.Forms.CheckBox();
            this.weekSunday = new System.Windows.Forms.CheckBox();
            this.labelSelectDays = new System.Windows.Forms.Label();
            this.labelEvery = new System.Windows.Forms.Label();
            this.labelweeks = new System.Windows.Forms.Label();
            this.weekInterval = new System.Windows.Forms.NumericUpDown();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.dateStart = new System.Windows.Forms.DateTimePicker();
            this.labelStartDate = new System.Windows.Forms.Label();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.timeStart = new System.Windows.Forms.DateTimePicker();
            this.tabPages = new System.Windows.Forms.TabControl();
            this.tabDaily = new System.Windows.Forms.TabPage();
            this.tabWeekly = new System.Windows.Forms.TabPage();
            this.tabMonthly = new System.Windows.Forms.TabPage();
            this.tabTimely = new System.Windows.Forms.TabPage();
            this.timeType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timeInterval = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this._dayofmonth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.weekInterval)).BeginInit();
            this.tabPages.SuspendLayout();
            this.tabDaily.SuspendLayout();
            this.tabWeekly.SuspendLayout();
            this.tabMonthly.SuspendLayout();
            this.tabTimely.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // monthDecember
            // 
            this.monthDecember.Location = new System.Drawing.Point(216, 168);
            this.monthDecember.Name = "monthDecember";
            this.monthDecember.Size = new System.Drawing.Size(80, 16);
            this.monthDecember.TabIndex = 16;
            this.monthDecember.Text = "December";
            // 
            // monthNovember
            // 
            this.monthNovember.Location = new System.Drawing.Point(216, 144);
            this.monthNovember.Name = "monthNovember";
            this.monthNovember.Size = new System.Drawing.Size(80, 16);
            this.monthNovember.TabIndex = 15;
            this.monthNovember.Text = "November";
            // 
            // monthOctober
            // 
            this.monthOctober.Location = new System.Drawing.Point(216, 120);
            this.monthOctober.Name = "monthOctober";
            this.monthOctober.Size = new System.Drawing.Size(80, 16);
            this.monthOctober.TabIndex = 14;
            this.monthOctober.Text = "October";
            // 
            // monthSeptember
            // 
            this.monthSeptember.Location = new System.Drawing.Point(216, 96);
            this.monthSeptember.Name = "monthSeptember";
            this.monthSeptember.Size = new System.Drawing.Size(80, 16);
            this.monthSeptember.TabIndex = 13;
            this.monthSeptember.Text = "September";
            // 
            // monthAugust
            // 
            this.monthAugust.Location = new System.Drawing.Point(112, 168);
            this.monthAugust.Name = "monthAugust";
            this.monthAugust.Size = new System.Drawing.Size(72, 16);
            this.monthAugust.TabIndex = 12;
            this.monthAugust.Text = "August";
            // 
            // monthJuly
            // 
            this.monthJuly.Location = new System.Drawing.Point(112, 144);
            this.monthJuly.Name = "monthJuly";
            this.monthJuly.Size = new System.Drawing.Size(72, 16);
            this.monthJuly.TabIndex = 11;
            this.monthJuly.Text = "July";
            // 
            // monthJune
            // 
            this.monthJune.Location = new System.Drawing.Point(112, 120);
            this.monthJune.Name = "monthJune";
            this.monthJune.Size = new System.Drawing.Size(72, 16);
            this.monthJune.TabIndex = 10;
            this.monthJune.Text = "June";
            // 
            // monthMay
            // 
            this.monthMay.Location = new System.Drawing.Point(112, 96);
            this.monthMay.Name = "monthMay";
            this.monthMay.Size = new System.Drawing.Size(72, 16);
            this.monthMay.TabIndex = 9;
            this.monthMay.Text = "May";
            // 
            // monthApril
            // 
            this.monthApril.Location = new System.Drawing.Point(8, 168);
            this.monthApril.Name = "monthApril";
            this.monthApril.Size = new System.Drawing.Size(72, 16);
            this.monthApril.TabIndex = 8;
            this.monthApril.Text = "April";
            // 
            // monthMarch
            // 
            this.monthMarch.Location = new System.Drawing.Point(8, 144);
            this.monthMarch.Name = "monthMarch";
            this.monthMarch.Size = new System.Drawing.Size(72, 16);
            this.monthMarch.TabIndex = 7;
            this.monthMarch.Text = "March";
            // 
            // monthFebruary
            // 
            this.monthFebruary.Location = new System.Drawing.Point(8, 120);
            this.monthFebruary.Name = "monthFebruary";
            this.monthFebruary.Size = new System.Drawing.Size(72, 16);
            this.monthFebruary.TabIndex = 6;
            this.monthFebruary.Text = "February";
            // 
            // monthJanuary
            // 
            this.monthJanuary.Location = new System.Drawing.Point(8, 96);
            this.monthJanuary.Name = "monthJanuary";
            this.monthJanuary.Size = new System.Drawing.Size(64, 16);
            this.monthJanuary.TabIndex = 5;
            this.monthJanuary.Text = "January";
            // 
            // weekdayDropDown
            // 
            this.weekdayDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.weekdayDropDown.Items.AddRange(new object[] {
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "WeekDay",
            "Day"});
            this.weekdayDropDown.Location = new System.Drawing.Point(184, 48);
            this.weekdayDropDown.MaxDropDownItems = 9;
            this.weekdayDropDown.Name = "weekdayDropDown";
            this.weekdayDropDown.Size = new System.Drawing.Size(112, 21);
            this.weekdayDropDown.TabIndex = 4;
            // 
            // ordinalDropDown
            // 
            this.ordinalDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ordinalDropDown.Items.AddRange(new object[] {
            "First",
            "Second",
            "Third",
            "Fourth",
            "Last"});
            this.ordinalDropDown.Location = new System.Drawing.Point(72, 48);
            this.ordinalDropDown.MaxDropDownItems = 5;
            this.ordinalDropDown.Name = "ordinalDropDown";
            this.ordinalDropDown.Size = new System.Drawing.Size(96, 21);
            this.ordinalDropDown.TabIndex = 3;
            // 
            // radioOrdinal
            // 
            this.radioOrdinal.Location = new System.Drawing.Point(8, 46);
            this.radioOrdinal.Name = "radioOrdinal";
            this.radioOrdinal.Size = new System.Drawing.Size(56, 24);
            this.radioOrdinal.TabIndex = 2;
            this.radioOrdinal.Text = "The";
            // 
            // dayofmonth
            // 
            this._dayofmonth.Location = new System.Drawing.Point(64, 16);
            this._dayofmonth.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this._dayofmonth.Name = "_dayofmonth";
            this._dayofmonth.Size = new System.Drawing.Size(40, 20);
            this._dayofmonth.TabIndex = 1;
            // 
            // radioDayofMonth
            // 
            this.radioDayofMonth.Location = new System.Drawing.Point(8, 18);
            this.radioDayofMonth.Name = "radioDayofMonth";
            this.radioDayofMonth.Size = new System.Drawing.Size(56, 16);
            this.radioDayofMonth.TabIndex = 0;
            this.radioDayofMonth.Text = "Day";
            this.radioDayofMonth.CheckedChanged += new System.EventHandler(this.radioDayofMonth_CheckedChanged);
            // 
            // daySaturday
            // 
            this.daySaturday.Enabled = false;
            this.daySaturday.Location = new System.Drawing.Point(160, 152);
            this.daySaturday.Name = "daySaturday";
            this.daySaturday.Size = new System.Drawing.Size(104, 16);
            this.daySaturday.TabIndex = 10;
            this.daySaturday.Text = "Saturday";
            // 
            // dayFriday
            // 
            this.dayFriday.Enabled = false;
            this.dayFriday.Location = new System.Drawing.Point(160, 128);
            this.dayFriday.Name = "dayFriday";
            this.dayFriday.Size = new System.Drawing.Size(104, 16);
            this.dayFriday.TabIndex = 9;
            this.dayFriday.Text = "Friday";
            // 
            // dayThursday
            // 
            this.dayThursday.Enabled = false;
            this.dayThursday.Location = new System.Drawing.Point(160, 104);
            this.dayThursday.Name = "dayThursday";
            this.dayThursday.Size = new System.Drawing.Size(104, 16);
            this.dayThursday.TabIndex = 8;
            this.dayThursday.Text = "Thursday";
            // 
            // dayWednesday
            // 
            this.dayWednesday.Enabled = false;
            this.dayWednesday.Location = new System.Drawing.Point(160, 80);
            this.dayWednesday.Name = "dayWednesday";
            this.dayWednesday.Size = new System.Drawing.Size(104, 16);
            this.dayWednesday.TabIndex = 7;
            this.dayWednesday.Text = "Wednesday";
            // 
            // dayTuesday
            // 
            this.dayTuesday.Enabled = false;
            this.dayTuesday.Location = new System.Drawing.Point(40, 128);
            this.dayTuesday.Name = "dayTuesday";
            this.dayTuesday.Size = new System.Drawing.Size(104, 16);
            this.dayTuesday.TabIndex = 6;
            this.dayTuesday.Text = "Tuesday";
            // 
            // dayMonday
            // 
            this.dayMonday.Enabled = false;
            this.dayMonday.Location = new System.Drawing.Point(40, 104);
            this.dayMonday.Name = "dayMonday";
            this.dayMonday.Size = new System.Drawing.Size(104, 16);
            this.dayMonday.TabIndex = 5;
            this.dayMonday.Text = "Monday";
            // 
            // daySunday
            // 
            this.daySunday.Enabled = false;
            this.daySunday.Location = new System.Drawing.Point(40, 80);
            this.daySunday.Name = "daySunday";
            this.daySunday.Size = new System.Drawing.Size(104, 16);
            this.daySunday.TabIndex = 4;
            this.daySunday.Text = "Sunday";
            // 
            // radioSelectDays
            // 
            this.radioSelectDays.Location = new System.Drawing.Point(8, 56);
            this.radioSelectDays.Name = "radioSelectDays";
            this.radioSelectDays.Size = new System.Drawing.Size(104, 16);
            this.radioSelectDays.TabIndex = 3;
            this.radioSelectDays.Text = "On these days";
            // 
            // dayInterval
            // 
            this.dayInterval.Location = new System.Drawing.Point(64, 16);
            this.dayInterval.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.dayInterval.Name = "dayInterval";
            this.dayInterval.Size = new System.Drawing.Size(48, 20);
            this.dayInterval.TabIndex = 2;
            // 
            // labelDays
            // 
            this.labelDays.Location = new System.Drawing.Point(120, 16);
            this.labelDays.Name = "labelDays";
            this.labelDays.Size = new System.Drawing.Size(32, 16);
            this.labelDays.TabIndex = 10;
            this.labelDays.Text = "days";
            // 
            // radioDayInterval
            // 
            this.radioDayInterval.Checked = true;
            this.radioDayInterval.Location = new System.Drawing.Point(8, 16);
            this.radioDayInterval.Name = "radioDayInterval";
            this.radioDayInterval.Size = new System.Drawing.Size(64, 16);
            this.radioDayInterval.TabIndex = 1;
            this.radioDayInterval.TabStop = true;
            this.radioDayInterval.Text = "Every";
            this.radioDayInterval.CheckedChanged += new System.EventHandler(this.radioDayInterval_CheckedChanged);
            // 
            // weekSaturday
            // 
            this.weekSaturday.Location = new System.Drawing.Point(160, 152);
            this.weekSaturday.Name = "weekSaturday";
            this.weekSaturday.Size = new System.Drawing.Size(104, 16);
            this.weekSaturday.TabIndex = 11;
            this.weekSaturday.Text = "Saturday";
            // 
            // weekFriday
            // 
            this.weekFriday.Location = new System.Drawing.Point(160, 128);
            this.weekFriday.Name = "weekFriday";
            this.weekFriday.Size = new System.Drawing.Size(104, 16);
            this.weekFriday.TabIndex = 10;
            this.weekFriday.Text = "Friday";
            // 
            // weekThursday
            // 
            this.weekThursday.Location = new System.Drawing.Point(160, 104);
            this.weekThursday.Name = "weekThursday";
            this.weekThursday.Size = new System.Drawing.Size(104, 16);
            this.weekThursday.TabIndex = 9;
            this.weekThursday.Text = "Thursday";
            // 
            // weekWednesday
            // 
            this.weekWednesday.Location = new System.Drawing.Point(160, 80);
            this.weekWednesday.Name = "weekWednesday";
            this.weekWednesday.Size = new System.Drawing.Size(104, 16);
            this.weekWednesday.TabIndex = 8;
            this.weekWednesday.Text = "Wednesday";
            // 
            // weekTuesday
            // 
            this.weekTuesday.Location = new System.Drawing.Point(40, 128);
            this.weekTuesday.Name = "weekTuesday";
            this.weekTuesday.Size = new System.Drawing.Size(104, 16);
            this.weekTuesday.TabIndex = 7;
            this.weekTuesday.Text = "Tuesday";
            // 
            // weekMonday
            // 
            this.weekMonday.Location = new System.Drawing.Point(40, 104);
            this.weekMonday.Name = "weekMonday";
            this.weekMonday.Size = new System.Drawing.Size(104, 16);
            this.weekMonday.TabIndex = 6;
            this.weekMonday.Text = "Monday";
            // 
            // weekSunday
            // 
            this.weekSunday.Location = new System.Drawing.Point(40, 80);
            this.weekSunday.Name = "weekSunday";
            this.weekSunday.Size = new System.Drawing.Size(104, 16);
            this.weekSunday.TabIndex = 5;
            this.weekSunday.Text = "Sunday";
            // 
            // labelSelectDays
            // 
            this.labelSelectDays.Location = new System.Drawing.Point(8, 56);
            this.labelSelectDays.Name = "labelSelectDays";
            this.labelSelectDays.Size = new System.Drawing.Size(200, 16);
            this.labelSelectDays.TabIndex = 4;
            this.labelSelectDays.Text = "Select the day(s) of the week below:";
            // 
            // labelEvery
            // 
            this.labelEvery.Location = new System.Drawing.Point(8, 16);
            this.labelEvery.Name = "labelEvery";
            this.labelEvery.Size = new System.Drawing.Size(40, 16);
            this.labelEvery.TabIndex = 3;
            this.labelEvery.Text = "Every";
            // 
            // labelweeks
            // 
            this.labelweeks.Location = new System.Drawing.Point(112, 16);
            this.labelweeks.Name = "labelweeks";
            this.labelweeks.Size = new System.Drawing.Size(100, 16);
            this.labelweeks.TabIndex = 2;
            this.labelweeks.Text = "weeks";
            // 
            // weekInterval
            // 
            this.weekInterval.Location = new System.Drawing.Point(56, 16);
            this.weekInterval.Maximum = new decimal(new int[] {
            52,
            0,
            0,
            0});
            this.weekInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.weekInterval.Name = "weekInterval";
            this.weekInterval.Size = new System.Drawing.Size(48, 20);
            this.weekInterval.TabIndex = 1;
            this.weekInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(160, 360);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(248, 360);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            // 
            // dateStart
            // 
            this.dateStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateStart.Location = new System.Drawing.Point(104, 16);
            this.dateStart.Name = "dateStart";
            this.dateStart.Size = new System.Drawing.Size(104, 20);
            this.dateStart.TabIndex = 1;
            // 
            // labelStartDate
            // 
            this.labelStartDate.Location = new System.Drawing.Point(16, 18);
            this.labelStartDate.Name = "labelStartDate";
            this.labelStartDate.Size = new System.Drawing.Size(72, 16);
            this.labelStartDate.TabIndex = 8;
            this.labelStartDate.Text = "Start Date";
            // 
            // labelStartTime
            // 
            this.labelStartTime.Location = new System.Drawing.Point(16, 55);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(72, 23);
            this.labelStartTime.TabIndex = 9;
            this.labelStartTime.Text = "Start Time";
            // 
            // timeStart
            // 
            this.timeStart.CustomFormat = "HH:mm tt";
            this.timeStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timeStart.Location = new System.Drawing.Point(120, 56);
            this.timeStart.Name = "timeStart";
            this.timeStart.ShowUpDown = true;
            this.timeStart.Size = new System.Drawing.Size(88, 20);
            this.timeStart.TabIndex = 2;
            this.timeStart.Value = new System.DateTime(2005, 7, 30, 16, 22, 0, 0);
            // 
            // tabPages
            // 
            this.tabPages.Controls.Add(this.tabDaily);
            this.tabPages.Controls.Add(this.tabWeekly);
            this.tabPages.Controls.Add(this.tabMonthly);
            this.tabPages.Controls.Add(this.tabTimely);
            this.tabPages.Location = new System.Drawing.Point(8, 96);
            this.tabPages.Name = "tabPages";
            this.tabPages.SelectedIndex = 0;
            this.tabPages.Size = new System.Drawing.Size(320, 240);
            this.tabPages.TabIndex = 3;
            // 
            // tabDaily
            // 
            this.tabDaily.Controls.Add(this.dayInterval);
            this.tabDaily.Controls.Add(this.radioDayInterval);
            this.tabDaily.Controls.Add(this.labelDays);
            this.tabDaily.Controls.Add(this.radioSelectDays);
            this.tabDaily.Controls.Add(this.daySunday);
            this.tabDaily.Controls.Add(this.dayMonday);
            this.tabDaily.Controls.Add(this.dayTuesday);
            this.tabDaily.Controls.Add(this.dayWednesday);
            this.tabDaily.Controls.Add(this.dayThursday);
            this.tabDaily.Controls.Add(this.dayFriday);
            this.tabDaily.Controls.Add(this.daySaturday);
            this.tabDaily.Location = new System.Drawing.Point(4, 22);
            this.tabDaily.Name = "tabDaily";
            this.tabDaily.Size = new System.Drawing.Size(312, 214);
            this.tabDaily.TabIndex = 0;
            this.tabDaily.Text = "Daily";
            // 
            // tabWeekly
            // 
            this.tabWeekly.Controls.Add(this.labelSelectDays);
            this.tabWeekly.Controls.Add(this.labelEvery);
            this.tabWeekly.Controls.Add(this.labelweeks);
            this.tabWeekly.Controls.Add(this.weekInterval);
            this.tabWeekly.Controls.Add(this.weekSunday);
            this.tabWeekly.Controls.Add(this.weekMonday);
            this.tabWeekly.Controls.Add(this.weekTuesday);
            this.tabWeekly.Controls.Add(this.weekWednesday);
            this.tabWeekly.Controls.Add(this.weekThursday);
            this.tabWeekly.Controls.Add(this.weekFriday);
            this.tabWeekly.Controls.Add(this.weekSaturday);
            this.tabWeekly.Location = new System.Drawing.Point(4, 22);
            this.tabWeekly.Name = "tabWeekly";
            this.tabWeekly.Size = new System.Drawing.Size(312, 214);
            this.tabWeekly.TabIndex = 1;
            this.tabWeekly.Text = "Weekly";
            // 
            // tabMonthly
            // 
            this.tabMonthly.Controls.Add(this.monthApril);
            this.tabMonthly.Controls.Add(this.monthAugust);
            this.tabMonthly.Controls.Add(this.monthDecember);
            this.tabMonthly.Controls.Add(this.monthNovember);
            this.tabMonthly.Controls.Add(this.monthJuly);
            this.tabMonthly.Controls.Add(this.monthMarch);
            this.tabMonthly.Controls.Add(this.monthFebruary);
            this.tabMonthly.Controls.Add(this.monthJune);
            this.tabMonthly.Controls.Add(this.monthOctober);
            this.tabMonthly.Controls.Add(this.monthSeptember);
            this.tabMonthly.Controls.Add(this.monthMay);
            this.tabMonthly.Controls.Add(this.monthJanuary);
            this.tabMonthly.Controls.Add(this.ordinalDropDown);
            this.tabMonthly.Controls.Add(this.radioOrdinal);
            this.tabMonthly.Controls.Add(this.weekdayDropDown);
            this.tabMonthly.Controls.Add(this.radioDayofMonth);
            this.tabMonthly.Controls.Add(this._dayofmonth);
            this.tabMonthly.Location = new System.Drawing.Point(4, 22);
            this.tabMonthly.Name = "tabMonthly";
            this.tabMonthly.Size = new System.Drawing.Size(312, 214);
            this.tabMonthly.TabIndex = 2;
            this.tabMonthly.Text = "Monthly";
            // 
            // tabTimely
            // 
            this.tabTimely.BackColor = System.Drawing.SystemColors.Control;
            this.tabTimely.Controls.Add(this.timeInterval);
            this.tabTimely.Controls.Add(this.label1);
            this.tabTimely.Controls.Add(this.timeType);
            this.tabTimely.Location = new System.Drawing.Point(4, 22);
            this.tabTimely.Name = "tabTimely";
            this.tabTimely.Padding = new System.Windows.Forms.Padding(3);
            this.tabTimely.Size = new System.Drawing.Size(312, 214);
            this.tabTimely.TabIndex = 3;
            this.tabTimely.Text = "Timely";
            // 
            // timeType
            // 
            this.timeType.FormattingEnabled = true;
            this.timeType.Items.AddRange(new object[] {
            "Seconds",
            "Minutes",
            "Hours"});
            this.timeType.Location = new System.Drawing.Point(108, 28);
            this.timeType.Name = "timeType";
            this.timeType.Size = new System.Drawing.Size(95, 21);
            this.timeType.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Every:";
            // 
            // timeInterval
            // 
            this.timeInterval.Location = new System.Drawing.Point(54, 29);
            this.timeInterval.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.timeInterval.Name = "timeInterval";
            this.timeInterval.Size = new System.Drawing.Size(48, 20);
            this.timeInterval.TabIndex = 3;
            this.timeInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // ScheduleDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(336, 400);
            this.Controls.Add(this.timeStart);
            this.Controls.Add(this.labelStartTime);
            this.Controls.Add(this.labelStartDate);
            this.Controls.Add(this.dateStart);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.tabPages);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScheduleDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Schedule Properties";
            this.Load += new System.EventHandler(this.ScheduleDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this._dayofmonth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.weekInterval)).EndInit();
            this.tabPages.ResumeLayout(false);
            this.tabDaily.ResumeLayout(false);
            this.tabWeekly.ResumeLayout(false);
            this.tabMonthly.ResumeLayout(false);
            this.tabTimely.ResumeLayout(false);
            this.tabTimely.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeInterval)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private Button okButton;
        private Button cancelButton;
        private DateTimePicker dateStart;
        private Label labelStartDate;
        private Label labelStartTime;
        private DateTimePicker timeStart;
        private RadioButton radioDayInterval;
        private Label labelDays;
        private RadioButton radioSelectDays;
        private Label labelweeks;
        private Label labelEvery;
        private RadioButton radioDayofMonth;
        private NumericUpDown _dayofmonth;
        private RadioButton radioOrdinal;
        private ComboBox ordinalDropDown;
        private ComboBox weekdayDropDown;
        private CheckBox monthDecember;
        private CheckBox monthNovember;
        private CheckBox monthOctober;
        private CheckBox monthSeptember;
        private CheckBox monthAugust;
        private CheckBox monthJuly;
        private CheckBox monthJune;
        private CheckBox monthMay;
        private CheckBox monthApril;
        private CheckBox monthMarch;
        private CheckBox monthFebruary;
        private CheckBox monthJanuary;
        private CheckBox weekSaturday;
        private CheckBox weekFriday;
        private CheckBox weekThursday;
        private CheckBox weekWednesday;
        private CheckBox weekTuesday;
        private CheckBox weekMonday;
        private CheckBox weekSunday;
        private Label labelSelectDays;
        private NumericUpDown weekInterval;
        private CheckBox daySaturday;
        private CheckBox dayFriday;
        private CheckBox dayThursday;
        private CheckBox dayWednesday;
        private CheckBox dayTuesday;
        private CheckBox dayMonday;
        private CheckBox daySunday;
        private NumericUpDown dayInterval;
        private TabControl tabPages;
        private TabPage tabDaily;
        private TabPage tabWeekly;
        private TabPage tabMonthly;
        private TabPage tabTimely;
        private Label label1;
        private ComboBox timeType;
        private NumericUpDown timeInterval;
    }
}
