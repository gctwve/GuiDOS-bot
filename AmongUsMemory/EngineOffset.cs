namespace HamsterCheese.AmongUsMemory
{
    public sealed class Pattern
    {
        
        // Among Us v17.4s build 7044
        public static string AmongusClient_TypeInfo = "GameAssembly.dll+2AAEA30";
        public static string PlayerControl_TypeInfo = "GameAssembly.dll+2AC2BF0";
        public static string ShipStatus_TypeInfo = "GameAssembly.dll+2AD5BE8";
        public static string NetworkedPlayerInfo_TypeInfo = "GameAssembly.dll+2AB52BC";
        public static string MeetingHud_TypeInfo = "GameAssembly.dll+2AAF37C";
        public static string ChatController_TypeInfo = "GameAssembly.dll+2AB8E90";

        public static string PlayerControl_GetData = "GameAssembly.dll+5FBAD0";
        public static string ShipStatus_CalculateLightRadius = "GameAssembly.dll+666850";
        public static string NetworkedPlayerInfo_GetPlayerName = "GameAssembly.dll+816BB0";


    }

    public static class Offset
    {
        // Unity/IL2CPP collection layout used by List<T> and arrays in this dump.
        public const int Il2CppListItems = 0x8;
        public const int Il2CppListCount = 0xC;
        public const int Il2CppArrayFirstItem = 0x10;

        // PlayerControl fields, Among Us v17.4s build 7044.
        public const int PlayerControlMyTasks = 0xAC;
        public const int PlayerControlClosestUsable = 0xC8;
        public const int PlayerControlItemsInRange = 0xD8;
        public const int PlayerControlNewItemsInRange = 0xDC;

        // PlayerTask fields used by the task resolver.
        public const int PlayerTaskStartAt = 0x1C;
        public const int PlayerTaskTaskType = 0x20;
        public const int NormalPlayerTaskTaskStep = 0x30;
        public const int NormalPlayerTaskMaxStep = 0x34;
        public const int DivertPowerTaskTargetSystem = 0x60;

        // RoleBehaviour-derived role fields, Among Us v17.4s build 7044.
        public const int PhantomCooldownSecondsRemaining = 0x7C;
        public const int PhantomDurationSecondsRemaining = 0x80;
        public const int PhantomIsInvisible = 0x84;
        public const int PhantomIsFading = 0x85;
        public const int PhantomServerApproved = 0x86;

        public const int ShapeshifterCooldownSecondsRemaining = 0x8C;
        public const int ShapeshifterDurationSecondsRemaining = 0x90;

        // MeetingHud fields.
        public const int UnityObjectNativePtr = 0x8;
        public const int MeetingHudPlayerStates = 0x5C;
        public const int MeetingHudState = 0x88;
    }
} 
  
