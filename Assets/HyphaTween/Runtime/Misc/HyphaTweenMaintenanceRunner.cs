namespace AtMycelia.HyphaTween
{
    /// <summary>
    /// HyphaTween's hook into the Atelier Mycelia Common lib's MaintenanceRunner.
    /// The runner discovers this automatically, so no [RuntimeInitializeOnLoadMethod]
    /// is needed here.
    /// </summary>
    public class HyphaTweenMaintenanceRunner : MaintenanceModule
    {
        // Lower values run first, and the Common lib's CoreStartupMaintenance uses
        // int.MinValue, so the AtMycelia root is guaranteed to exist by the time we run.
        public override int Priority => DefaultPriority;

        public override void RunStartup()
        {
            HyphaTweenRuntimeBootstrapper.EnsureHyphaTweenHierarchy();
        }
    }
}
