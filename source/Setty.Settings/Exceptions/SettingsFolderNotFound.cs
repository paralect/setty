using System;

namespace Setty.Settings.Exceptions
{
    public class SettingsFolderNotFound : Exception
    {
        public SettingsFolderNotFound() {}
        public SettingsFolderNotFound(string message) : base(message) {}
        public SettingsFolderNotFound(string message, Exception innerException) : base(message, innerException) {}
    }
}
