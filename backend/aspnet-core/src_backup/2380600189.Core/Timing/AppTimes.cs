using Abp.Dependency;
using System;

namespace 2380600189.Timing;

public class AppTimes : ISingletonDependency
{
    /// <summary>
    /// Gets the startup time of the application.
    /// </summary>
    public DateTime StartupTime { get; set; }
}
