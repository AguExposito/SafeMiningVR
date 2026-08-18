using System;
using System.Collections.Generic;

[Serializable]
public class PendingSyncQueueData
{
    public List<PendingSyncEntry> entries = new List<PendingSyncEntry>();
}
