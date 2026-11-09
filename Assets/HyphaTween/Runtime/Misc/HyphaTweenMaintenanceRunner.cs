namespace AtMycelia.HyphaTween
{
    /// <summary>
    /// HyphaTween's hook into the Atelier Mycelia Common lib's MaintenanceRunner.
    /// The runner discovers this automatically, so no [RuntimeInitializeOnLoadMethod]
    /// is needed here.
    /// </summary>
    public class RuntimeMaintenanceRunner : MaintenanceModule
    {
        // Lower values run first, and the Common lib's CoreStartupMaintenance uses
        // int.MinValue, so the AtMycelia root is guaranteed to exist by the time we run.
        public override int Priority => MaintenanceOrder.StartPriority;

        public override void RunStartup()
        {
            RuntimeBootstrapper.EnsureHierarchy();
        }
    }
    
    /// <summary>
    /// For other packages that want to time their maintenance relative
    /// to this package's.
    /// </summary>
    public static class MaintenanceOrder
    {
        internal static int StartPriority => 0;
        public static int EndPriority => StartPriority + 1;
    }
}
