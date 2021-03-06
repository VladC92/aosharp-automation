using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace MultiboxHelper.IPCMessages
{
    [AoContract((int)IPCOpcode.Floor)]
    public class FloorMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Floor;

        [AoMember(0)]
        public int Floor { get; set; }

    }
}
