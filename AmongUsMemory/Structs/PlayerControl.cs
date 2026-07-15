using System; 
 using System.Runtime.InteropServices;

 [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
public struct PlayerControl{
[System.Runtime.InteropServices.FieldOffset(8)]	public uint m_CachedPtr;
[System.Runtime.InteropServices.FieldOffset(12)]	public uint SpawnId;
[System.Runtime.InteropServices.FieldOffset(16)]	public uint NetId;
[System.Runtime.InteropServices.FieldOffset(20)]	public uint DirtyBits;
[System.Runtime.InteropServices.FieldOffset(24)]	public uint SpawnFlags;
[System.Runtime.InteropServices.FieldOffset(25)]	public uint sendMode;
[System.Runtime.InteropServices.FieldOffset(28)]	public uint OwnerId;
[System.Runtime.InteropServices.FieldOffset(32)]	public byte DespawnOnDestroy;
[System.Runtime.InteropServices.FieldOffset(36)]	public uint LastStartCounter;
[System.Runtime.InteropServices.FieldOffset(40)]	public byte PlayerId;
[System.Runtime.InteropServices.FieldOffset(44)]	public IntPtr FriendCode;
[System.Runtime.InteropServices.FieldOffset(48)]	public IntPtr Puid;
[System.Runtime.InteropServices.FieldOffset(52)]	public float MaxReportDistance;
[System.Runtime.InteropServices.FieldOffset(56)]	public byte moveable;
[System.Runtime.InteropServices.FieldOffset(60)]	public IntPtr cosmetics;
[System.Runtime.InteropServices.FieldOffset(72)]	public byte inVent;
[System.Runtime.InteropServices.FieldOffset(88)]	public IntPtr _cachedData;
[System.Runtime.InteropServices.FieldOffset(116)]	public IntPtr FootSteps;
[System.Runtime.InteropServices.FieldOffset(120)]	public IntPtr KillSfx;
[System.Runtime.InteropServices.FieldOffset(124)]	public IntPtr KillAnimations;
[System.Runtime.InteropServices.FieldOffset(128)]	public float killTimer;
[System.Runtime.InteropServices.FieldOffset(132)]	public uint RemainingEmergencies;
[System.Runtime.InteropServices.FieldOffset(136)]	public IntPtr LightPrefab;
[System.Runtime.InteropServices.FieldOffset(140)]	public IntPtr myLight;
[System.Runtime.InteropServices.FieldOffset(144)]	public IntPtr Collider;
[System.Runtime.InteropServices.FieldOffset(148)]	public IntPtr MyPhysics;
[System.Runtime.InteropServices.FieldOffset(152)]	public IntPtr NetTransform;
[System.Runtime.InteropServices.FieldOffset(172)]	public IntPtr myTasks;
[System.Runtime.InteropServices.FieldOffset(196)]	public IntPtr hitBuffer;
[System.Runtime.InteropServices.FieldOffset(200)]	public IntPtr closest;
[System.Runtime.InteropServices.FieldOffset(204)]	public byte isNew;
[System.Runtime.InteropServices.FieldOffset(212)]	public IntPtr cache;
[System.Runtime.InteropServices.FieldOffset(216)]	public IntPtr itemsInRange;
[System.Runtime.InteropServices.FieldOffset(220)]	public IntPtr newItemsInRange;
[System.Runtime.InteropServices.FieldOffset(224)]	public byte scannerCount;
}
