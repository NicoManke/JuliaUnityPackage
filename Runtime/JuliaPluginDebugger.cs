namespace JuliaPlugin
{
    /// <summary>
    /// Interface that allows the user to implement and use his own debugger for the JuliaPlugin's classes.
    /// </summary>
    public interface IJuliaPluginDebugger
    {
        public abstract void DisplayMessage(string message);
        public abstract void DisplayWarning(string message);
        public abstract void DisplayError(string message);
    }
}
