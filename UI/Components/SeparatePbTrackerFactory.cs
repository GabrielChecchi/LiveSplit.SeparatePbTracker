﻿using LiveSplit.Model;
using System;

namespace LiveSplit.UI.Components
{
    public class SeparatePbTrackerFactory : IComponentFactory
    {
        public string ComponentName => "Separate RT and IGT PB Tracker";

        public string Description => "Keeps track of your RT PB and IGT PB separately, saving them in a new Comparison called 'PB'";

        public ComponentCategory Category => ComponentCategory.Other;

        public IComponent Create(LiveSplitState state) => new SeparatePbTrackerComponent(state);

        public string UpdateName => ComponentName;

        public string UpdateURL => "";

        public string XMLURL => UpdateURL + "Components/update.LiveSplit.SeparatePbTracker.xml";

        public Version Version => Version.Parse("1.0.0");
    }
}