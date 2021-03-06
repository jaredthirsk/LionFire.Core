﻿using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Notifications
{
    public class NotificationProfile : IKeyed<string>
    {

        public string Name { get; set; }

        #region Construction

        public NotificationProfile() { }
        public NotificationProfile(string name)
        {
            this.Name = name;

        }

        #endregion

        public bool Modal { get; set; }

        public string DefaultTitle { get; set; }
        public Importance Importance { get; set; }
        public Urgency Urgency { get; set; }

        public List<Escalation> Escalations { get; set; }

        public string Sound { get; set; }

        /// <summary>
        /// If true, for the last Escalation (if any), continue using that profile's list of escalations.  This causes the last one to be chained.
        /// </summary>
        public bool ContinueWithEscalationProfileEscalationSequence { get; set; } = true;

        string IKeyed<string>.Key => Name;
    }

    public class SoundProfile
    {

    }

    public class Escalation
    {
        public int MillisecondsDelay { get; set; }
        public string NotificationProfile { get; set; }

    }
}
