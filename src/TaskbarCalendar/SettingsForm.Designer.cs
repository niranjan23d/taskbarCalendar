using System.Drawing;
using System.Windows.Forms;

namespace TaskbarCalendar
{
    public sealed partial class SettingsForm
    {
        System.ComponentModel.IContainer components;
        internal TabControl Tabs; internal TabPage outlookPage, displayPage, appearancePage, privacyPage, generalPage;
        internal NumericUpDown lookahead, refresh, width, gap, amber, fadeDelay, fadeDuration, appearStart, appearDuration, opacity, ruleThickness, marginThickness;
        internal ComboBox layout, tone, privacyDisplay, preset, clickAction, doubleClickAction, hoverAction, textElement;
        internal CheckBox automaticPrivacy, shadow, callout, startup, autoWidth, twentyFourHour, lateAlert, skipCancelledMeetings, noStripBackground, autoTaskbarTextColors, textBold, textItalic, textUnderline, doubleMargin;
        internal TextBox emptyMessage; internal Button fontPicker, textColour, testOutlook, colourStrip, colourGreen, colourAmber, colourPaper, colourRule, colourAccent, pickAnother, settingsFolder, logFolder, applyButton, okButton, cancelButton;
        internal AppearancePreview appearancePreview; internal Label testResult;         GroupBox gOutlook, gRefresh, gStrip, gActions, gPreview, gCard, gText, gColours, gEmpty, gPrivacy, gStartup, gFiles, gAbout;
        internal Label colourStripLabel, colourGreenLabel, colourAmberLabel, colourPaperLabel, colourRuleLabel, colourAccentLabel, ruleThicknessLabel, marginThicknessLabel, cornerRadiusLabel, cornerRadiusValue, emptyPreview;
        internal TrackBar cornerRadius;
        Label l1,l2,l3,l4,l5,l6,l7,l8,l9,l10,l11,l12,l13,l14,l15,l16,l17,l18,l19,l20,l21,l22,l23;
        Panel commands;

        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.Tabs = new System.Windows.Forms.TabControl();
            this.outlookPage = new System.Windows.Forms.TabPage();
            this.gOutlook = new System.Windows.Forms.GroupBox();
            this.l1 = new System.Windows.Forms.Label();
            this.testOutlook = new System.Windows.Forms.Button();
            this.testResult = new System.Windows.Forms.Label();
            this.gRefresh = new System.Windows.Forms.GroupBox();
            this.l2 = new System.Windows.Forms.Label();
            this.lookahead = new System.Windows.Forms.NumericUpDown();
            this.l3 = new System.Windows.Forms.Label();
            this.refresh = new System.Windows.Forms.NumericUpDown();
            this.displayPage = new System.Windows.Forms.TabPage();
            this.gStrip = new System.Windows.Forms.GroupBox();
            this.l4 = new System.Windows.Forms.Label();
            this.layout = new System.Windows.Forms.ComboBox();
            this.autoWidth = new System.Windows.Forms.CheckBox();
            this.twentyFourHour = new System.Windows.Forms.CheckBox();
            this.l5 = new System.Windows.Forms.Label();
            this.width = new System.Windows.Forms.NumericUpDown();
            this.l6 = new System.Windows.Forms.Label();
            this.gap = new System.Windows.Forms.NumericUpDown();
            this.l7 = new System.Windows.Forms.Label();
            this.amber = new System.Windows.Forms.NumericUpDown();
            this.lateAlert = new System.Windows.Forms.CheckBox();
            this.skipCancelledMeetings = new System.Windows.Forms.CheckBox();
            this.gActions = new System.Windows.Forms.GroupBox();
            this.l8 = new System.Windows.Forms.Label();
            this.fadeDelay = new System.Windows.Forms.NumericUpDown();
            this.fadeDuration = new System.Windows.Forms.NumericUpDown();
            this.l23 = new System.Windows.Forms.Label();
            this.appearStart = new System.Windows.Forms.NumericUpDown();
            this.appearDuration = new System.Windows.Forms.NumericUpDown();
            this.l9 = new System.Windows.Forms.Label();
            this.clickAction = new System.Windows.Forms.ComboBox();
            this.l10 = new System.Windows.Forms.Label();
            this.doubleClickAction = new System.Windows.Forms.ComboBox();
            this.l11 = new System.Windows.Forms.Label();
            this.hoverAction = new System.Windows.Forms.ComboBox();
            this.appearancePage = new System.Windows.Forms.TabPage();
            this.gPreview = new System.Windows.Forms.GroupBox();
            this.l12 = new System.Windows.Forms.Label();
            this.preset = new System.Windows.Forms.ComboBox();
            this.appearancePreview = new TaskbarCalendar.AppearancePreview();
            this.gCard = new System.Windows.Forms.GroupBox();
            this.l13 = new System.Windows.Forms.Label();
            this.opacity = new System.Windows.Forms.NumericUpDown();
            this.noStripBackground = new System.Windows.Forms.CheckBox();
            this.autoTaskbarTextColors = new System.Windows.Forms.CheckBox();
            this.shadow = new System.Windows.Forms.CheckBox();
            this.callout = new System.Windows.Forms.CheckBox();
            this.gText = new System.Windows.Forms.GroupBox();
            this.l14 = new System.Windows.Forms.Label();
            this.textElement = new System.Windows.Forms.ComboBox();
            this.l15 = new System.Windows.Forms.Label();
            this.textColour = new System.Windows.Forms.Button();
            this.textBold = new System.Windows.Forms.CheckBox();
            this.textItalic = new System.Windows.Forms.CheckBox();
            this.textUnderline = new System.Windows.Forms.CheckBox();
            this.l16 = new System.Windows.Forms.Label();
            this.fontPicker = new System.Windows.Forms.Button();
            this.gColours = new System.Windows.Forms.GroupBox();
            this.colourStripLabel = new System.Windows.Forms.Label();
            this.colourStrip = new System.Windows.Forms.Button();
            this.colourGreenLabel = new System.Windows.Forms.Label();
            this.colourGreen = new System.Windows.Forms.Button();
            this.colourAmberLabel = new System.Windows.Forms.Label();
            this.colourAmber = new System.Windows.Forms.Button();
            this.colourPaperLabel = new System.Windows.Forms.Label();
            this.colourPaper = new System.Windows.Forms.Button();
            this.colourRuleLabel = new System.Windows.Forms.Label();
            this.colourRule = new System.Windows.Forms.Button();
            this.colourAccentLabel = new System.Windows.Forms.Label();
            this.colourAccent = new System.Windows.Forms.Button();
            this.doubleMargin = new System.Windows.Forms.CheckBox();
            this.ruleThicknessLabel = new System.Windows.Forms.Label();
            this.ruleThickness = new System.Windows.Forms.NumericUpDown();
            this.marginThicknessLabel = new System.Windows.Forms.Label();
            this.marginThickness = new System.Windows.Forms.NumericUpDown();
            this.cornerRadiusLabel = new System.Windows.Forms.Label();
            this.cornerRadius = new System.Windows.Forms.TrackBar();
            this.cornerRadiusValue = new System.Windows.Forms.Label();
            this.gEmpty = new System.Windows.Forms.GroupBox();
            this.l17 = new System.Windows.Forms.Label();
            this.tone = new System.Windows.Forms.ComboBox();
            this.l18 = new System.Windows.Forms.Label();
            this.emptyMessage = new System.Windows.Forms.TextBox();
            this.pickAnother = new System.Windows.Forms.Button();
            this.emptyPreview = new System.Windows.Forms.Label();
            this.privacyPage = new System.Windows.Forms.TabPage();
            this.gPrivacy = new System.Windows.Forms.GroupBox();
            this.automaticPrivacy = new System.Windows.Forms.CheckBox();
            this.l19 = new System.Windows.Forms.Label();
            this.privacyDisplay = new System.Windows.Forms.ComboBox();
            this.generalPage = new System.Windows.Forms.TabPage();
            this.gStartup = new System.Windows.Forms.GroupBox();
            this.startup = new System.Windows.Forms.CheckBox();
            this.gFiles = new System.Windows.Forms.GroupBox();
            this.settingsFolder = new System.Windows.Forms.Button();
            this.logFolder = new System.Windows.Forms.Button();
            this.gAbout = new System.Windows.Forms.GroupBox();
            this.l20 = new System.Windows.Forms.Label();
            this.l21 = new System.Windows.Forms.Label();
            this.l22 = new System.Windows.Forms.Label();
            this.applyButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.commands = new System.Windows.Forms.Panel();
            this.Tabs.SuspendLayout();
            this.outlookPage.SuspendLayout();
            this.gOutlook.SuspendLayout();
            this.gRefresh.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lookahead)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.refresh)).BeginInit();
            this.displayPage.SuspendLayout();
            this.gStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.width)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amber)).BeginInit();
            this.gActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fadeDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fadeDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.appearStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.appearDuration)).BeginInit();
            this.appearancePage.SuspendLayout();
            this.gPreview.SuspendLayout();
            this.gCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.opacity)).BeginInit();
            this.gText.SuspendLayout();
            this.gColours.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ruleThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cornerRadius)).BeginInit();
            this.gEmpty.SuspendLayout();
            this.privacyPage.SuspendLayout();
            this.gPrivacy.SuspendLayout();
            this.generalPage.SuspendLayout();
            this.gStartup.SuspendLayout();
            this.gFiles.SuspendLayout();
            this.gAbout.SuspendLayout();
            this.commands.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.outlookPage);
            this.Tabs.Controls.Add(this.displayPage);
            this.Tabs.Controls.Add(this.appearancePage);
            this.Tabs.Controls.Add(this.privacyPage);
            this.Tabs.Controls.Add(this.generalPage);
            this.Tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tabs.Location = new System.Drawing.Point(0, 0);
            this.Tabs.Name = "Tabs";
            this.Tabs.Padding = new System.Drawing.Point(8, 3);
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(510, 812);
            this.Tabs.TabIndex = 0;
            // 
            // outlookPage
            // 
            this.outlookPage.AutoScroll = true;
            this.outlookPage.Controls.Add(this.gOutlook);
            this.outlookPage.Controls.Add(this.gRefresh);
            this.outlookPage.Location = new System.Drawing.Point(4, 22);
            this.outlookPage.Name = "outlookPage";
            this.outlookPage.Size = new System.Drawing.Size(502, 761);
            this.outlookPage.TabIndex = 0;
            this.outlookPage.Text = "Outlook";
            // 
            // gOutlook
            // 
            this.gOutlook.Controls.Add(this.l1);
            this.gOutlook.Controls.Add(this.testOutlook);
            this.gOutlook.Controls.Add(this.testResult);
            this.gOutlook.Location = new System.Drawing.Point(9, 8);
            this.gOutlook.Name = "gOutlook";
            this.gOutlook.Size = new System.Drawing.Size(490, 95);
            this.gOutlook.TabIndex = 0;
            this.gOutlook.TabStop = false;
            this.gOutlook.Text = "Classic Outlook calendar";
            // 
            // l1
            // 
            this.l1.Location = new System.Drawing.Point(15, 22);
            this.l1.Name = "l1";
            this.l1.Size = new System.Drawing.Size(372, 20);
            this.l1.TabIndex = 0;
            this.l1.Text = "Reads the default calendar from classic Outlook on this PC.";
            // 
            // testOutlook
            // 
            this.testOutlook.Location = new System.Drawing.Point(15, 55);
            this.testOutlook.Name = "testOutlook";
            this.testOutlook.Size = new System.Drawing.Size(105, 25);
            this.testOutlook.TabIndex = 1;
            this.testOutlook.Text = "Test Outlook";
            // 
            // testResult
            // 
            this.testResult.Location = new System.Drawing.Point(130, 57);
            this.testResult.Name = "testResult";
            this.testResult.Size = new System.Drawing.Size(349, 20);
            this.testResult.TabIndex = 2;
            this.testResult.Text = "[Outlook connection message]";
            this.testResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gRefresh
            // 
            this.gRefresh.Controls.Add(this.l2);
            this.gRefresh.Controls.Add(this.lookahead);
            this.gRefresh.Controls.Add(this.l3);
            this.gRefresh.Controls.Add(this.refresh);
            this.gRefresh.Location = new System.Drawing.Point(9, 110);
            this.gRefresh.Name = "gRefresh";
            this.gRefresh.Size = new System.Drawing.Size(490, 80);
            this.gRefresh.TabIndex = 1;
            this.gRefresh.TabStop = false;
            this.gRefresh.Text = "Calendar refresh";
            // 
            // l2
            // 
            this.l2.Location = new System.Drawing.Point(15, 25);
            this.l2.Name = "l2";
            this.l2.Size = new System.Drawing.Size(140, 20);
            this.l2.TabIndex = 0;
            this.l2.Text = "Days to show";
            // 
            // lookahead
            // 
            this.lookahead.Location = new System.Drawing.Point(165, 22);
            this.lookahead.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.lookahead.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lookahead.Name = "lookahead";
            this.lookahead.Size = new System.Drawing.Size(62, 20);
            this.lookahead.TabIndex = 1;
            this.lookahead.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // l3
            // 
            this.l3.Location = new System.Drawing.Point(15, 52);
            this.l3.Name = "l3";
            this.l3.Size = new System.Drawing.Size(140, 20);
            this.l3.TabIndex = 2;
            this.l3.Text = "Refresh minutes";
            // 
            // refresh
            // 
            this.refresh.Location = new System.Drawing.Point(165, 49);
            this.refresh.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.refresh.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.refresh.Name = "refresh";
            this.refresh.Size = new System.Drawing.Size(62, 20);
            this.refresh.TabIndex = 3;
            this.refresh.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // displayPage
            // 
            this.displayPage.AutoScroll = true;
            this.displayPage.Controls.Add(this.gStrip);
            this.displayPage.Controls.Add(this.gActions);
            this.displayPage.Location = new System.Drawing.Point(4, 22);
            this.displayPage.Name = "displayPage";
            this.displayPage.Size = new System.Drawing.Size(502, 761);
            this.displayPage.TabIndex = 1;
            this.displayPage.Text = "Display";
            // 
            // gStrip
            // 
            this.gStrip.Controls.Add(this.l4);
            this.gStrip.Controls.Add(this.layout);
            this.gStrip.Controls.Add(this.autoWidth);
            this.gStrip.Controls.Add(this.twentyFourHour);
            this.gStrip.Controls.Add(this.l5);
            this.gStrip.Controls.Add(this.width);
            this.gStrip.Controls.Add(this.l6);
            this.gStrip.Controls.Add(this.gap);
            this.gStrip.Controls.Add(this.l7);
            this.gStrip.Controls.Add(this.amber);
            this.gStrip.Controls.Add(this.lateAlert);
            this.gStrip.Controls.Add(this.skipCancelledMeetings);
            this.gStrip.Location = new System.Drawing.Point(9, 8);
            this.gStrip.Name = "gStrip";
            this.gStrip.Size = new System.Drawing.Size(490, 187);
            this.gStrip.TabIndex = 0;
            this.gStrip.TabStop = false;
            this.gStrip.Text = "Taskbar strip";
            // 
            // l4
            // 
            this.l4.Location = new System.Drawing.Point(15, 25);
            this.l4.Name = "l4";
            this.l4.Size = new System.Drawing.Size(140, 20);
            this.l4.TabIndex = 0;
            this.l4.Text = "Layout";
            // 
            // layout
            // 
            this.layout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.layout.Items.AddRange(new object[] {
            "[Select layout]"});
            this.layout.Location = new System.Drawing.Point(165, 22);
            this.layout.Name = "layout";
            this.layout.Size = new System.Drawing.Size(170, 21);
            this.layout.TabIndex = 1;
            // 
            // autoWidth
            // 
            this.autoWidth.Location = new System.Drawing.Point(165, 50);
            this.autoWidth.Name = "autoWidth";
            this.autoWidth.Size = new System.Drawing.Size(280, 20);
            this.autoWidth.TabIndex = 2;
            this.autoWidth.Text = "Size strip to its text automatically";
            //
            // twentyFourHour
            //
            this.twentyFourHour.Location = new System.Drawing.Point(15, 50);
            this.twentyFourHour.Name = "twentyFourHour";
            this.twentyFourHour.Size = new System.Drawing.Size(140, 20);
            this.twentyFourHour.TabIndex = 11;
            this.twentyFourHour.Text = "Use 24-hour time";
            //
            // l5
            //
            this.l5.Location = new System.Drawing.Point(15, 78);
            this.l5.Name = "l5";
            this.l5.Size = new System.Drawing.Size(140, 20);
            this.l5.TabIndex = 3;
            this.l5.Text = "Manual width";
            //
            // width
            //
            this.width.Location = new System.Drawing.Point(165, 75);
            this.width.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.width.Minimum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.width.Name = "width";
            this.width.Size = new System.Drawing.Size(62, 20);
            this.width.TabIndex = 4;
            this.width.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            //
            // l6
            //
            this.l6.Location = new System.Drawing.Point(15, 105);
            this.l6.Name = "l6";
            this.l6.Size = new System.Drawing.Size(140, 20);
            this.l6.TabIndex = 5;
            this.l6.Text = "Right gap";
            //
            // gap
            //
            this.gap.Location = new System.Drawing.Point(165, 102);
            this.gap.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.gap.Name = "gap";
            this.gap.Size = new System.Drawing.Size(62, 20);
            this.gap.TabIndex = 6;
            // 
            // l7
            // 
            this.l7.Location = new System.Drawing.Point(15, 132);
            this.l7.Name = "l7";
            this.l7.Size = new System.Drawing.Size(140, 20);
            this.l7.TabIndex = 7;
            this.l7.Text = "Late alert after start";
            // 
            // amber
            // 
            this.amber.Location = new System.Drawing.Point(165, 129);
            this.amber.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.amber.Name = "amber";
            this.amber.Size = new System.Drawing.Size(62, 20);
            this.amber.TabIndex = 8;
            // 
            // lateAlert
            // 
            this.lateAlert.Location = new System.Drawing.Point(235, 129);
            this.lateAlert.Name = "lateAlert";
            this.lateAlert.Size = new System.Drawing.Size(85, 20);
            this.lateAlert.TabIndex = 9;
            this.lateAlert.Text = "Use late alert";
            // 
            // skipCancelledMeetings
            // 
            this.skipCancelledMeetings.Location = new System.Drawing.Point(165, 156);
            this.skipCancelledMeetings.Name = "skipCancelledMeetings";
            this.skipCancelledMeetings.Size = new System.Drawing.Size(280, 20);
            this.skipCancelledMeetings.TabIndex = 10;
            this.skipCancelledMeetings.Text = "Skip cancelled meetings today";
            // 
            // gActions
            // 
            this.gActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gActions.Controls.Add(this.l8);
            this.gActions.Controls.Add(this.fadeDelay);
            this.gActions.Controls.Add(this.fadeDuration);
            this.gActions.Controls.Add(this.l23);
            this.gActions.Controls.Add(this.appearStart);
            this.gActions.Controls.Add(this.appearDuration);
            this.gActions.Controls.Add(this.l9);
            this.gActions.Controls.Add(this.clickAction);
            this.gActions.Controls.Add(this.l10);
            this.gActions.Controls.Add(this.doubleClickAction);
            this.gActions.Controls.Add(this.l11);
            this.gActions.Controls.Add(this.hoverAction);
            this.gActions.Location = new System.Drawing.Point(9, 203);
            this.gActions.Name = "gActions";
            this.gActions.Size = new System.Drawing.Size(490, 176);
            this.gActions.TabIndex = 1;
            this.gActions.TabStop = false;
            this.gActions.Text = "Meeting details and pointer actions";
            // 
            // l8
            // 
            this.l8.Location = new System.Drawing.Point(15, 25);
            this.l8.Name = "l8";
            this.l8.Size = new System.Drawing.Size(140, 20);
            this.l8.TabIndex = 0;
            this.l8.Text = "Close delay / fade";
            // 
            // fadeDelay
            // 
            this.fadeDelay.Location = new System.Drawing.Point(165, 22);
            this.fadeDelay.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.fadeDelay.Name = "fadeDelay";
            this.fadeDelay.Size = new System.Drawing.Size(62, 20);
            this.fadeDelay.TabIndex = 1;
            // 
            // fadeDuration
            // 
            this.fadeDuration.Location = new System.Drawing.Point(235, 22);
            this.fadeDuration.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.fadeDuration.Name = "fadeDuration";
            this.fadeDuration.Size = new System.Drawing.Size(62, 20);
            this.fadeDuration.TabIndex = 2;
            // 
            // l23
            // 
            this.l23.Location = new System.Drawing.Point(15, 55);
            this.l23.Name = "l23";
            this.l23.Size = new System.Drawing.Size(140, 20);
            this.l23.TabIndex = 3;
            this.l23.Text = "Appear: start % / fade";
            // 
            // appearStart
            // 
            this.appearStart.Location = new System.Drawing.Point(165, 52);
            this.appearStart.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.appearStart.Name = "appearStart";
            this.appearStart.Size = new System.Drawing.Size(62, 20);
            this.appearStart.TabIndex = 4;
            this.appearStart.Value = new decimal(new int[] {
            85,
            0,
            0,
            0});
            // 
            // appearDuration
            // 
            this.appearDuration.Location = new System.Drawing.Point(235, 52);
            this.appearDuration.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.appearDuration.Name = "appearDuration";
            this.appearDuration.Size = new System.Drawing.Size(62, 20);
            this.appearDuration.TabIndex = 5;
            this.appearDuration.Value = new decimal(new int[] {
            180,
            0,
            0,
            0});
            // 
            // l9
            // 
            this.l9.Location = new System.Drawing.Point(15, 85);
            this.l9.Name = "l9";
            this.l9.Size = new System.Drawing.Size(140, 20);
            this.l9.TabIndex = 6;
            this.l9.Text = "Click";
            // 
            // clickAction
            // 
            this.clickAction.Items.AddRange(new object[] {
            "[Select click action]"});
            this.clickAction.Location = new System.Drawing.Point(165, 82);
            this.clickAction.Name = "clickAction";
            this.clickAction.Size = new System.Drawing.Size(185, 21);
            this.clickAction.TabIndex = 7;
            // 
            // l10
            // 
            this.l10.Location = new System.Drawing.Point(15, 115);
            this.l10.Name = "l10";
            this.l10.Size = new System.Drawing.Size(140, 20);
            this.l10.TabIndex = 8;
            this.l10.Text = "Double-click";
            // 
            // doubleClickAction
            // 
            this.doubleClickAction.Items.AddRange(new object[] {
            "[Select double-click action]"});
            this.doubleClickAction.Location = new System.Drawing.Point(165, 112);
            this.doubleClickAction.Name = "doubleClickAction";
            this.doubleClickAction.Size = new System.Drawing.Size(185, 21);
            this.doubleClickAction.TabIndex = 9;
            // 
            // l11
            // 
            this.l11.Location = new System.Drawing.Point(15, 145);
            this.l11.Name = "l11";
            this.l11.Size = new System.Drawing.Size(140, 20);
            this.l11.TabIndex = 10;
            this.l11.Text = "Hover";
            // 
            // hoverAction
            // 
            this.hoverAction.Items.AddRange(new object[] {
            "[Select hover action]"});
            this.hoverAction.Location = new System.Drawing.Point(165, 142);
            this.hoverAction.Name = "hoverAction";
            this.hoverAction.Size = new System.Drawing.Size(185, 21);
            this.hoverAction.TabIndex = 11;
            // 
            // appearancePage
            // 
            this.appearancePage.AutoScroll = true;
            this.appearancePage.Controls.Add(this.gPreview);
            this.appearancePage.Controls.Add(this.gCard);
            this.appearancePage.Controls.Add(this.gText);
            this.appearancePage.Controls.Add(this.gColours);
            this.appearancePage.Controls.Add(this.gEmpty);
            this.appearancePage.Location = new System.Drawing.Point(4, 22);
            this.appearancePage.Name = "appearancePage";
            this.appearancePage.Size = new System.Drawing.Size(502, 786);
            this.appearancePage.TabIndex = 2;
            this.appearancePage.Text = "Appearance";
            this.appearancePage.Click += new System.EventHandler(this.appearancePage_Click);
            // 
            // gPreview
            // 
            this.gPreview.Controls.Add(this.l12);
            this.gPreview.Controls.Add(this.preset);
            this.gPreview.Controls.Add(this.appearancePreview);
            this.gPreview.Location = new System.Drawing.Point(9, 8);
            this.gPreview.Name = "gPreview";
            this.gPreview.Size = new System.Drawing.Size(490, 202);
            this.gPreview.TabIndex = 0;
            this.gPreview.TabStop = false;
            this.gPreview.Text = "Live preview";
            // 
            // l12
            // 
            this.l12.Location = new System.Drawing.Point(15, 25);
            this.l12.Name = "l12";
            this.l12.Size = new System.Drawing.Size(41, 20);
            this.l12.TabIndex = 0;
            this.l12.Text = "Theme";
            this.l12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // preset
            // 
            this.preset.Items.AddRange(new object[] {
            "[Select theme]"});
            this.preset.Location = new System.Drawing.Point(62, 25);
            this.preset.Name = "preset";
            this.preset.Size = new System.Drawing.Size(170, 21);
            this.preset.TabIndex = 1;
            // 
            // appearancePreview
            //
            this.appearancePreview.Location = new System.Drawing.Point(15, 52);
            this.appearancePreview.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.appearancePreview.Name = "appearancePreview";
            this.appearancePreview.Size = new System.Drawing.Size(457, 143);
            this.appearancePreview.TabIndex = 2;
            // 
            // gCard
            //
            this.gCard.AutoSize = true;
            this.gCard.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gCard.Controls.Add(this.l13);
            this.gCard.Controls.Add(this.opacity);
            this.gCard.Controls.Add(this.noStripBackground);
            this.gCard.Controls.Add(this.autoTaskbarTextColors);
            this.gCard.Controls.Add(this.shadow);
            this.gCard.Controls.Add(this.callout);
            this.gCard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gCard.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gCard.Location = new System.Drawing.Point(9, 217);
            this.gCard.Name = "gCard";
            this.gCard.Size = new System.Drawing.Size(164, 170);
            this.gCard.TabIndex = 1;
            this.gCard.TabStop = false;
            this.gCard.Text = "Card and strip";
            // 
            // l13
            // 
            this.l13.Location = new System.Drawing.Point(9, 25);
            this.l13.Name = "l13";
            this.l13.Size = new System.Drawing.Size(51, 20);
            this.l13.TabIndex = 0;
            this.l13.Text = "Opacity";
            this.l13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // opacity
            // 
            this.opacity.Location = new System.Drawing.Point(66, 25);
            this.opacity.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.opacity.Minimum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.opacity.Name = "opacity";
            this.opacity.Size = new System.Drawing.Size(62, 20);
            this.opacity.TabIndex = 1;
            this.opacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.opacity.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // noStripBackground
            // 
            this.noStripBackground.Location = new System.Drawing.Point(12, 52);
            this.noStripBackground.Name = "noStripBackground";
            this.noStripBackground.Size = new System.Drawing.Size(143, 20);
            this.noStripBackground.TabIndex = 2;
            this.noStripBackground.Text = "No taskbar background";
            //
            // autoTaskbarTextColors
            //
            this.autoTaskbarTextColors.Location = new System.Drawing.Point(12, 79);
            this.autoTaskbarTextColors.Name = "autoTaskbarTextColors";
            this.autoTaskbarTextColors.Size = new System.Drawing.Size(146, 20);
            this.autoTaskbarTextColors.TabIndex = 3;
            this.autoTaskbarTextColors.Text = "Automatic text contrast";
            //
            // shadow
            // 
            this.shadow.Location = new System.Drawing.Point(12, 105);
            this.shadow.Name = "shadow";
            this.shadow.Size = new System.Drawing.Size(110, 20);
            this.shadow.TabIndex = 4;
            this.shadow.Text = "Card shadow";
            // 
            // callout
            // 
            this.callout.Location = new System.Drawing.Point(12, 131);
            this.callout.Name = "callout";
            this.callout.Size = new System.Drawing.Size(130, 20);
            this.callout.TabIndex = 5;
            this.callout.Text = "Callout triangle";
            // 
            // gText
            // 
            this.gText.AutoSize = true;
            this.gText.Controls.Add(this.l14);
            this.gText.Controls.Add(this.textElement);
            this.gText.Controls.Add(this.l15);
            this.gText.Controls.Add(this.textColour);
            this.gText.Controls.Add(this.textBold);
            this.gText.Controls.Add(this.textItalic);
            this.gText.Controls.Add(this.textUnderline);
            this.gText.Controls.Add(this.l16);
            this.gText.Controls.Add(this.fontPicker);
            this.gText.Location = new System.Drawing.Point(176, 217);
            this.gText.Name = "gText";
            this.gText.Size = new System.Drawing.Size(321, 170);
            this.gText.TabIndex = 2;
            this.gText.TabStop = false;
            this.gText.Text = "Text colour and formatting";
            // 
            // l14
            // 
            this.l14.Location = new System.Drawing.Point(15, 25);
            this.l14.Name = "l14";
            this.l14.Size = new System.Drawing.Size(74, 20);
            this.l14.TabIndex = 0;
            this.l14.Text = "Text element";
            this.l14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textElement
            // 
            this.textElement.Items.AddRange(new object[] {
            "[Select text element]"});
            this.textElement.Location = new System.Drawing.Point(95, 25);
            this.textElement.Name = "textElement";
            this.textElement.Size = new System.Drawing.Size(210, 21);
            this.textElement.TabIndex = 1;
            // 
            // l15
            // 
            this.l15.Location = new System.Drawing.Point(15, 52);
            this.l15.Name = "l15";
            this.l15.Size = new System.Drawing.Size(74, 20);
            this.l15.TabIndex = 2;
            this.l15.Text = "Colour";
            this.l15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textColour
            // 
            this.textColour.Location = new System.Drawing.Point(95, 50);
            this.textColour.Name = "textColour";
            this.textColour.Size = new System.Drawing.Size(80, 24);
            this.textColour.TabIndex = 3;
            this.textColour.Text = "[Selected colour]";
            // 
            // textBold
            // 
            this.textBold.Location = new System.Drawing.Point(99, 105);
            this.textBold.Name = "textBold";
            this.textBold.Size = new System.Drawing.Size(55, 20);
            this.textBold.TabIndex = 4;
            this.textBold.Text = "Bold";
            // 
            // textItalic
            // 
            this.textItalic.Location = new System.Drawing.Point(160, 105);
            this.textItalic.Name = "textItalic";
            this.textItalic.Size = new System.Drawing.Size(55, 20);
            this.textItalic.TabIndex = 5;
            this.textItalic.Text = "Italic";
            // 
            // textUnderline
            // 
            this.textUnderline.Location = new System.Drawing.Point(221, 105);
            this.textUnderline.Name = "textUnderline";
            this.textUnderline.Size = new System.Drawing.Size(75, 20);
            this.textUnderline.TabIndex = 6;
            this.textUnderline.Text = "Underline";
            // 
            // l16
            // 
            this.l16.Location = new System.Drawing.Point(15, 79);
            this.l16.Name = "l16";
            this.l16.Size = new System.Drawing.Size(71, 20);
            this.l16.TabIndex = 7;
            this.l16.Text = "Font";
            this.l16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // fontPicker
            // 
            this.fontPicker.Location = new System.Drawing.Point(95, 77);
            this.fontPicker.Name = "fontPicker";
            this.fontPicker.Size = new System.Drawing.Size(220, 24);
            this.fontPicker.TabIndex = 8;
            this.fontPicker.Text = "[Selected font]";
            // 
            // gColours
            // 
            this.gColours.Controls.Add(this.colourStripLabel);
            this.gColours.Controls.Add(this.colourStrip);
            this.gColours.Controls.Add(this.colourGreenLabel);
            this.gColours.Controls.Add(this.colourGreen);
            this.gColours.Controls.Add(this.colourAmberLabel);
            this.gColours.Controls.Add(this.colourAmber);
            this.gColours.Controls.Add(this.colourPaperLabel);
            this.gColours.Controls.Add(this.colourPaper);
            this.gColours.Controls.Add(this.colourRuleLabel);
            this.gColours.Controls.Add(this.colourRule);
            this.gColours.Controls.Add(this.colourAccentLabel);
            this.gColours.Controls.Add(this.colourAccent);
            this.gColours.Controls.Add(this.doubleMargin);
            this.gColours.Controls.Add(this.ruleThicknessLabel);
            this.gColours.Controls.Add(this.ruleThickness);
            this.gColours.Controls.Add(this.marginThicknessLabel);
            this.gColours.Controls.Add(this.marginThickness);
            this.gColours.Controls.Add(this.cornerRadiusLabel);
            this.gColours.Controls.Add(this.cornerRadius);
            this.gColours.Controls.Add(this.cornerRadiusValue);
            this.gColours.Location = new System.Drawing.Point(9, 532);
            this.gColours.Name = "gColours";
            this.gColours.Size = new System.Drawing.Size(488, 249);
            this.gColours.TabIndex = 3;
            this.gColours.TabStop = false;
            this.gColours.Text = "Paper and status colours";
            // 
            // colourStripLabel
            // 
            this.colourStripLabel.Location = new System.Drawing.Point(20, 24);
            this.colourStripLabel.Name = "colourStripLabel";
            this.colourStripLabel.Size = new System.Drawing.Size(145, 20);
            this.colourStripLabel.TabIndex = 0;
            this.colourStripLabel.Text = "Strip background";
            // 
            // colourStrip
            // 
            this.colourStrip.Location = new System.Drawing.Point(170, 22);
            this.colourStrip.Name = "colourStrip";
            this.colourStrip.Size = new System.Drawing.Size(150, 24);
            this.colourStrip.TabIndex = 0;
            this.colourStrip.Text = "[Strip background colour]";
            // 
            // colourGreenLabel
            // 
            this.colourGreenLabel.Location = new System.Drawing.Point(20, 54);
            this.colourGreenLabel.Name = "colourGreenLabel";
            this.colourGreenLabel.Size = new System.Drawing.Size(145, 20);
            this.colourGreenLabel.TabIndex = 1;
            this.colourGreenLabel.Text = "Time started";
            // 
            // colourGreen
            // 
            this.colourGreen.Location = new System.Drawing.Point(170, 52);
            this.colourGreen.Name = "colourGreen";
            this.colourGreen.Size = new System.Drawing.Size(150, 24);
            this.colourGreen.TabIndex = 1;
            this.colourGreen.Text = "[Started-time colour]";
            // 
            // colourAmberLabel
            // 
            this.colourAmberLabel.Location = new System.Drawing.Point(20, 84);
            this.colourAmberLabel.Name = "colourAmberLabel";
            this.colourAmberLabel.Size = new System.Drawing.Size(145, 20);
            this.colourAmberLabel.TabIndex = 2;
            this.colourAmberLabel.Text = "Late alert";
            // 
            // colourAmber
            // 
            this.colourAmber.Location = new System.Drawing.Point(170, 82);
            this.colourAmber.Name = "colourAmber";
            this.colourAmber.Size = new System.Drawing.Size(150, 24);
            this.colourAmber.TabIndex = 2;
            this.colourAmber.Text = "[Late alert colour]";
            // 
            // colourPaperLabel
            // 
            this.colourPaperLabel.Location = new System.Drawing.Point(20, 114);
            this.colourPaperLabel.Name = "colourPaperLabel";
            this.colourPaperLabel.Size = new System.Drawing.Size(145, 20);
            this.colourPaperLabel.TabIndex = 3;
            this.colourPaperLabel.Text = "Card paper";
            // 
            // colourPaper
            // 
            this.colourPaper.Location = new System.Drawing.Point(170, 112);
            this.colourPaper.Name = "colourPaper";
            this.colourPaper.Size = new System.Drawing.Size(150, 24);
            this.colourPaper.TabIndex = 3;
            this.colourPaper.Text = "[Card paper colour]";
            // 
            // colourRuleLabel
            // 
            this.colourRuleLabel.Location = new System.Drawing.Point(20, 144);
            this.colourRuleLabel.Name = "colourRuleLabel";
            this.colourRuleLabel.Size = new System.Drawing.Size(145, 20);
            this.colourRuleLabel.TabIndex = 4;
            this.colourRuleLabel.Text = "Rule lines";
            // 
            // colourRule
            // 
            this.colourRule.Location = new System.Drawing.Point(170, 142);
            this.colourRule.Name = "colourRule";
            this.colourRule.Size = new System.Drawing.Size(150, 24);
            this.colourRule.TabIndex = 4;
            this.colourRule.Text = "[Rule-line colour]";
            // 
            // colourAccentLabel
            // 
            this.colourAccentLabel.Location = new System.Drawing.Point(20, 174);
            this.colourAccentLabel.Name = "colourAccentLabel";
            this.colourAccentLabel.Size = new System.Drawing.Size(145, 20);
            this.colourAccentLabel.TabIndex = 5;
            this.colourAccentLabel.Text = "Accent and margin";
            // 
            // colourAccent
            // 
            this.colourAccent.Location = new System.Drawing.Point(170, 172);
            this.colourAccent.Name = "colourAccent";
            this.colourAccent.Size = new System.Drawing.Size(150, 24);
            this.colourAccent.TabIndex = 5;
            this.colourAccent.Text = "[Accent colour]";
            // 
            // doubleMargin
            // 
            this.doubleMargin.Location = new System.Drawing.Point(337, 112);
            this.doubleMargin.Name = "doubleMargin";
            this.doubleMargin.Size = new System.Drawing.Size(135, 20);
            this.doubleMargin.TabIndex = 6;
            this.doubleMargin.Text = "Double red margin";
            // 
            // ruleThicknessLabel
            // 
            this.ruleThicknessLabel.Location = new System.Drawing.Point(337, 142);
            this.ruleThicknessLabel.Name = "ruleThicknessLabel";
            this.ruleThicknessLabel.Size = new System.Drawing.Size(80, 20);
            this.ruleThicknessLabel.TabIndex = 7;
            this.ruleThicknessLabel.Text = "Rule width";
            this.ruleThicknessLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ruleThickness
            // 
            this.ruleThickness.Location = new System.Drawing.Point(420, 142);
            this.ruleThickness.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.ruleThickness.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ruleThickness.Name = "ruleThickness";
            this.ruleThickness.Size = new System.Drawing.Size(50, 20);
            this.ruleThickness.TabIndex = 8;
            this.ruleThickness.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // marginThicknessLabel
            // 
            this.marginThicknessLabel.Location = new System.Drawing.Point(337, 172);
            this.marginThicknessLabel.Name = "marginThicknessLabel";
            this.marginThicknessLabel.Size = new System.Drawing.Size(80, 20);
            this.marginThicknessLabel.TabIndex = 9;
            this.marginThicknessLabel.Text = "Margin width";
            this.marginThicknessLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // marginThickness
            // 
            this.marginThickness.Location = new System.Drawing.Point(420, 172);
            this.marginThickness.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.marginThickness.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.marginThickness.Name = "marginThickness";
            this.marginThickness.Size = new System.Drawing.Size(50, 20);
            this.marginThickness.TabIndex = 10;
            this.marginThickness.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cornerRadiusLabel
            //
            this.cornerRadiusLabel.Location = new System.Drawing.Point(20, 207);
            this.cornerRadiusLabel.Name = "cornerRadiusLabel";
            this.cornerRadiusLabel.Size = new System.Drawing.Size(94, 20);
            this.cornerRadiusLabel.TabIndex = 11;
            this.cornerRadiusLabel.Text = "Card corners";
            this.cornerRadiusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cornerRadius
            //
            this.cornerRadius.LargeChange = 2;
            this.cornerRadius.Location = new System.Drawing.Point(115, 202);
            this.cornerRadius.Maximum = 16;
            this.cornerRadius.Name = "cornerRadius";
            this.cornerRadius.Size = new System.Drawing.Size(284, 45);
            this.cornerRadius.TabIndex = 12;
            this.cornerRadius.Value = 4;
            //
            // cornerRadiusValue
            //
            this.cornerRadiusValue.Location = new System.Drawing.Point(409, 207);
            this.cornerRadiusValue.Name = "cornerRadiusValue";
            this.cornerRadiusValue.Size = new System.Drawing.Size(58, 20);
            this.cornerRadiusValue.TabIndex = 13;
            this.cornerRadiusValue.Text = "4 px";
            this.cornerRadiusValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gEmpty
            //
            this.gEmpty.AutoSize = true;
            this.gEmpty.Controls.Add(this.l17);
            this.gEmpty.Controls.Add(this.tone);
            this.gEmpty.Controls.Add(this.l18);
            this.gEmpty.Controls.Add(this.emptyMessage);
            this.gEmpty.Controls.Add(this.pickAnother);
            this.gEmpty.Controls.Add(this.emptyPreview);
            this.gEmpty.Location = new System.Drawing.Point(9, 393);
            this.gEmpty.Name = "gEmpty";
            this.gEmpty.Size = new System.Drawing.Size(488, 133);
            this.gEmpty.TabIndex = 4;
            this.gEmpty.TabStop = false;
            this.gEmpty.Text = "Empty day";
            // 
            // l17
            // 
            this.l17.Location = new System.Drawing.Point(15, 25);
            this.l17.Name = "l17";
            this.l17.Size = new System.Drawing.Size(89, 20);
            this.l17.TabIndex = 0;
            this.l17.Text = "Message style";
            this.l17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tone
            // 
            this.tone.Items.AddRange(new object[] {
            "[Select message style]"});
            this.tone.Location = new System.Drawing.Point(110, 24);
            this.tone.Name = "tone";
            this.tone.Size = new System.Drawing.Size(170, 21);
            this.tone.TabIndex = 1;
            // 
            // l18
            // 
            this.l18.Location = new System.Drawing.Point(15, 52);
            this.l18.Name = "l18";
            this.l18.Size = new System.Drawing.Size(89, 20);
            this.l18.TabIndex = 2;
            this.l18.Text = "Custom message";
            this.l18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // emptyMessage
            // 
            this.emptyMessage.Location = new System.Drawing.Point(110, 52);
            this.emptyMessage.Name = "emptyMessage";
            this.emptyMessage.Size = new System.Drawing.Size(250, 20);
            this.emptyMessage.TabIndex = 3;
            this.emptyMessage.Text = "[Custom empty-day message]";
            // 
            // pickAnother
            // 
            this.pickAnother.Location = new System.Drawing.Point(366, 50);
            this.pickAnother.Name = "pickAnother";
            this.pickAnother.Size = new System.Drawing.Size(100, 24);
            this.pickAnother.TabIndex = 4;
            this.pickAnother.Text = "Pick another";
            // 
            // emptyPreview
            // 
            this.emptyPreview.Location = new System.Drawing.Point(110, 77);
            this.emptyPreview.Name = "emptyPreview";
            this.emptyPreview.Size = new System.Drawing.Size(356, 40);
            this.emptyPreview.TabIndex = 5;
            this.emptyPreview.Text = "[Current empty-day message preview]";
            // 
            // privacyPage
            // 
            this.privacyPage.AutoScroll = true;
            this.privacyPage.Controls.Add(this.gPrivacy);
            this.privacyPage.Location = new System.Drawing.Point(4, 22);
            this.privacyPage.Name = "privacyPage";
            this.privacyPage.Size = new System.Drawing.Size(502, 761);
            this.privacyPage.TabIndex = 3;
            this.privacyPage.Text = "Privacy";
            // 
            // gPrivacy
            // 
            this.gPrivacy.Controls.Add(this.automaticPrivacy);
            this.gPrivacy.Controls.Add(this.l19);
            this.gPrivacy.Controls.Add(this.privacyDisplay);
            this.gPrivacy.Location = new System.Drawing.Point(9, 8);
            this.gPrivacy.Name = "gPrivacy";
            this.gPrivacy.Size = new System.Drawing.Size(490, 115);
            this.gPrivacy.TabIndex = 0;
            this.gPrivacy.TabStop = false;
            this.gPrivacy.Text = "Teams screen-sharing protection";
            // 
            // automaticPrivacy
            // 
            this.automaticPrivacy.Location = new System.Drawing.Point(15, 25);
            this.automaticPrivacy.Name = "automaticPrivacy";
            this.automaticPrivacy.Size = new System.Drawing.Size(450, 20);
            this.automaticPrivacy.TabIndex = 0;
            this.automaticPrivacy.Text = "Protect meeting details automatically while I present in Teams";
            // 
            // l19
            // 
            this.l19.Location = new System.Drawing.Point(15, 55);
            this.l19.Name = "l19";
            this.l19.Size = new System.Drawing.Size(140, 20);
            this.l19.TabIndex = 1;
            this.l19.Text = "When privacy is active";
            // 
            // privacyDisplay
            // 
            this.privacyDisplay.Items.AddRange(new object[] {
            "[Select privacy display]"});
            this.privacyDisplay.Location = new System.Drawing.Point(165, 52);
            this.privacyDisplay.Name = "privacyDisplay";
            this.privacyDisplay.Size = new System.Drawing.Size(170, 21);
            this.privacyDisplay.TabIndex = 2;
            // 
            // generalPage
            // 
            this.generalPage.AutoScroll = true;
            this.generalPage.Controls.Add(this.gStartup);
            this.generalPage.Controls.Add(this.gFiles);
            this.generalPage.Controls.Add(this.gAbout);
            this.generalPage.Location = new System.Drawing.Point(4, 22);
            this.generalPage.Name = "generalPage";
            this.generalPage.Size = new System.Drawing.Size(502, 761);
            this.generalPage.TabIndex = 4;
            this.generalPage.Text = "General";
            // 
            // gStartup
            // 
            this.gStartup.Controls.Add(this.startup);
            this.gStartup.Location = new System.Drawing.Point(9, 8);
            this.gStartup.Name = "gStartup";
            this.gStartup.Size = new System.Drawing.Size(490, 60);
            this.gStartup.TabIndex = 0;
            this.gStartup.TabStop = false;
            this.gStartup.Text = "Startup";
            // 
            // startup
            // 
            this.startup.Location = new System.Drawing.Point(15, 25);
            this.startup.Name = "startup";
            this.startup.Size = new System.Drawing.Size(400, 20);
            this.startup.TabIndex = 0;
            this.startup.Text = "Start Taskbar Calendar when I sign in to Windows";
            // 
            // gFiles
            // 
            this.gFiles.Controls.Add(this.settingsFolder);
            this.gFiles.Controls.Add(this.logFolder);
            this.gFiles.Location = new System.Drawing.Point(9, 76);
            this.gFiles.Name = "gFiles";
            this.gFiles.Size = new System.Drawing.Size(490, 62);
            this.gFiles.TabIndex = 1;
            this.gFiles.TabStop = false;
            this.gFiles.Text = "Files";
            // 
            // settingsFolder
            // 
            this.settingsFolder.Location = new System.Drawing.Point(15, 25);
            this.settingsFolder.Name = "settingsFolder";
            this.settingsFolder.Size = new System.Drawing.Size(125, 24);
            this.settingsFolder.TabIndex = 0;
            this.settingsFolder.Text = "Settings folder";
            // 
            // logFolder
            // 
            this.logFolder.Location = new System.Drawing.Point(148, 25);
            this.logFolder.Name = "logFolder";
            this.logFolder.Size = new System.Drawing.Size(100, 24);
            this.logFolder.TabIndex = 1;
            this.logFolder.Text = "Log folder";
            // 
            // gAbout
            // 
            this.gAbout.Controls.Add(this.l20);
            this.gAbout.Location = new System.Drawing.Point(9, 146);
            this.gAbout.Name = "gAbout";
            this.gAbout.Size = new System.Drawing.Size(490, 60);
            this.gAbout.TabIndex = 2;
            this.gAbout.TabStop = false;
            this.gAbout.Text = "About";
            // 
            // l20
            // 
            this.l20.Location = new System.Drawing.Point(15, 25);
            this.l20.Name = "l20";
            this.l20.Size = new System.Drawing.Size(467, 20);
            this.l20.TabIndex = 0;
            this.l20.Text = "Taskbar Calendar";
            this.l20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.l20.Click += new System.EventHandler(this.l20_Click);
            // 
            // l21
            // 
            this.l21.Location = new System.Drawing.Point(0, 0);
            this.l21.Name = "l21";
            this.l21.Size = new System.Drawing.Size(100, 23);
            this.l21.TabIndex = 0;
            // 
            // l22
            // 
            this.l22.Location = new System.Drawing.Point(0, 0);
            this.l22.Name = "l22";
            this.l22.Size = new System.Drawing.Size(100, 23);
            this.l22.TabIndex = 0;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(258, 6);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 24);
            this.applyButton.TabIndex = 0;
            this.applyButton.Text = "Apply";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(339, 6);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 24);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(420, 6);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 24);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            // 
            // commands
            // 
            this.commands.Controls.Add(this.applyButton);
            this.commands.Controls.Add(this.okButton);
            this.commands.Controls.Add(this.cancelButton);
            this.commands.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commands.Location = new System.Drawing.Point(0, 812);
            this.commands.Name = "commands";
            this.commands.Size = new System.Drawing.Size(510, 38);
            this.commands.TabIndex = 1;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(510, 850);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.commands);
            this.MinimumSize = new System.Drawing.Size(510, 390);
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Taskbar Calendar Properties";
            this.Tabs.ResumeLayout(false);
            this.outlookPage.ResumeLayout(false);
            this.gOutlook.ResumeLayout(false);
            this.gRefresh.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lookahead)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.refresh)).EndInit();
            this.displayPage.ResumeLayout(false);
            this.gStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.width)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amber)).EndInit();
            this.gActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fadeDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fadeDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.appearStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.appearDuration)).EndInit();
            this.appearancePage.ResumeLayout(false);
            this.appearancePage.PerformLayout();
            this.gPreview.ResumeLayout(false);
            this.gCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.opacity)).EndInit();
            this.gText.ResumeLayout(false);
            this.gColours.ResumeLayout(false);
            this.gColours.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ruleThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cornerRadius)).EndInit();
            this.gEmpty.ResumeLayout(false);
            this.gEmpty.PerformLayout();
            this.privacyPage.ResumeLayout(false);
            this.gPrivacy.ResumeLayout(false);
            this.generalPage.ResumeLayout(false);
            this.gStartup.ResumeLayout(false);
            this.gFiles.ResumeLayout(false);
            this.gAbout.ResumeLayout(false);
            this.commands.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
