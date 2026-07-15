using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace HamsterCheese.AmongUsMemory
{
    public static class Utils
    {
        static Dictionary<(Type, string), int> _offsetMap = new Dictionary<(Type, string), int>();

        public static T FromBytes<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length < SizeOf<T>())
            {
                return default(T);
            }

            GCHandle gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var data = (T)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            gcHandle.Free();
            return data;
        }

        public static int SizeOf<T>()
        {
            var size = Marshal.SizeOf(typeof(T));
            return size;
        }


        public static string GetAddress(this long value) { return value.ToString("X"); }
        public static string GetAddress(this int value) { return value.ToString("X"); }
        public static string GetAddress(this uint value) { return value.ToString("X"); }
        public static string GetAddress(this IntPtr value) { return value.ToInt64().ToString("X"); }
        public static string GetAddress(this UIntPtr value) { return value.ToUInt64().ToString("X"); }

        public static IntPtr Sum(this IntPtr ptr, IntPtr ptr2) { return new IntPtr(ptr.ToInt64() + ptr2.ToInt64()); }
        public static IntPtr Sum(this IntPtr ptr, UIntPtr ptr2) { return new IntPtr(ptr.ToInt64() + unchecked((long)ptr2.ToUInt64())); }
        public static IntPtr Sum(this UIntPtr ptr, IntPtr ptr2) { return new IntPtr(unchecked((long)ptr.ToUInt64()) + ptr2.ToInt64()); }
        public static IntPtr Sum(this int ptr, IntPtr ptr2) { return new IntPtr(ptr + ptr2.ToInt64()); }
        public static IntPtr Sum(this IntPtr ptr, int ptr2) { return new IntPtr(ptr.ToInt64() + ptr2); }

        public static IntPtr GetMemberPointer(IntPtr basePtr, Type type, string fieldName)
        {
            var offset = GetOffset(type, fieldName); 
            return basePtr.Sum(offset);
        }
        public static int GetOffset(Type type, string fieldName)
        {
            if (_offsetMap.ContainsKey((type, fieldName)))
            {
                return _offsetMap[(type, fieldName)];
            }
            var field = type.GetField(fieldName);
            var atts = field.GetCustomAttributes(true);
            foreach (var att in atts)
            {
                if (att.GetType() == typeof(FieldOffsetAttribute))
                {
                    _offsetMap.Add((type, fieldName), (att as FieldOffsetAttribute).Value);
                    return (att as FieldOffsetAttribute).Value;
                }
            }

            return -1;
        }

        /// <summary>
        /// Support All Language.
        /// </summary> 
        public static string ReadString(IntPtr offset)
        {
            if (offset == IntPtr.Zero)
            {
                return string.Empty;
            }

            var lengthOffset = IntPtr.Size == 8 ? 0x10 : 8;
            var charsOffset = IntPtr.Size == 8 ? 0x14 : 12;
            var length = HamsterCheese.AmongUsMemory.Cheese.mem.ReadInt(offset.Sum(lengthOffset).GetAddress());
            if (length <= 0 || length > 512)
            {
                return string.Empty;
            }

            //unit of string is 2byte.
            var format_length = length * 2;

            var strByte = HamsterCheese.AmongUsMemory.Cheese.mem.ReadBytes(offset.Sum(charsOffset).GetAddress(), format_length);
            if (strByte == null || strByte.Length < format_length)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(); 
            for (int i = 0; i < strByte.Length; i += 2)
            {
                // english = 1byte
                if (strByte[i + 1] == 0) 
                    sb.Append((char)strByte[i]); 
                // korean & unicode = 2byte
                else
                    sb.Append(System.Text.Encoding.Unicode.GetString(new byte[] { strByte[i], strByte[i + 1] }));
            }

            return sb.ToString();
        }

    }
}
