using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slippi.NET.Common;
public interface IEvent<TEventArgs>
{
    string Event { get; }

    TEventArgs Args { get; }
}
