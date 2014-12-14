// zrbj/DAITools_CRCTest.cs 
// 2014-12-14 DarthGizka

using System;
using System.Collections.Generic;

static class zrbj_DAITools_CRCTest
{
   static UInt32[] byte_lookup_uint32_lsb_first (UInt32 polynomial)
   {
      UInt32[] lookup = new UInt32[256];

      for (UInt32 b = 0; b < 256; ++b)
      {
         UInt32 x = b;

         for (int i = 0; i < 8; ++i)
         {
            x = (x >> 1) ^ ((x & 1) == 0 ? 0 : polynomial);
         }

         lookup[b] = x;
      }

      return lookup;
   }

   static UInt32[] byte_lookup_uint32_msb_first (UInt32 polynomial)
   {
      UInt32[] lookup = new UInt32[256];

      for (UInt32 b = 0; b < 256; ++b)
      {
         UInt32 x = b << 24;

         for (int i = 0; i < 8; ++i)
         {
            x = (x << 1) ^ ((Int32)x < 0 ? polynomial : 0);
         }

         lookup[b] = x;
      }

      return lookup;
   }

   static UInt32 crc32_lsb_first (IEnumerable<byte> data, UInt32 crc, UInt32[] lookup)
   {
      foreach (byte b in data)
      {
         crc = (crc >> 8) ^ lookup[(byte)crc ^ b];
      }
         
      return crc;
   }

   static UInt32 crc32_msb_first (IEnumerable<byte> data, UInt32 crc, UInt32[] lookup)
   {
      foreach (byte b in data)
      {
         crc = (crc << 8) ^ lookup[(crc >> 24) ^ b];
      }
         
      return crc;
   }

   const UInt32 CRC32POLY_ZIP = 0xEDB88320u;
   const UInt32 CRC32POLY_REV = 0x04C11DB7u;

   public static UInt32[] lookup_crc32_zip = byte_lookup_uint32_lsb_first(CRC32POLY_ZIP);
   public static UInt32[] lookup_crc32_rev = byte_lookup_uint32_msb_first(CRC32POLY_REV);

   public static UInt32 crc32_zip (IEnumerable<byte> data, UInt32 crc = 0)       
   {
      return ~crc32_lsb_first(data, ~crc, lookup_crc32_zip);
   }

   public static UInt32 crc32_rev (IEnumerable<byte> data, UInt32 crc = 0)
   {
      return crc32_msb_first(data, crc, lookup_crc32_rev);
   }

   public static UInt32 crc32_rev (IEnumerable<byte> data, UInt32 crc, bool post_condition)
   {
      crc = crc32_rev(data, crc);
         
      return post_condition ? ~crc : crc;
   }

   /////////////////////////////////////////////////////////////////////////////////////////////

   public static byte[] raw_bytes (string s)
   {
      return System.Text.Encoding.UTF8.GetBytes(s);
   }

   public static byte[] raw_bytes (UInt32[] a)
   {
      byte[] bytes = new byte[a.Length * 4];
      uint k = 0;

      foreach (UInt32 w in a)
      {
         bytes[k + 0] = (byte)(w >>  0);
         bytes[k + 1] = (byte)(w >>  8);
         bytes[k + 2] = (byte)(w >> 16);
         bytes[k + 3] = (byte)(w >> 24);
         k += 4;
      }

      return bytes;
   }

   /////////////////////////////////////////////////////////////////////////////////////////////

   public delegate UInt32 crc_func (IEnumerable<byte> data, UInt32 crc);

   public static UInt32 crc_all_subranges (crc_func f, byte[] data, UInt32 crc = 0)
   {
      for (int i = 0; i <= data.Length; ++i)
      {
         int n = data.Length - i;

         for (int j = 0; j <= n; ++j)
         {
            crc = f(new ArraySegment<byte>(data, i, j), crc);
         }
      }

      return crc;
   }

   ////////////////////////////////////////////////////////////////////////////////////////////////
   
   public static bool print_and_check (string description, UInt32 chk, UInt32 tst)
   {
      System.Console.WriteLine(description + " {0:x8}", tst);

      if (tst != chk)
      {
         System.Console.WriteLine("wrong result, expected {0:x8}", chk);

         return false;
      }

      return true;
   }

   public static void crc_test ()
   {
      byte[] data = raw_bytes(lookup_crc32_zip);
         
      print_and_check("crc32_zip(lookup_crc32_zip, 00000000)", 
         0x926F9BAB, crc_all_subranges(crc32_zip, data)  );

      print_and_check("crc32_zip(lookup_crc32_zip, 811C9DC5)", 
         0x2BFADF82, crc_all_subranges(crc32_zip, data, 0x811C9DC5)  );

      data = raw_bytes(lookup_crc32_rev);

      print_and_check("crc32_rev(lookup_crc32_rev, 00000000)", 
         0x5CDEF5F6, crc_all_subranges(crc32_rev, data)  );

      print_and_check("crc32_rev(lookup_crc32_rev, 811C9DC5)", 
         0x7F5EAD74, crc_all_subranges(crc32_rev, data, 0x811C9DC5)  );
   }

   static void Main(string[] args)
   {
      Console.WriteLine("crc32_zip {0:x8}", crc32_zip(raw_bytes("ANT15")));
      Console.WriteLine("crc32_rev {0:x8}", crc32_rev(raw_bytes("ANT15")));
      Console.WriteLine("crc32_zip {0:x8}", crc32_zip(raw_bytes(lookup_crc32_zip)));
      Console.WriteLine("crc32_rev {0:x8}", crc32_rev(raw_bytes(lookup_crc32_rev)));
         
      crc_test();
   }
}
