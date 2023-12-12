using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQMCH
{
	internal class Character
	{
		private readonly uint mAddress;

		public Character(uint address) => mAddress = address;

		public uint Job
		{
			get => SaveData.Instance().ReadNumber(mAddress + 2, 1) - 1;
			set => SaveData.Instance().WriteNumber(mAddress + 2, 1, value + 1);
		}

		public uint Weight
		{
			get => SaveData.Instance().ReadNumber(mAddress, 1);
			set => Util.WriteNumber(mAddress, 1, 0, 255, value);
		}

		public uint Sex
		{
			get => SaveData.Instance().ReadNumber(mAddress + 1, 1) & 0x01;
			set
			{
				var current = SaveData.Instance().ReadNumber(mAddress + 1, 1) & 0xFE;
				current |= value;
				SaveData.Instance().WriteNumber(mAddress + 1, 1, current);
			}
		}

		public uint Rank
		{
			get => (SaveData.Instance().ReadNumber(mAddress + 1, 1) & 0x06) / 2;
			set
			{
				var current = SaveData.Instance().ReadNumber(mAddress + 1, 1) & 0xF9;
				current |= value * 2;
				SaveData.Instance().WriteNumber(mAddress + 1, 1, current);
			}
		}

		public uint Adult
		{
			get => (SaveData.Instance().ReadNumber(mAddress + 1, 1) & 0x10) >> 4;
			set
			{
				var current = SaveData.Instance().ReadNumber(mAddress + 1, 1) & 0xE7;
				current |= 0x08U << (value == 0 ? 0 : 1);
				SaveData.Instance().WriteNumber(mAddress + 1, 1, current);
			}
		}
	}
}
