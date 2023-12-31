﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQMCH
{
	internal class SaveData
	{
		private static SaveData mThis = new SaveData();
		private String mFileName = "";
		private Byte[]? mBuffer = null;
		private readonly System.Text.Encoding mEncode = System.Text.Encoding.ASCII;
		public uint Adventure { private get; set; } = 0;

		private SaveData()
		{ }

		public static SaveData Instance()
		{
			return mThis;
		}

		public bool Open(String filename, bool force)
		{
			if (System.IO.File.Exists(filename) == false) return false;

			mFileName = filename;
			mBuffer = System.IO.File.ReadAllBytes(filename);

            if (force)
            {
				Backup();
				return true;
			}

            // support retrofreak
            uint[] baseAddress = { 0, 0x20000 };
			foreach (var address in baseAddress)
			{
				Adventure = address;

				// check header
				// big endianness
				// DQM30119
				if (System.Text.Encoding.ASCII.GetString(ReadValue(0, 8)) != "91103MQD") continue;

				// check sum
				var sum = CalcCheckSum();
				if ((sum & 0xFF) != ReadNumber(0x1EB8, 1)) continue;
				if (((sum >> 8) & 0xFF) != ReadNumber(0x1EC7, 1)) continue;

				// all ok
				Backup();
				return true;
			}

			mBuffer = null;
			mFileName = "";
			return false;
		}

		public bool Save()
		{
			if (mFileName == null || mBuffer == null) return false;

			var sum = CalcCheckSum();
			WriteNumber(0x1EB8, 1, (byte)(sum & 0xFF));
			WriteNumber(0x1EC7, 1, (byte)((sum >> 8) & 0xFF));
			System.IO.File.WriteAllBytes(mFileName, mBuffer);
			return true;
		}

		public bool SaveAs(String filename)
		{
			if (mFileName == null || mBuffer == null) return false;
			mFileName = filename;
			return Save();
		}

		public uint ReadNumber(uint address, uint size)
		{
			if (mBuffer == null) return 0;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return 0;
			if (address - size < 0) return 0;
			uint result = 0;
			for (int i = 0; i < size; i++)
			{
				result += (uint)mBuffer[address - i] << (i * 8);
			}
			return result;
		}

		public Byte[] ReadValue(uint address, uint size)
		{
			Byte[] result = new Byte[size];
			if (mBuffer == null) return result;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return result;
			Array.Copy(mBuffer, address, result, 0, size);
			return result;
		}

		// 0 to 7.
		public bool ReadBit(uint address, uint bit)
		{
			if (bit > 7) return false;
			if (mBuffer == null) return false;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return false;
			Byte mask = (Byte)(1 << (int)bit);
			return (mBuffer[address] & mask) != 0;
		}

		public void WriteNumber(uint address, uint size, uint value)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return;
			if (address - size < 0) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[address - i] = (Byte)(value & 0xFF);
				value >>= 8;
			}
		}

		// 0 to 7.
		public void WriteBit(uint address, uint bit, bool value)
		{
			if (bit > 7) return;
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return;
			Byte mask = (Byte)(1 << (int)bit);
			if (value) mBuffer[address] = (Byte)(mBuffer[address] | mask);
			else mBuffer[address] = (Byte)(mBuffer[address] & ~mask);
		}

		public void WriteValue(uint address, Byte[] buffer)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + buffer.Length >= mBuffer.Length) return;
			Array.Copy(buffer, 0, mBuffer, address, buffer.Length);
		}

		public void Fill(uint address, uint size, Byte number)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[address + i] = number;
			}
		}

		public void Copy(uint from, uint to, uint size)
		{
			if (mBuffer == null) return;
			from = CalcAddress(from);
			to = CalcAddress(to);
			if (from + size >= mBuffer.Length) return;
			if (to + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[to + i] = mBuffer[from + i];
			}
		}

		public void Swap(uint from, uint to, uint size)
		{
			if (mBuffer == null) return;
			from = CalcAddress(from);
			to = CalcAddress(to);
			if (from + size >= mBuffer.Length) return;
			if (to + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				Byte tmp = mBuffer[to + i];
				mBuffer[to + i] = mBuffer[from + i];
				mBuffer[from + i] = tmp;
			}
		}

		public List<uint> FindAddress(String name, uint index)
		{
			List<uint> result = new List<uint>();
			if (mBuffer == null) return result;

			for (; index < mBuffer.Length; index++)
			{
				if (mBuffer[index] != name[0]) continue;

				int len = 1;
				for (; len < name.Length; len++)
				{
					if (mBuffer[index + len] != name[len]) break;
				}
				if (len >= name.Length) result.Add(index);
				index += (uint)len;
			}
			return result;
		}

		private uint CalcCheckSum()
		{
			if(mBuffer == null) return 0;

			uint sum = 0;
			for (uint index = 0; index < 0x1EC0; index++)
			{
				sum += ReadNumber(index, 1);
			}
			sum -= ReadNumber(0x1EB8, 1);
			return sum;
		}

		private uint CalcAddress(uint address)
		{
			return address + Adventure;
		}

		private void Backup()
		{
			DateTime now = DateTime.Now;
			String path = System.IO.Directory.GetCurrentDirectory();
			path = System.IO.Path.Combine(path, "backup");
			if (!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
			}
			path = System.IO.Path.Combine(path, $"{now:yyyy-MM-dd HH-mm-ss} {System.IO.Path.GetFileName(mFileName)}");
			System.IO.File.Copy(mFileName, path, true);
		}
	}
}
