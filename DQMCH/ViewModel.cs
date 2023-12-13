using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace DQMCH
{
	internal class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public Info Info { get; private set; } = Info.Instance();
		public CommandAction FileOpenCommandAction { get; private set; }
		public CommandAction FileForceOpenCommandAction { get; private set; }
		public CommandAction FileSaveCommandAction { get; private set; }
		public CommandAction ChoiceItemCommandAction { get; private set; }
		public CommandAction AppendCharacterCommandAction { get; private set; }

		public Basic Basic { get; private set; } = new Basic();
		public ObservableCollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
		public ObservableCollection<Store> Stores { get; private set; } = new ObservableCollection<Store>();
		public ObservableCollection<Character> Caravan { get; private set; } = new ObservableCollection<Character>();

		public ViewModel()
		{
			FileOpenCommandAction = new CommandAction(FileOpen);
			FileForceOpenCommandAction = new CommandAction(FileForceOpen);
			FileSaveCommandAction = new CommandAction(FileSave);
			ChoiceItemCommandAction = new CommandAction(ChoiceItem);
			AppendCharacterCommandAction = new CommandAction(AppendCharacter);
		}

		private void Init()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Basic)));
			
			Items.Clear();
			for (uint i = 0; i < 3; i++)
			{
				for (uint j = 0; j < 8; j++)
				{
					var address = 0x18 + i * 8 + 7 - j;
					Items.Add(new Item(address));
				}
			}

			Stores.Clear();
			foreach (var item in Info.Instance().Store)
			{
				Stores.Add(new Store(item.Value));
			}

			Caravan.Clear();
			uint count = SaveData.Instance().ReadNumber(0x788, 1);
			for (uint i = 0; i < count; i++)
			{
				AppendCharacter(null);
			}
		}

		private void FileOpen(object? obj)
		{
			FileOpen(false);
		}

		private void FileForceOpen(object? obj)
		{
			FileOpen(true);
		}

		private void FileOpen(bool force)
		{
			var dlg = new OpenFileDialog();
			if (dlg.ShowDialog() == false) return;

			var result = SaveData.Instance().Open(dlg.FileName, force);
			MessageBox.Show(result ? "OK" : "NG");
			if (result == false) return;

			Init();
		}

		private void FileSave(object? obj)
		{
			SaveData.Instance().WriteNumber(0x788, 1, (uint)Caravan.Count);
			SaveData.Instance().Save();
		}

		private void ChoiceItem(object? obj)
		{
			Item item = obj as Item;
			if (item == null) return;

			var dlg = new ItemChoiceWindow();
			dlg.ID = item.ID;
			dlg.ShowDialog();
			item.ID = dlg.ID;
		}

		private void AppendCharacter(object? obj)
		{
			var index = (uint)Caravan.Count;
			if (index >= 20) return;

			var address = 0x7A0 + (index / 2) * 56 + (index % 2) * 36;
			Caravan.Add(new Character(address));
		}
	}
}
