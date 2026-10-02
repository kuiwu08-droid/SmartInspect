namespace SmartInspect.Communication
{
    public static class PlcRegisterMap
    {
        public const ushort Trigger = 0;
        public const ushort Busy = 1;
        public const ushort Result = 2;
        public const ushort Done = 3;
        public const ushort Heartbeat = 4;
    }

    public static class PlcResultCode
    {
        public const ushort None = 0;
        public const ushort Ok = 1;
        public const ushort Ng = 2;
    }
}