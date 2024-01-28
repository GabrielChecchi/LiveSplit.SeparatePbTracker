using LiveSplit.Model;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class SeparatePbTrackerSettings : UserControl
    {
        public string PbComparisonName { get; set; }

        private LiveSplitState CurrentState { get; set; }

        public SeparatePbTrackerSettings(LiveSplitState state)
        {
            InitializeComponent();

            PbComparisonName = "Separate PB";
            CurrentState = state;

            CreateComparison();
        }

        private void UpdatablePBSettings_Load(object sender, EventArgs e) {}

        private void CreateComparison()
        {
            if (!CurrentState.Run.Comparisons.Contains(PbComparisonName))
            {
                CurrentState.Run.CustomComparisons.Add(PbComparisonName);

                foreach (var segment in CurrentState.Run)
                {
                    segment.Comparisons[PbComparisonName] = segment.PersonalBestSplitTime;
                }
            }
        }

        private void RenameComparison(string newComparisonName)
        {
            if (CurrentState.Run.Comparisons.Contains(PbComparisonName) && !CurrentState.Run.Comparisons.Contains(newComparisonName))
            {
                CurrentState.Run.CustomComparisons[CurrentState.Run.CustomComparisons.IndexOf(PbComparisonName)] = newComparisonName;

                foreach (var segment in CurrentState.Run)
                {
                    segment.Comparisons[newComparisonName] = segment.Comparisons[PbComparisonName];
                    segment.Comparisons.Remove(PbComparisonName);
                }

                if (CurrentState.CurrentComparison == PbComparisonName)
                    CurrentState.CurrentComparison = newComparisonName;

                PbComparisonName = newComparisonName;
            }
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            //PbComparisonName = SettingsHelper.ParseString(element["PbComparisonName"]);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.0");
        }
    }
}
