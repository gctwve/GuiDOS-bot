using System; 
 using System.Runtime.InteropServices;

 [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
public struct AmongUsClient{
[System.Runtime.InteropServices.FieldOffset(8)]	public uint m_CachedPtr;
[System.Runtime.InteropServices.FieldOffset(20)]	public IntPtr networkAddress;
[System.Runtime.InteropServices.FieldOffset(24)]	public uint networkPort;
[System.Runtime.InteropServices.FieldOffset(28)]	public byte useDtls;
[System.Runtime.InteropServices.FieldOffset(32)]	public IntPtr connection;
[System.Runtime.InteropServices.FieldOffset(36)]	public uint mode;
[System.Runtime.InteropServices.FieldOffset(40)]	public uint NetworkMode;
[System.Runtime.InteropServices.FieldOffset(44)]	public int GameId;
[System.Runtime.InteropServices.FieldOffset(48)]	public int HostId;
[System.Runtime.InteropServices.FieldOffset(52)]	public int ClientId;
[System.Runtime.InteropServices.FieldOffset(56)]	public IntPtr allClients;
[System.Runtime.InteropServices.FieldOffset(96)]	public byte IsGamePublic;
[System.Runtime.InteropServices.FieldOffset(100)]	public uint GameState;
[System.Runtime.InteropServices.FieldOffset(116)]	public float MinSendInterval;
[System.Runtime.InteropServices.FieldOffset(120)]	public uint NetIdCnt;
[System.Runtime.InteropServices.FieldOffset(124)]	public float timer;
[System.Runtime.InteropServices.FieldOffset(128)]	public IntPtr SpawnableObjects;
[System.Runtime.InteropServices.FieldOffset(132)]	public IntPtr NonAddressableSpawnableObjects;
[System.Runtime.InteropServices.FieldOffset(136)]	public IntPtr allObjects;
[System.Runtime.InteropServices.FieldOffset(140)]	public byte InOnlineScene;
[System.Runtime.InteropServices.FieldOffset(176)]	public IntPtr OnlineScene;
[System.Runtime.InteropServices.FieldOffset(180)]	public IntPtr MainMenuScene;
[System.Runtime.InteropServices.FieldOffset(184)]	public IntPtr GameDataPrefab;
[System.Runtime.InteropServices.FieldOffset(188)]	public IntPtr VoteBanPrefab;
[System.Runtime.InteropServices.FieldOffset(192)]	public IntPtr PlayerPrefab;
[System.Runtime.InteropServices.FieldOffset(196)]	public IntPtr ShipPrefabs;
[System.Runtime.InteropServices.FieldOffset(200)]	public uint TutorialMapId;
[System.Runtime.InteropServices.FieldOffset(204)]	public float SpawnRadius;
[System.Runtime.InteropServices.FieldOffset(208)]	public uint discoverState;
[System.Runtime.InteropServices.FieldOffset(212)]	public IntPtr DisconnectHandlers;
[System.Runtime.InteropServices.FieldOffset(216)]	public IntPtr GameListHandlers;
}
