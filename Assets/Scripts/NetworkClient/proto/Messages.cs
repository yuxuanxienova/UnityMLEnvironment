using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//---------------System Message----------------
public class MsgPing : MsgBase
{
    public MsgPing() { protoName = "MsgPing"; }
}

public class MsgPong : MsgBase
{
    public MsgPong() { protoName = "MsgPong"; }
}


//--------------Test Message----------
public class MsgTest : MsgBase
{
    public MsgTest() { protoName = "MsgTest"; }
    public string str;
    public int num;
}
