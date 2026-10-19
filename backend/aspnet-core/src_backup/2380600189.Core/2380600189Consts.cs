using 2380600189.Debugging;

namespace 2380600189;

public class 2380600189Consts
{
    public const string LocalizationSourceName = "2380600189";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "1de8210f2ef242438bdc77c3e439e3e3";
}
