using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DQMCH
{
	internal class Item : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;

		public Item(uint address) => mAddress = address;

		public uint ID
		{
			get => SaveData.Instance().ReadNumber(mAddress, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
			}
		}
	}
}
