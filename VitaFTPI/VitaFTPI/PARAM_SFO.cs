/*
 * This file is made by jllopisol@gmail.com all credits and I mean ALL credits of this file go to him
 * You can donate to him via paypal:
 * https://www.paypal.com/donate/?cmd=_s-xclick&hosted_button_id=RMFDRTBU49E8E
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PeXploit
{
    public class PARAM_SFO
    {
        public PARAM_SFO(string filepath) => this.Init((Stream)new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read));

        public PARAM_SFO(byte[] inputdata) => this.Init((Stream)new MemoryStream(inputdata));

        public PARAM_SFO(Stream input) => this.Init(input);

        public PARAM_SFO.Table[] Tables { get; private set; }

        public PARAM_SFO.DataTypes DataType
        {
            get
            {
                if (this.Tables == null)
                    return PARAM_SFO.DataTypes.None;
                foreach (PARAM_SFO.Table table in this.Tables)
                {
                    if (table.Name == "CATEGORY")
                        return (PARAM_SFO.DataTypes)BitConverter.ToUInt16(Encoding.UTF8.GetBytes(table.Value), 0);
                }
                return PARAM_SFO.DataTypes.None;
            }
        }

        public string Detail
        {
            get
            {
                if (this.Tables == null)
                    return "";
                foreach (PARAM_SFO.Table table in this.Tables)
                {
                    if (table.Name == "DETAIL")
                        return table.Value;
                }
                return "";
            }
        }

        public string ContentID
        {
            get
            {
                if (this.Tables == null)
                    return "";
                foreach (PARAM_SFO.Table table in this.Tables)
                {
                    if (table.Name == "CONTENT_ID")
                        return table.Value;
                }
                return "";
            }
        }

        public string TITLEID
        {
            get
            {
                if (this.Tables == null)
                    return "";
                foreach (PARAM_SFO.Table table in this.Tables)
                {
                    if (table.Name == "TITLE_ID")
                        return table.Value;
                }
                return "";
            }
        }

        public string TitleID
        {
            get
            {
                string titleid = this.TITLEID;
                if (titleid == "")
                    return "";
                return titleid.Split('-')[0];
            }
        }

        public string Title
        {
            get
            {
                if (this.Tables == null)
                    return "";
                foreach (PARAM_SFO.Table table in this.Tables)
                {
                    if (table.Name == "TITLE")
                        return table.Value;
                }
                return "";
            }
        }

        private string ReadValue(BinaryReader br, PARAM_SFO.index_table table)
        {
            br.BaseStream.Position = (long)(PARAM_SFO.Header.DataTableStart + table.param_data_offset);
            switch (table.param_data_fmt)
            {
                case PARAM_SFO.FMT.UTF_8:
                    return Encoding.UTF8.GetString(br.ReadBytes((int)table.param_data_max_len)).Replace("\0", "");
                case PARAM_SFO.FMT.ASCII:
                    return Encoding.ASCII.GetString(br.ReadBytes((int)table.param_data_max_len)).Replace("\0", "");
                case PARAM_SFO.FMT.UINT32:
                    return br.ReadUInt32().ToString();
                default:
                    return (string)null;
            }
        }

        private string ReadName(BinaryReader br, PARAM_SFO.index_table table)
        {
            br.BaseStream.Position = (long)(PARAM_SFO.Header.KeyTableStart + (uint)table.param_key_offset);
            string str = "";
            while ((byte)br.PeekChar() != (byte)0)
                str += br.ReadChar().ToString();
            ++br.BaseStream.Position;
            return str;
        }

        private void Init(Stream input)
        {
            using (BinaryReader binaryReader = new BinaryReader(input))
            {
                PARAM_SFO.Header.Read(binaryReader);
                if (!Functions.CompareBytes(PARAM_SFO.Header.Magic, new byte[4]
                {
          (byte) 0,
          (byte) 80,
          (byte) 83,
          (byte) 70
                }))
                    throw new Exception("Invalid PARAM.SFO Header Magic");
                List<PARAM_SFO.index_table> indexTableList = new List<PARAM_SFO.index_table>();
                for (int index = 0; (long)index < (long)PARAM_SFO.Header.IndexTableEntries; ++index)
                {
                    PARAM_SFO.index_table indexTable = new PARAM_SFO.index_table();
                    indexTable.Read(binaryReader);
                    indexTableList.Add(indexTable);
                }
                List<PARAM_SFO.Table> tableList = new List<PARAM_SFO.Table>();
                int num = 0;
                foreach (PARAM_SFO.index_table table1 in indexTableList)
                {
                    PARAM_SFO.Table table2 = new PARAM_SFO.Table();
                    table2.index = num;
                    table2.Indextable = table1;
                    table2.Name = this.ReadName(binaryReader, table1);
                    table2.Value = this.ReadValue(binaryReader, table1);
                    ++num;
                    tableList.Add(table2);
                }
                this.Tables = tableList.ToArray();
                binaryReader.Close();
            }
        }

        public enum DataTypes : uint
        {
            AppleTV = 4154, // 0x0000103A
            WebTV = 5754, // 0x0000167A
            AppMusic = 16717, // 0x0000414D
            AppPhoto = 16720, // 0x00004150
            AutoInstallRoot = 16722, // 0x00004152
            AppVideo = 16726, // 0x00004156
            BroadCastVideo = 16982, // 0x00004256
            CellBE = 17218, // 0x00004342
            DiscGame = 17479, // 0x00004447
            DiscMovie = 17485, // 0x0000444D
            None = 17486, // 0x0000444E
            DiscPackage = 17488, // 0x00004450
            GameData = 18244, // 0x00004744
            PSNGame = 18248, // 0x00004748
            HDDGame = 18503, // 0x00004847
            Home = 18509, // 0x0000484D
            SaveData = 21316, // 0x00005344
            StoreFronted = 21318, // 0x00005346
            ThemeRoot = 21586, // 0x00005452
            VideoRoot = 22098, // 0x00005652
            ExtraRoot = 22610, // 0x00005852
        }

        public enum FMT : ushort
        {
            UTF_8 = 1024, // 0x0400
            ASCII = 1026, // 0x0402
            UINT32 = 1028, // 0x0404
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct Header
        {
            public static byte[] Magic = new byte[4]
            {
        (byte) 0,
        (byte) 80,
        (byte) 83,
        (byte) 70
            };
            public static byte[] version = new byte[4]
            {
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 0
            };
            public static uint KeyTableStart = 0;
            public static uint DataTableStart = 0;
            public static uint IndexTableEntries = 0;

            private static byte[] Buffer
            {
                get
                {
                    byte[] numArray = new byte[20];
                    Array.Copy((Array)PARAM_SFO.Header.Magic, 0, (Array)numArray, 0, 4);
                    Array.Copy((Array)PARAM_SFO.Header.version, 0, (Array)numArray, 4, 4);
                    Array.Copy((Array)BitConverter.GetBytes(PARAM_SFO.Header.KeyTableStart), 0, (Array)numArray, 8, 4);
                    Array.Copy((Array)BitConverter.GetBytes(PARAM_SFO.Header.DataTableStart), 0, (Array)numArray, 12, 4);
                    Array.Copy((Array)BitConverter.GetBytes(PARAM_SFO.Header.IndexTableEntries), 0, (Array)numArray, 16, 4);
                    return numArray;
                }
            }

            public static void Read(BinaryReader input)
            {
                input.BaseStream.Seek(0L, SeekOrigin.Begin);
                input.Read(PARAM_SFO.Header.Magic, 0, 4);
                input.Read(PARAM_SFO.Header.version, 0, 4);
                PARAM_SFO.Header.KeyTableStart = input.ReadUInt32();
                PARAM_SFO.Header.DataTableStart = input.ReadUInt32();
                PARAM_SFO.Header.IndexTableEntries = input.ReadUInt32();
            }
        }

        public struct Table
        {
            public PARAM_SFO.index_table Indextable;
            public string Name;
            public string Value;
            public int index;

            private byte[] NameBuffer
            {
                get
                {
                    byte[] numArray = new byte[this.Name.Length + 1];
                    Array.Copy((Array)Encoding.UTF8.GetBytes(this.Name), 0, (Array)numArray, 0, this.Name.Length);
                    return numArray;
                }
            }

            private byte[] ValueBuffer
            {
                get
                {
                    switch (this.Indextable.param_data_fmt)
                    {
                        case PARAM_SFO.FMT.UTF_8:
                            byte[] numArray1 = new byte[(int)this.Indextable.param_data_max_len];
                            Array.Copy((Array)Encoding.UTF8.GetBytes(this.Value), 0, (Array)numArray1, 0, this.Value.Length);
                            return numArray1;
                        case PARAM_SFO.FMT.ASCII:
                            byte[] numArray2 = new byte[(int)this.Indextable.param_data_max_len];
                            Array.Copy((Array)Encoding.ASCII.GetBytes(this.Value), 0, (Array)numArray2, 0, this.Value.Length);
                            return numArray2;
                        case PARAM_SFO.FMT.UINT32:
                            return BitConverter.GetBytes(uint.Parse(this.Value));
                        default:
                            return (byte[])null;
                    }
                }
            }
        }

        public struct index_table
        {
            public PARAM_SFO.FMT param_data_fmt;
            public uint param_data_len;
            public uint param_data_max_len;
            public uint param_data_offset;
            public ushort param_key_offset;

            private byte[] Buffer
            {
                get
                {
                    byte[] numArray = new byte[16];
                    Array.Copy((Array)BitConverter.GetBytes(this.param_key_offset), 0, (Array)numArray, 0, 2);
                    Array.Copy((Array)BitConverter.GetBytes(((ushort)this.param_data_fmt).SwapByteOrder()), 0, (Array)numArray, 2, 2);
                    Array.Copy((Array)BitConverter.GetBytes(this.param_data_len), 0, (Array)numArray, 4, 4);
                    Array.Copy((Array)BitConverter.GetBytes(this.param_data_max_len), 0, (Array)numArray, 8, 4);
                    Array.Copy((Array)BitConverter.GetBytes(this.param_data_offset), 0, (Array)numArray, 12, 4);
                    return numArray;
                }
            }

            public void Read(BinaryReader input)
            {
                this.param_key_offset = input.ReadUInt16();
                this.param_data_fmt = (PARAM_SFO.FMT)input.ReadUInt16().SwapByteOrder();
                this.param_data_len = input.ReadUInt32();
                this.param_data_max_len = input.ReadUInt32();
                this.param_data_offset = input.ReadUInt32();
            }
        }
    }
}
