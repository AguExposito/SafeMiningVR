using System;

[Serializable]
public class PendingSyncEntry
{
    public const string EntryTypeStart = "Start";
    public const string EntryTypeEvent = "Event";
    public const string EntryTypeComplete = "Complete";

    public string entryId;
    public string entryType;
    public string payloadJson;
    public string queuedAtUtc;
    public int retryCount;
    public string lastError;
}
