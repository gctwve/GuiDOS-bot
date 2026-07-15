using System;
using System.Runtime.InteropServices;

[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
public struct ShipStatus
{
    [System.Runtime.InteropServices.FieldOffset(0xD8)] public uint SomethingUnknown;
    [System.Runtime.InteropServices.FieldOffset(0)]     public uint instance;
    [System.Runtime.InteropServices.FieldOffset(4)]     public uint _null;
    [System.Runtime.InteropServices.FieldOffset(8)]     public uint m_CachedPtr;
    [System.Runtime.InteropServices.FieldOffset(12)]    public uint SpawnId;
    [System.Runtime.InteropServices.FieldOffset(16)]    public uint NetId;
    [System.Runtime.InteropServices.FieldOffset(20)]    public uint DirtyBits;
    [System.Runtime.InteropServices.FieldOffset(24)]    public uint SpawnFlags;
    [System.Runtime.InteropServices.FieldOffset(25)]    public uint sendMode;
    [System.Runtime.InteropServices.FieldOffset(28)]    public uint OwnerId;
    [System.Runtime.InteropServices.FieldOffset(32)]    public byte DespawnOnDestroy;
    [System.Runtime.InteropServices.FieldOffset(40)]    public uint CameraColor;
    [System.Runtime.InteropServices.FieldOffset(56)]    public float MaxLightRadius;
    [System.Runtime.InteropServices.FieldOffset(60)]    public float MinLightRadius;
    [System.Runtime.InteropServices.FieldOffset(64)]    public float MapScale;
    [System.Runtime.InteropServices.FieldOffset(68)]    public IntPtr MapPrefab;
    [System.Runtime.InteropServices.FieldOffset(72)]    public IntPtr ExileCutscenePrefab;
    [System.Runtime.InteropServices.FieldOffset(96)]    public uint InitialSpawnCenter;
    [System.Runtime.InteropServices.FieldOffset(104)]   public uint MeetingSpawnCenter;
    [System.Runtime.InteropServices.FieldOffset(112)]   public uint MeetingSpawnCenter2;
    [System.Runtime.InteropServices.FieldOffset(120)]   public float SpawnRadius;
    [System.Runtime.InteropServices.FieldOffset(124)]   public IntPtr CommonTasks;
    [System.Runtime.InteropServices.FieldOffset(128)]   public IntPtr LongTasks;
    [System.Runtime.InteropServices.FieldOffset(132)]   public IntPtr NormalTasks;
    [System.Runtime.InteropServices.FieldOffset(136)]   public IntPtr SpecialTasks;
    [System.Runtime.InteropServices.FieldOffset(140)]   public IntPtr DummyLocations;
    [System.Runtime.InteropServices.FieldOffset(144)]   public IntPtr AllCameras;
    [System.Runtime.InteropServices.FieldOffset(148)]   public IntPtr AllDoors;
    [System.Runtime.InteropServices.FieldOffset(152)]   public IntPtr AllConsoles;
    [System.Runtime.InteropServices.FieldOffset(160)]   public IntPtr Systems;
    [System.Runtime.InteropServices.FieldOffset(172)]   public IntPtr AllStepWatchers;
    [System.Runtime.InteropServices.FieldOffset(176)]   public IntPtr AllRooms;
    [System.Runtime.InteropServices.FieldOffset(180)]   public IntPtr FastRooms;
    [System.Runtime.InteropServices.FieldOffset(184)]   public IntPtr AllVents;
    [System.Runtime.InteropServices.FieldOffset(192)]   public IntPtr WeaponFires;
    [System.Runtime.InteropServices.FieldOffset(196)]   public IntPtr WeaponsImage;
    [System.Runtime.InteropServices.FieldOffset(200)]   public IntPtr VentMoveSounds;
    [System.Runtime.InteropServices.FieldOffset(204)]   public IntPtr VentEnterSound;
    [System.Runtime.InteropServices.FieldOffset(212)]   public IntPtr HatchActive;
    [System.Runtime.InteropServices.FieldOffset(216)]   public IntPtr Hatch;
    [System.Runtime.InteropServices.FieldOffset(220)]   public IntPtr HatchParticles;
    [System.Runtime.InteropServices.FieldOffset(224)]   public IntPtr ShieldsActive;
    [System.Runtime.InteropServices.FieldOffset(228)]   public IntPtr ShieldsImages;
    [System.Runtime.InteropServices.FieldOffset(232)]   public IntPtr ShieldBorder;
    [System.Runtime.InteropServices.FieldOffset(236)]   public IntPtr ShieldBorderOn;
    [System.Runtime.InteropServices.FieldOffset(240)]   public IntPtr MedScanner;
    [System.Runtime.InteropServices.FieldOffset(244)]   public uint WeaponFireIdx;
    [System.Runtime.InteropServices.FieldOffset(248)]   public float Timer;
    [System.Runtime.InteropServices.FieldOffset(252)]   public float EmergencyCooldown;
    [System.Runtime.InteropServices.FieldOffset(256)]   public uint Type;
}
