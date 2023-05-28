using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
	public class ScrollButton : MonoBehaviour
	{
		public TouchBtn btn;
		public Scrollbar bar;
		public int direction = 1;

		void Start()
		{
			if (btn == null) { btn = this.GetComponent<TouchBtn>(); }
			if (bar == null) { bar = Helpers.Find<Scrollbar>(this.name.Replace("SlidingArrowDown", "Scrollbar")); }
			if (bar == null) { bar = Helpers.Find<Scrollbar>(this.name.Replace("SlidingArrowUp", "Scrollbar")); }
			btn.AddOnClickListener(ButtonClick, false);
		}
		void ButtonClick()
		{
			bar.value += bar.size * direction;
		}
	}

}