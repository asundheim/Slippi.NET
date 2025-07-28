using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace Windows.Win32;

internal static partial class PInvoke
{
    public static int GET_X_LPARAM(LPARAM lp) => unchecked((short)(long)lp);
    public static int GET_Y_LPARAM(LPARAM lp) => unchecked((short)((long)lp >> 16));
}
