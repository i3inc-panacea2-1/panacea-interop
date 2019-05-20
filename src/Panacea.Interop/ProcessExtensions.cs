using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Interop
{
    public static class ProcessExtensions
    {
        public static void BindToCurrentProcess(this Process child)
        {
            ChildProcessTracker.AddProcess(child);
        }
    }
}
