

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HamsterCheese.AmongUsMemory
{
    public class PlayerData
    {
        #region ObserveStates
        private bool observe_dieFlag = false;
        #endregion


        /// <summary>
        /// Player Control Instance
        /// </summary>
        public PlayerControl Instance;

        /// <summary>
        /// Player Die Event
        /// </summary>
        public System.Action<Vector2, byte> onDie;  

        /// <summary>
        /// Player Info Pointer&Offset
        /// </summary>
        public string PlayerInfoPTR = null;
        public IntPtr PlayerInfoPTROffset;

        /// <summary>
        /// Player Controll Pointer&Offset
        /// </summary>
        public IntPtr PlayerControllPTR;
        public string PlayerControllPTROffset;


        Dictionary<string, CancellationTokenSource> Tokens = new Dictionary<string, CancellationTokenSource>();


        /*[Obsolete] 
        public void ObserveState()
        {
            if (PlayerInfo.HasValue)
            {
                if (observe_dieFlag == false && PlayerInfo.Value.IsDead == 1)
                {
                    observe_dieFlag = true;
                    onDie?.Invoke(Position, PlayerInfo.Value.ColorId);
                }
            }
        }*/


        /// <summary>
        /// PlayerInfo 가져오기 
        /// </summary>
        public PlayerInfo? PlayerInfo
        {
            get
            {
                if (PlayerInfoPTROffset == IntPtr.Zero)
                {
                    var ptr = Instance._cachedData;
                    if (ptr == IntPtr.Zero)
                    {
                        ReadMemory();
                        ptr = Instance._cachedData;
                    }

                    if (ptr == IntPtr.Zero)
                    {
                        return null;
                    }

                    PlayerInfoPTR = ptr.GetAddress();
                    PlayerInfo pInfo = Utils.FromBytes<PlayerInfo>(Cheese.mem.ReadBytes(PlayerInfoPTR, Utils.SizeOf<PlayerInfo>()));
                    PlayerInfoPTROffset = ptr;
                    playerInfo = HydratePlayerInfo(pInfo);
                    return (PlayerInfo)playerInfo;

                }
                else
                {
                    PlayerInfo pInfo = Utils.FromBytes<PlayerInfo>(Cheese.mem.ReadBytes(PlayerInfoPTR, Utils.SizeOf<PlayerInfo>()));
                    playerInfo = HydratePlayerInfo(pInfo);
                    return (PlayerInfo)playerInfo;
                }

            }
        }
        private PlayerInfo? playerInfo = null;

        private PlayerInfo HydratePlayerInfo(PlayerInfo info)
        {
            info.IsImpostor = IsImpostorRole(info.RoleType) ? (byte)1 : (byte)0;
            var outfit = ReadDefaultOutfit(info.Outfits);
            if (outfit.HasValue)
            {
                info.ColorId = outfit.Value.ColorId;
                info.PlayerName = outfit.Value.PlayerName;
            }

            return info;
        }

        private static bool IsImpostorRole(ushort roleType)
        {
            return roleType == 1 || roleType == 5 || roleType == 7 || roleType == 9 || roleType == 18;
        }

        private struct PlayerOutfitSnapshot
        {
            public byte ColorId;
            public IntPtr PlayerName;
        }

        private PlayerOutfitSnapshot? ReadDefaultOutfit(IntPtr outfits)
        {
            if (outfits == IntPtr.Zero)
            {
                return null;
            }

            var entries = Cheese.ReadPointer(outfits.Sum(0xC));
            var count = Cheese.mem.ReadInt(outfits.Sum(0x10).GetAddress());
            if (entries == IntPtr.Zero || count <= 0 || count > 32)
            {
                return null;
            }

            PlayerOutfitSnapshot? first = null;
            for (var i = 0; i < count; i++)
            {
                var entry = entries.Sum(0x10 + (i * 0x10));
                var hashCode = Cheese.mem.ReadInt(entry.GetAddress());
                var key = Cheese.mem.ReadByte(entry.Sum(0x8).GetAddress());
                var value = Cheese.ReadPointer(entry.Sum(0xC));
                if (hashCode < 0 || value == IntPtr.Zero)
                {
                    continue;
                }

                var snapshot = new PlayerOutfitSnapshot()
                {
                    ColorId = (byte)Cheese.mem.ReadByte(value.Sum(0x8).GetAddress()),
                    PlayerName = Cheese.ReadPointer(value.Sum(0x20))
                };

                if (key == 0)
                {
                    return snapshot;
                }

                if (!first.HasValue)
                {
                    first = snapshot;
                }
            }

            return first;
        }

        
        public LightSource LightSource
        {
            get
            {
                var lsPtr = Instance.myLight;
                Console.WriteLine("light source : " + lsPtr.GetAddress());
                var lsBytes = Cheese.mem.ReadBytes(lsPtr.GetAddress(), Utils.SizeOf<LightSource>());
                var ls = Utils.FromBytes<LightSource>(lsBytes);
                return ls; 
            }
        }
        //public void WriteMemory_LightRange(float value)
        //{
        //    var targetPointer = Utils.GetMemberPointer(Instance.myLight, typeof(LightSource), "LightRadius");
        //    Cheese.mem.WriteMemory(targetPointer.GetAddress(), "float", value.ToString("0.0"));
        //}

        /// <summary>
        /// Set Player Impostor State. *Client Side
        /// </summary>
        /// <param name="value"></param> 
        //public void WriteMemory_Impostor(byte value)
        //{
        //    var targetPointer = Utils.GetMemberPointer(PlayerInfoPTROffset, typeof(PlayerInfo), "IsImpostor");
        //    Cheese.mem.WriteMemory(targetPointer.GetAddress(), "byte", value.ToString());
        //}

       
        /// <summary>
        /// Set Player Dead State. *Client Side
        /// </summary>
        /// <param name="value"></param>
        //public void WriteMemory_IsDead(byte value)
        //{
        //    var targetPointer = Utils.GetMemberPointer(PlayerInfoPTROffset, typeof(PlayerInfo), "IsDead");
        //    Cheese.mem.WriteMemory(targetPointer.GetAddress(), "byte", value.ToString());
        //}
        /// <summary>
        /// Set Player KillTimer
        /// </summary>
        /// <param name="value"></param>
        //public void WriteMemory_KillTimer(float value)
        //{
        //    var targetPointer = Utils.GetMemberPointer(PlayerControllPTR, typeof(PlayerControl), "killTimer");
        //    Cheese.mem.WriteMemory(targetPointer.GetAddress(), "float", value.ToString());
       // }
        /// <summary>
        /// Set Player KillTimer
        /// </summary>
        /// <param name="value"></param>
        //public void WriteMemory_SetNameTextColor(Color value)
        //{
        //    var targetPointer = Utils.GetMemberPointer(Instance.nameText, typeof(TextRenderer), "Color");
         //   Cheese.mem.WriteMemory(targetPointer.GetAddress(), "float", value.r.ToString("0.0"));
         //   Cheese.mem.WriteMemory((targetPointer + 4).GetAddress(), "float", value.g.ToString("0.0"));
         //   Cheese.mem.WriteMemory((targetPointer + 8).GetAddress(), "float", value.b.ToString("0.0"));
         //   Cheese.mem.WriteMemory((targetPointer + 12).GetAddress(), "float", value.a.ToString("0.0"));
        //}
        public float getKillTimer()
        {
            var targetPointer = Utils.GetMemberPointer(PlayerControllPTR, typeof(PlayerControl), "killTimer");
            return Cheese.mem.ReadFloat(targetPointer.GetAddress());
        }
         
        public bool inVent()
        {
            var targetPointer = Utils.GetMemberPointer(PlayerControllPTR, typeof(PlayerControl), "inVent");
            int val = Cheese.mem.ReadByte(targetPointer.GetAddress());
            return System.Convert.ToBoolean(val);
        }

        public uint remainingEmergencies()
        {
            var targetPointer = Utils.GetMemberPointer(PlayerControllPTR, typeof(PlayerControl), "RemainingEmergencies");
            uint val = (uint) Cheese.mem.ReadInt(targetPointer.GetAddress());
            return val;
        }

        public bool movable()
        {
            var targetPointer = Utils.GetMemberPointer(PlayerControllPTR, typeof(PlayerControl), "moveable");
            int val = Cheese.mem.ReadByte(targetPointer.GetAddress());
            return System.Convert.ToBoolean(val);
        }

        public void StopObserveState()
        {
            var key = Tokens.ContainsKey("ObserveState");
            if(key)
            {
                if (Tokens["ObserveState"].IsCancellationRequested == false)
                {
                    Tokens["ObserveState"].Cancel();
                    Tokens.Remove("ObserveState");
                }
            } 
        }
        public void StartObserveState()
        {
            if(Tokens.ContainsKey("ObserveState"))
            {
                Console.WriteLine("Already Observed!");
                return;
            }
            else
            {
                CancellationTokenSource cts = new CancellationTokenSource(); 
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var playerInfo = PlayerInfo;
                        if (playerInfo.HasValue)
                        {
                            if (observe_dieFlag == false && playerInfo.Value.IsDead == 1)
                            {
                                observe_dieFlag = true;
                                onDie?.Invoke(Position, playerInfo.Value.ColorId);
                            }
                        }
                        System.Threading.Thread.Sleep(25); 
                    }
                }, cts.Token);

                Tokens.Add("ObserveState", cts);
            }
          
        }

        public Vector2 Position
        {
            get
            {
                if (IsLocalPlayer)
                    return GetMyPosition();
                else
                    return GetSyncPosition();
            }
        }

        public void ReadMemory()
        {
            Instance = Utils.FromBytes<PlayerControl>(Cheese.mem.ReadBytes(PlayerControllPTROffset, Utils.SizeOf<PlayerControl>()));
        }

        public bool IsLocalPlayer
        {
            get
            {
                var localPlayer = Cheese.GetLocalPlayer();
                if (localPlayer != IntPtr.Zero)
                {
                    return PlayerControllPTR == localPlayer;
                }

                return Instance.myLight != IntPtr.Zero;
            }
        }


        Vector2 GetSyncPosition()
        {
            try
            {
                int _offset_vec2_position = 0x44;
                int _offset_vec2_sizeOf = 8;
                var netTransform = Instance.NetTransform.Sum(_offset_vec2_position).GetAddress();
                var vec2Data= Cheese.mem.ReadBytes($"{netTransform}",_offset_vec2_sizeOf); // 주소로부터 8바이트 읽는다   
                if (vec2Data != null && vec2Data.Length != 0)
                {
                    var vec2 = Utils.FromBytes<Vector2>(vec2Data);
                    return vec2;
                }
                else
                {
                    return Vector2.Zero;
                }
            }


            catch (Exception e)
            {
                Console.WriteLine(e);
                return Vector2.Zero;
            }
        }
        Vector2 GetMyPosition()
        {
            try
            {
                int _offset_vec2_position = 0x4C;
                int _offset_vec2_sizeOf = 8;
                var netTransform = Instance.NetTransform.Sum(_offset_vec2_position).GetAddress();
                var vec2Data= Cheese.mem.ReadBytes($"{netTransform}",_offset_vec2_sizeOf); // 주소로부터 8바이트 읽는다  
                if (vec2Data != null && vec2Data.Length != 0)
                {
                    var vec2 = Utils.FromBytes<Vector2>(vec2Data);
                    return vec2;
                }
                else
                {
                    return Vector2.Zero;
                }
            }
            catch
            {
                return Vector2.Zero;
            }
        }


 

    }
}
