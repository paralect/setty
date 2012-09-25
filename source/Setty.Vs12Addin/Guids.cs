// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.Setty_Vs12Addin
{
    static class GuidList
    {
        public const string guidSetty_Vs12AddinPkgString = "9dc86b8b-3af3-4301-9289-16072f9dfbf9";
        public const string guidSetty_Vs12AddinCmdSetString = "2896545f-b70c-4dac-8dc4-871300a564a3";

        public static readonly Guid guidSetty_Vs12AddinCmdSet = new Guid(guidSetty_Vs12AddinCmdSetString);
    };
}