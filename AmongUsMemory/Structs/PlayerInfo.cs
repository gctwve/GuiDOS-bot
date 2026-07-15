using System; 
 using System.Runtime.InteropServices;

 [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
public struct PlayerInfo{
[System.Runtime.InteropServices.FieldOffset(40)]	public byte PlayerId;
[System.Runtime.InteropServices.FieldOffset(44)]	public int ClientId;
[System.Runtime.InteropServices.FieldOffset(48)]	public IntPtr FriendCode;
[System.Runtime.InteropServices.FieldOffset(52)]	public IntPtr Puid;
[System.Runtime.InteropServices.FieldOffset(56)]	public ushort RoleType;
[System.Runtime.InteropServices.FieldOffset(64)]	public IntPtr Outfits;
[System.Runtime.InteropServices.FieldOffset(68)]	public uint PlayerLevel;
[System.Runtime.InteropServices.FieldOffset(72)]	public byte Disconnected;
[System.Runtime.InteropServices.FieldOffset(76)]	public IntPtr Role;
[System.Runtime.InteropServices.FieldOffset(80)]	public IntPtr Tasks;
[System.Runtime.InteropServices.FieldOffset(84)]	public byte IsDead;
[System.Runtime.InteropServices.FieldOffset(85)]	public byte WasEjected;
[System.Runtime.InteropServices.FieldOffset(88)]	public IntPtr _object;

// Compatibility fields filled by PlayerData after reading NetworkedPlayerInfo.
[System.Runtime.InteropServices.FieldOffset(92)]	public IntPtr PlayerName;
[System.Runtime.InteropServices.FieldOffset(96)]	public byte ColorId;
[System.Runtime.InteropServices.FieldOffset(97)]	public byte IsImpostor;
}
