using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;


namespace LiveSplit.UI.Components
{
    public class SeparatePbTrackerComponent : IComponent
    {
        // Interface-required properties
        public string ComponentName => "Separate RT and IGT PB Tracker";

        public float HorizontalWidth => 0;
        public float VerticalHeight => 0;

        public float MinimumWidth => 0;
        public float MinimumHeight => 0;

        public float PaddingTop => 0;
        public float PaddingBottom => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;

        public IDictionary<string, Action> ContextMenuControls => null; // I don't know what this is for, but we don't need it.

        // Settings that the user will be able to set for our component.
        public SeparatePbTrackerSettings Settings { get; set; }

        // Object containing all of the current information about LiveSplit (the run, splits, timer, layout, settings, etc).
        protected LiveSplitState CurrentState { get; set; }

        // Helper object to keep a save of the information about the run, since `CurrentState` updates the splits before dispatching the `OnReset` event.
        private IRun SavedCurrentRun { get; set; }

        // The constructor is called when LiveSplit creates your component. This happens when the component
        // is added to the layout, or when LiveSplit opens a layout with this component already added.
        public SeparatePbTrackerComponent(LiveSplitState state)
        {
            Settings = new SeparatePbTrackerSettings(state); // Initialize the settings for our component.
            CurrentState = state; // Keep a reference to the LiveSplite state.

            state.OnReset += state_OnReset; // Suscribe to the `OnReset` event, so that our method `state_OnReset` executes whenever said event is dispatched.
            state.OnSplit += state_OnSplit; // Suscribe to the `OnSplit` event, so that our method `state_OnSplit` executes whenever said event is dispatched.
        }

        // This function is called when the component is removed from the layout, or when LiveSplit closes a layout with this component in it.
        public void Dispose()
        {
            // Unsuscribe from all suscribed events and clean the `SavedCurrentRun` property to free space.
            CurrentState.OnReset -= state_OnReset;
            CurrentState.OnSplit -= state_OnSplit;
            SavedCurrentRun = null;
        }

        // This function is called hundreds to thousands of times per second, and it's where we decide what needs to be displayed at each frame.
        // This method doesn't do anything in this case since this component doesn't need to display anything on screen.
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode) { }

        // Every time a split occurs, we update `SavedCurrentRun` with the just-updated information of the run.
        void state_OnSplit(object sender, EventArgs e)
        {
            SavedCurrentRun = (Run)CurrentState.Run.Clone();
        }

        // When the user resets the splits, we check if either the PB IGT or PB RT were beaten. If they were, we update
        // them (so if both were beaten, we update both, but if only one was beaten, we update only that one).
        void state_OnReset(object sender, TimerPhase e)
        {
            // Get the IGT and RT time of the current last split from the saved splits.
            TimeSpan? currentIgtTime = SavedCurrentRun.Last().SplitTime[TimingMethod.GameTime];
            TimeSpan? currentRtTime = SavedCurrentRun.Last().SplitTime[TimingMethod.RealTime];

            // Get the IGT and RT PB of the last split from the saved splits.
            TimeSpan? pbIgtTime = SavedCurrentRun.Last().Comparisons[Settings.PbComparisonName][TimingMethod.GameTime];
            TimeSpan? pbRtTime = SavedCurrentRun.Last().Comparisons[Settings.PbComparisonName][TimingMethod.RealTime];

            // If there is no registered IGT PB, or if the current split is better than the registered IGT PB,
            // then we update ONLY IGT time for all splits, keeping the existing RT time.
            if ((currentIgtTime != null && pbIgtTime == null) || currentIgtTime < pbIgtTime)
            {
                var update = true;

                if (CurrentState.Settings.WarnOnReset && MessageBox.Show("You have beaten your IGT PB.\r\nDo you want to update it?", "Update IGT PB?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    update = false;
                }

                if (update)
                {
                    for (int i = 0; i < CurrentState.Run.Count(); i++)
                    {
                        CurrentState.Run[i].Comparisons[Settings.PbComparisonName] = new Time(SavedCurrentRun[i].PersonalBestSplitTime[TimingMethod.RealTime], SavedCurrentRun[i].SplitTime[TimingMethod.GameTime]);
                    }
                }
            }

            // If there is no registered RT PB, or if the current split is better than the registered RT PB,
            // then we update ONLY RT time for all splits, keeping the existing IGT time.
            if ((currentRtTime != null && pbRtTime == null) || currentRtTime < pbRtTime)
            {
                var update = true;

                if (CurrentState.Settings.WarnOnReset && MessageBox.Show("You have beaten your RT PB.\r\nDo you want to update it?", "Update RT PB?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    update = false;
                }

                if (update)
                {
                    for (int i = 0; i < CurrentState.Run.Count(); i++)
                    {
                        CurrentState.Run[i].Comparisons[Settings.PbComparisonName] = new Time(SavedCurrentRun[i].SplitTime[TimingMethod.RealTime], SavedCurrentRun[i].PersonalBestSplitTime[TimingMethod.GameTime]);
                    }
                }
            }
        }
        
        // Some more interface-required methods. These first two methods don't do anything in this case since this component doesn't need to display anything on screen.
        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) {}

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) {}

        public XmlNode GetSettings(XmlDocument document) => Settings.GetSettings(document);

        public Control GetSettingsControl(LayoutMode mode) => Settings;

        public void SetSettings(XmlNode settings) => Settings.SetSettings(settings);
    }
}
