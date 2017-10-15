﻿using System;

namespace Cake.Hg.Versions
{
    public class HgIncrementVersionSettings : HgVersionSettings
    {
        /// <summary>
        /// Branch name.
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// Project version increment strategy.
        /// </summary>
        public Func<Version, Version> Increment { get; set; }
        
        /// <summary>
        /// .ctor
        /// </summary>
        public HgIncrementVersionSettings() : base()
        {
            Branch = "default";
            Increment = DefaultIncrement;
        }

        private static Version DefaultIncrement(Version version)
        {
            var major = version.Major;
            var minor = version.Minor;
            var build = version.Build;
            var revision = version.Revision;

            if (revision != -1)
                return new Version(major, minor, build, revision + 1);

            if (build != -1)
                return new Version(major, minor, build + 1);

            return new Version(major, minor + 1);
        }
    }
}
