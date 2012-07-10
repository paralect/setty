using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SettyVsAddin
{
    static class GuidList
    {
        public const string guidSettyVsAddinPkgString = "7cc0d490-3f59-4e50-967b-f21140c991e3";
        public const string guidCommandSetString = "5fef0364-e747-48dd-88d7-70f1b0823ab0";

        public static readonly Guid guidSettyVsAddinCmdSet = new Guid(guidCommandSetString);
    };
}
