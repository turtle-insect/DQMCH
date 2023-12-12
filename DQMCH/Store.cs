using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQMCH
{
	internal class Store
	{
		private const uint BaseAddress = 0xC0;
		public uint ID {  get; }

		public Store(uint id) => ID = id;

		public uint Count
		{
			get => SaveData.Instance().ReadNumber(BaseAddress + ID, 1);
			set => Util.WriteNumber(BaseAddress + ID, 1, 0, 99, value);
		}
	}
}
