using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQMCH
{
	internal class Basic
	{
		public uint Money
		{
			get => SaveData.Instance().ReadNumber(0x13, 3);
			set => Util.WriteNumber(0x13, 3, 0, 999999, value);
		}

		public uint Bank
		{
			get => SaveData.Instance().ReadNumber(0xC7, 4);
			set => Util.WriteNumber(0xC7, 4, 0, 9999900, value);
		}
	}
}
