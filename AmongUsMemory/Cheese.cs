using ProcessUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HamsterCheese.AmongUsMemory
{
    public static class Cheese
    {

        public static Memory.Mem mem = new Memory.Mem();
        public static ProcessMemory ProcessMemory = null;
        public static bool Is64BitAmongUs { get; private set; }
        public static bool Init()
        {
            var state = mem.OpenProcess("Among Us");

            if (state)
            {
                Process proc = Process.GetProcessesByName("Among Us")[0];
                Is64BitAmongUs = Environment.Is64BitOperatingSystem && !IsWow64Process(proc.Handle);
                if (Is64BitAmongUs)
                {
                    Console.WriteLine("Current Among Us appears to be 64-bit. This bot's memory structs were generated for the old 32-bit build, so memory automation is disabled until the structs/offsets are regenerated from the current IL2CPP dump.");
                    return false;
                }

                ProcessMemory = new ProcessMemory(proc);
                ProcessMemory.Open(ProcessAccess.AllAccess);
                Methods.Init();
                return true;
            }
            return false;
        }

        public static IntPtr ReadPointer(IntPtr address)
        {
            return mem.ReadPointer(address);
        }

        public static IntPtr ResolveAddress(string address)
        {
            return mem.ResolveAddressPublic(address);
        }

        public static IntPtr GetStaticFields(string typeInfoAddress)
        {
            var typeInfoSlot = ResolveAddress(typeInfoAddress);
            var klass = ReadPointer(typeInfoSlot);
            return klass == IntPtr.Zero ? IntPtr.Zero : ReadPointer(klass.Sum(0x5C));
        }

        public static IntPtr ReadStaticPointer(string typeInfoAddress, int staticOffset)
        {
            var staticFields = GetStaticFields(typeInfoAddress);
            return staticFields == IntPtr.Zero ? IntPtr.Zero : ReadPointer(staticFields.Sum(staticOffset));
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Winapi)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr process, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] out bool wow64Process);

        private static bool IsWow64Process(IntPtr processHandle)
        {
            bool isWow64;
            return IsWow64Process(processHandle, out isWow64) && isWow64;
        }

        private static ShipStatus prevShipStatus;
        public static ShipStatus shipStatus;

        public static AmongUsClient amongUsClient;
        public static IntPtr amongUsClientPtr;

        public static String shipStatusAddress;
        public static IntPtr shipStatusPTR;
        private static Dictionary<string, CancellationTokenSource> Tokens = new Dictionary<string, CancellationTokenSource>();
        private static System.Action<uint> onChangeShipStatus;


        static void _ObserveShipStatus()
        {
            while (Tokens.ContainsKey("ObserveShipStatus") && Tokens["ObserveShipStatus"].IsCancellationRequested == false)
            {
                Thread.Sleep(250);
                shipStatus = Cheese.GetShipStatus();
                if (prevShipStatus.OwnerId != shipStatus.OwnerId)
                {
                    prevShipStatus = shipStatus;
                    onChangeShipStatus?.Invoke(shipStatus.Type);
                    Console.WriteLine("OnShipStatusChanged");
                }
                else
                { 

                }
            }
        }

        /// <summary>
        /// Subscribe shipstatus changed.
        /// </summary>
        /// <param name="onChangeShipStatus"></param>
        public static void ObserveShipStatus(System.Action<uint> onChangeShipStatus)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            if (Tokens.ContainsKey("ObserveShipStatus"))
            {
                Tokens["ObserveShipStatus"].Cancel();
                Tokens.Remove("ObserveShipStatus");
            }

            Tokens.Add("ObserveShipStatus", cts);
            Cheese.onChangeShipStatus = onChangeShipStatus;
            Task.Factory.StartNew(_ObserveShipStatus, cts.Token);
        }


        /// <summary>
        /// Get Ship Status From AmongUs Proccess
        /// </summary>
        /// <returns></returns>
        public static ShipStatus GetShipStatus()
        { 
            ShipStatus shipStatus = new ShipStatus();
            var ptr = ReadStaticPointer(Pattern.ShipStatus_TypeInfo, 0);
            if (ptr == IntPtr.Zero)
            {
                return shipStatus;
            }

            shipStatus = Utils.FromBytes<ShipStatus>(Cheese.mem.ReadBytes(ptr.GetAddress(), Utils.SizeOf<ShipStatus>()));
            shipStatusAddress = ptr.GetAddress();
            shipStatusPTR = ptr;
            return shipStatus;
        }

        public static AmongUsClient getAmongUsClient()
        {
            AmongUsClient client = new AmongUsClient();
            var ptr = ReadStaticPointer(Pattern.AmongusClient_TypeInfo, 0);
            if (ptr == IntPtr.Zero)
            {
                return client;
            }

            client = Utils.FromBytes<AmongUsClient>(Cheese.mem.ReadBytes(ptr.GetAddress(), Utils.SizeOf<AmongUsClient>()));
            amongUsClient = client;
            amongUsClientPtr = ptr;
            return client;
        }

        public static IntPtr GetMeetingHud()
        {
            return ReadStaticPointer(Pattern.MeetingHud_TypeInfo, 0);
        }

        private static IntPtr chatControllerPtr = IntPtr.Zero;
        private static DateTime lastChatControllerScan = DateTime.MinValue;
        private static bool chatControllerScanRunning = false;
        private static readonly object chatControllerScanLock = new object();

        public static IntPtr GetChatController(bool allowBackgroundScan = false)
        {
            if (chatControllerPtr != IntPtr.Zero && IsValidChatController(chatControllerPtr))
            {
                return chatControllerPtr;
            }

            if (allowBackgroundScan)
            {
                QueueChatControllerScan();
            }

            return IntPtr.Zero;
        }

        private static void QueueChatControllerScan()
        {
            lock (chatControllerScanLock)
            {
                if (chatControllerScanRunning || (DateTime.UtcNow - lastChatControllerScan).TotalSeconds < 30)
                {
                    return;
                }

                chatControllerScanRunning = true;
                lastChatControllerScan = DateTime.UtcNow;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    ScanChatController();
                }
                finally
                {
                    lock (chatControllerScanLock)
                    {
                        chatControllerScanRunning = false;
                    }
                }
            });
        }

        private static void ScanChatController()
        {
            var klass = ReadPointer(ResolveAddress(Pattern.ChatController_TypeInfo));
            if (klass == IntPtr.Zero)
            {
                return;
            }

            var bytes = IntPtr.Size == 8
                ? BitConverter.GetBytes(klass.ToInt64())
                : BitConverter.GetBytes(klass.ToInt32());
            var pattern = string.Join(" ", bytes.Select(x => x.ToString("X2")));

            foreach (var candidate in mem.AoBScan(pattern).Result)
            {
                var ptr = new IntPtr(candidate);
                if (IsValidChatController(ptr))
                {
                    chatControllerPtr = ptr;
                    return;
                }
            }
        }

        private static bool IsValidChatController(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return false;
            }

            var nativeObject = mem.ReadInt(ptr.Sum(0x8).GetAddress());
            var state = mem.ReadInt(ptr.Sum(0x6C).GetAddress());
            var chatBubblePool = ReadPointer(ptr.Sum(0x28));
            if (nativeObject == 0 || state < 0 || state > 3 || chatBubblePool == IntPtr.Zero)
            {
                return false;
            }

            var activeChildren = ReadPointer(chatBubblePool.Sum(0x18));
            var activeCount = activeChildren == IntPtr.Zero ? -1 : mem.ReadInt(activeChildren.Sum(0xC).GetAddress());
            return activeCount >= 0 && activeCount <= 64;
        }

        public static List<string> GetVisibleChatMessages()
        {
            var messages = new List<string>();
            var chatController = GetChatController(true);
            if (chatController == IntPtr.Zero)
            {
                return messages;
            }

            var chatBubblePool = ReadPointer(chatController.Sum(0x28));
            var activeChildren = chatBubblePool == IntPtr.Zero ? IntPtr.Zero : ReadPointer(chatBubblePool.Sum(0x18));
            var items = activeChildren == IntPtr.Zero ? IntPtr.Zero : ReadPointer(activeChildren.Sum(0x8));
            var count = activeChildren == IntPtr.Zero ? 0 : mem.ReadInt(activeChildren.Sum(0xC).GetAddress());
            if (items == IntPtr.Zero || count <= 0 || count > 64)
            {
                return messages;
            }

            for (var i = 0; i < count; i++)
            {
                var bubble = ReadPointer(items.Sum(0x10 + (i * IntPtr.Size)));
                if (bubble == IntPtr.Zero)
                {
                    continue;
                }

                var textArea = ReadPointer(bubble.Sum(0x28));
                var textString = textArea == IntPtr.Zero ? IntPtr.Zero : ReadPointer(textArea.Sum(0x84));
                var text = Utils.ReadString(textString);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    messages.Add(text.Trim());
                }
            }

            return messages;
        }

        public static bool IsInMeeting()
        {
            try
            {
                var meetingHud = GetMeetingHud();
                if (meetingHud == IntPtr.Zero)
                {
                    return false;
                }

                // Unity objects can leave managed/static references behind after destroy.
                // A real active MeetingHud must still have a native object and a populated voter list.
                var nativeObject = Cheese.mem.ReadInt(meetingHud.Sum(Offset.UnityObjectNativePtr).GetAddress());
                if (nativeObject == 0)
                {
                    return false;
                }

                var playerStates = ReadPointer(meetingHud.Sum(Offset.MeetingHudPlayerStates));
                if (playerStates == IntPtr.Zero)
                {
                    return false;
                }

                var playerStateCount = Cheese.mem.ReadInt(playerStates.Sum(Offset.Il2CppListCount).GetAddress());
                if (playerStateCount <= 0 || playerStateCount > 15)
                {
                    return false;
                }

                var firstPlayerState = ReadPointer(playerStates.Sum(Offset.Il2CppArrayFirstItem));
                if (firstPlayerState == IntPtr.Zero)
                {
                    return false;
                }

                var state = Cheese.mem.ReadInt(meetingHud.Sum(Offset.MeetingHudState).GetAddress());
                return state >= 0 && state <= 5;
            }
            catch
            {
                return false;
            }
        }

        public static int GetMeetingVoteState()
        {
            var meetingHud = GetMeetingHud();
            return meetingHud == IntPtr.Zero ? -1 : Cheese.mem.ReadInt(meetingHud.Sum(Offset.MeetingHudState).GetAddress());
        }

        public static int GetMeetingVoteButtonIndex(byte playerId)
        {
            var meetingHud = GetMeetingHud();
            if (meetingHud == IntPtr.Zero)
            {
                return -1;
            }

            var playerStates = ReadPointer(meetingHud.Sum(Offset.MeetingHudPlayerStates));
            if (playerStates == IntPtr.Zero)
            {
                return -1;
            }

            var count = Cheese.mem.ReadInt(playerStates.Sum(0xC).GetAddress());
            if (count <= 0 || count > 32)
            {
                return -1;
            }

            for (var i = 0; i < count; i++)
            {
                var voteArea = ReadPointer(playerStates.Sum(0x10 + (i * IntPtr.Size)));
                if (voteArea == IntPtr.Zero)
                {
                    continue;
                }

                var targetPlayerId = Cheese.mem.ReadByte(voteArea.Sum(0x14).GetAddress());
                if (targetPlayerId == playerId)
                {
                    return i + 1; // 0 is skip vote; player buttons start at 1.
                }
            }

            return -1;
        }

        public static IntPtr GetLocalPlayer()
        {
            return ReadStaticPointer(Pattern.PlayerControl_TypeInfo, 0);
        }

        public static IntPtr ReadPlayerControlPointer(IntPtr playerControlPtr, int offset)
        {
            return playerControlPtr == IntPtr.Zero ? IntPtr.Zero : ReadPointer(playerControlPtr.Sum(offset));
        }

        public static float ReadRoleFloat(IntPtr rolePtr, int offset)
        {
            return rolePtr == IntPtr.Zero ? 0 : mem.ReadFloat(rolePtr.Sum(offset).GetAddress());
        }

        public static bool ReadRoleBool(IntPtr rolePtr, int offset)
        {
            return rolePtr != IntPtr.Zero && mem.ReadByte(rolePtr.Sum(offset).GetAddress()) != 0;
        }
         

        public static string MakeAobString(byte[] aobTarget, int length, string unknownText = "?? ?? ?? ??")
        {
            int cnt = 0;
            // aob pattern
            string aobData = "";
            // read 4byte aob pattern.
            foreach (var _byte in aobTarget)
            {
                if (_byte < 16)
                    aobData += "0" + _byte.ToString("X");
                else
                    aobData += _byte.ToString("X");

                if (cnt + 1 != 4)
                    aobData += " ";

                cnt++;
                if (cnt == length)
                {
                    aobData += $" {unknownText}";
                    break;
                }
            }
            return aobData;
        }

        /// <summary>
        /// Get All Players From AmongUs Proccess
        /// </summary>
        /// <returns></returns>
        public static List<PlayerData> GetAllPlayers()
        {
            List<PlayerData > datas = new List<PlayerData>();

            var list = ReadStaticPointer(Pattern.PlayerControl_TypeInfo, 4);
            if (list == IntPtr.Zero)
            {
                return datas;
            }

            var items = ReadPointer(list.Sum(0x8));
            var size = Cheese.mem.ReadInt(list.Sum(0xC).GetAddress());
            if (items == IntPtr.Zero || size <= 0 || size > 64)
            {
                return datas;
            }

            for (var i = 0; i < size; i++)
            {
                var playerPtr = ReadPointer(items.Sum(0x10 + (i * IntPtr.Size)));
                if (playerPtr == IntPtr.Zero)
                {
                    continue;
                }

                var bytes = Cheese.mem.ReadBytes(playerPtr.GetAddress(), Utils.SizeOf<PlayerControl>());
                var playerControl = Utils.FromBytes<PlayerControl>(bytes);
                if (playerControl.PlayerId >= byte.MaxValue || playerControl.NetId >= uint.MaxValue - 10000)
                {
                    continue;
                }

                datas.Add(new PlayerData()
                {
                    Instance = playerControl,
                    PlayerControllPTROffset = playerPtr.GetAddress(),
                    PlayerControllPTR = playerPtr
                });
            }
            Console.WriteLine("data => " + datas.Count);
            return datas;
        }


    }
}
