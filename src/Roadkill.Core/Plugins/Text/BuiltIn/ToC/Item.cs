using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Plugins.Text.BuiltIn.ToC
{
	/// <summary>
	/// Represents a H tags and its child headers.
	/// </summary>
	public class Item
	{
		private List<Item> _children;
		
		public string Id { get; private set; }
		public string Title { get; set; }
		public Item Parent { get; private set; }
		public int Level { get; set; }

		public IEnumerable<Item> Children
		{
			get { return _children; }
		}

		public Item(string title, string id)
		{
			Title = title;
			Id = id;
			Level = Tree.DEFAULT_LEVEL_ZERO_BASED;
			_children = new List<Item>();
		}

		public void AddChild(Item item)
		{
			item.Parent = this;
			item.Level = Level + 1;
			_children.Add(item);
		}

		public int GetPositionAmongSiblings()
		{
			if (Parent != null)
			{
				int i = Parent.Children.ToList().IndexOf(this);
				return i + 1;
			}
			else
			{
				return 1;
			}
		}

		/// <summary>
		/// Gets the item's child at a particular index - zero based.
		/// </summary>
		public Item GetChild(int index)
		{
			return _children[index];
		}

		public bool HasChildren()
		{
			return _children.Count > 0;
		}
	}
}
