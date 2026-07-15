# Among Us v17.4s build 7044 memory map

Generated from:

- `C:\Program Files (x86)\Steam\steamapps\common\Among Us\GameAssembly.dll`
- `C:\Program Files (x86)\Steam\steamapps\common\Among Us\Among Us_Data\il2cpp_data\Metadata\global-metadata.dat`

Il2CppDumper:

- Metadata Version: 31
- Il2Cpp Version: 31
- CodeRegistration: `0x12373CF0`
- MetadataRegistration: `0x1276FE90`

## TypeInfo RVAs

These are `GameAssembly.dll+RVA` addresses for IL2CPP TypeInfo pointer symbols.
For static fields, read the TypeInfo pointer, then read `klass + 0x5C` for `static_fields`.

```text
AmongUsClient_TypeInfo       = GameAssembly.dll+2AAEA30
PlayerControl_TypeInfo       = GameAssembly.dll+2AC2BF0
ShipStatus_TypeInfo          = GameAssembly.dll+2AD5BE8
NetworkedPlayerInfo_TypeInfo = GameAssembly.dll+2AB52BC
```

## Static Fields

`Il2CppClass.static_fields` offset: `0x5C` on this 32-bit build.

```text
AmongUsClient.StaticFields.Instance       = static_fields+0x0
ShipStatus.StaticFields.Instance          = static_fields+0x0
PlayerControl.StaticFields.LocalPlayer    = static_fields+0x0
PlayerControl.StaticFields.AllPlayerControls = static_fields+0x4
```

## Method RVAs

```text
PlayerControl.get_Data              = GameAssembly.dll+5FBAD0
ShipStatus.CalculateLightRadius     = GameAssembly.dll+666850
NetworkedPlayerInfo.get_PlayerName  = GameAssembly.dll+816BB0
GameData.GetPlayerById              = GameAssembly.dll+80D3A0
```

## Current Field Offsets

All object offsets include the 32-bit IL2CPP object header.

### InnerNetObject base

```text
SpawnId          0x10
NetId            0x14
DirtyBits        0x18
SpawnFlags       0x1C
sendMode         0x1D
OwnerId          0x20
DespawnOnDestroy 0x24
```

### PlayerControl

```text
PlayerId             0x28
FriendCode           0x2C
Puid                 0x30
MaxReportDistance    0x34
moveable             0x38
inVent               0x48
CachedPlayerData     0x58
FootSteps            0x74
KillSfx              0x78
KillAnimations       0x7C
killTimer            0x80
RemainingEmergencies 0x84
LightPrefab          0x88
lightSource          0x8C
Collider             0x90
MyPhysics            0x94
NetTransform         0x98
myTasks              0xAC
hitBuffer            0xC4
closest              0xC8
isNew                0xCC
cache                0xD4
itemsInRange         0xD8
newItemsInRange      0xDC
scannerCount         0xE0
```

### CosmeticsLayer

```text
bodySprites       0x28
colorBlindText    0x2C
hat               0x30
nameText          0x34
nameTextContainer 0x38
petParent         0x3C
skin              0x40
visor             0x44
currentPet        0x6C
visible           0x81
isNameVisible     0x82
localPlayer       0x84
```

### NetworkedPlayerInfo

```text
PlayerId     0x28
ClientId     0x2C
FriendCode   0x30
Puid         0x34
RoleType     0x38
Outfits      0x40
PlayerLevel  0x44
Disconnected 0x48
Role         0x4C
Tasks        0x50
IsDead       0x54
WasEjected   0x55
_object      0x58
```

### ShipStatus

```text
CameraColor      0x28
MaxLightRadius   0x38
MinLightRadius   0x3C
MapScale         0x40
MapPrefab        0x44
SpawnRadius      0x78
CommonTasks      0x7C
LongTasks        0x80
ShortTasks       0x84
SpecialTasks     0x88
DummyLocations   0x8C
AllCameras       0x90
AllDoors         0x94
AllConsoles      0x98
Ladders          0x9C
Systems          0xA0
AllStepWatchers  0xAC
AllRooms         0xB0
FastRooms        0xB4
AllVents         0xB8
WeaponFires      0xC0
WeaponsImage     0xC4
VentMoveSounds   0xC8
VentEnterSound   0xCC
HatchActive      0xD4
Hatch            0xD8
HatchParticles   0xDC
ShieldsActive    0xE0
ShieldsImages    0xE4
ShieldBorder     0xE8
ShieldBorderOn   0xEC
MedScanner       0xF0
WeaponFireIdx    0xF4
Timer            0xF8
EmergencyCooldown 0xFC
Type             0x100
```

### CustomNetworkTransform

```text
myPlayer          0x28
body              0x2C
sendQueue         0x30
incomingPosQueue  0x34
rubberbandModifier 0x38
idealSpeed        0x3C
isPaused          0x40
lastSequenceId    0x42
lastPosition      0x44
lastPosSent       0x4C
tempSnapPosition  0x54
```

### PlayerPhysics

```text
ImpostorDiscoveredSound 0x28
Animations              0x2C
inputHandler            0x30
Speed                   0x34
GhostSpeed              0x38
body                    0x40
myPlayer                0x44
bodyType                0x48
petCoroutine            0x4C
DoingCustomAnimation    0x50
lastClimbLadderSid      0x51
```

### LightSource

```text
viewDistance            0x10
lightCutawayMaterial    0x14
lightChildMesh          0x18
rendererType            0x1C
useFlashlight           0x20
gpuShadowCasterMaterial 0x24
gpuShadowmapResolution  0x28
gpuPreferredRTFormat    0x2C
raycastMinRayCount      0x30
raycastTolerance        0x34
renderer                0x38
lightChild              0x3C
lightChildMeshFilter    0x40
controller              0x44
flashlightSize          0x48
lastFlashlightDirection 0x4C
lightOffset             0x54
touchFlashlightTarget   0x60
```

## Live pointer snapshot

Captured while Among Us was running in `OnlineGame` before the ship scene was active:

```text
GameAssembly base: 0x784E0000
AmongUsClient.StaticFields: 0x3260CE68
PlayerControl.StaticFields: 0x2AAE1E10
ShipStatus.StaticFields: 0x3309AB88

AmongUsClient.Instance: 0x3263AE00
PlayerControl.LocalPlayer: 0x293FC0F0
PlayerControl.AllPlayerControls: 0x2A629800
ShipStatus.Instance: 0x00000000
```

`ShipStatus.Instance` was zero in this snapshot. Capture again after the ship scene/round is loaded to get the live ship object.

## Live round pointer snapshot

Captured in a loaded ship round at `2026-07-15T13:59:20-04:00`.

```text
GameAssembly base: 0x784E0000
AmongUsClient.StaticFields: 0x3260CE68
PlayerControl.StaticFields: 0x2AAE1E10
ShipStatus.StaticFields: 0x3309AB88

AmongUsClient.Instance: 0x3263AE00
PlayerControl.LocalPlayer: 0x293FC0F0
PlayerControl.AllPlayerControls: 0x2A629800
ShipStatus.Instance: 0x23BEB980

AmongUsClient.GameState: 2
ShipStatus.Type: 0
ShipStatus.MapScale: 4.4
ShipStatus.SpawnRadius: 1.6
ShipStatus.AllVents: 0x32723280
ShipStatus.AllRooms: 0x326E14E0
ShipStatus.Systems: 0x34E5A498
```
