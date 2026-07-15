using System; 
 using System.Runtime.InteropServices;

 [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
public struct LightSource{
[System.Runtime.InteropServices.FieldOffset(8)]	public uint m_CachedPtr;
[System.Runtime.InteropServices.FieldOffset(16)]	public float LightRadius;
[System.Runtime.InteropServices.FieldOffset(20)]	public IntPtr Material;
[System.Runtime.InteropServices.FieldOffset(24)]	public IntPtr myMesh;
[System.Runtime.InteropServices.FieldOffset(28)]	public uint rendererType;
[System.Runtime.InteropServices.FieldOffset(32)]	public byte useFlashlight;
[System.Runtime.InteropServices.FieldOffset(48)]	public uint MinRays;
[System.Runtime.InteropServices.FieldOffset(52)]	public float tol;
[System.Runtime.InteropServices.FieldOffset(56)]	public IntPtr renderer;
[System.Runtime.InteropServices.FieldOffset(60)]	public IntPtr child;
}
