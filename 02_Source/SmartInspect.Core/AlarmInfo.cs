namespace SmartInspect.Core;

public enum AlarmLevel { Info, Warning, Error }
public sealed record AlarmInfo(string Code, AlarmLevel Level, string Message, DateTime Time);
