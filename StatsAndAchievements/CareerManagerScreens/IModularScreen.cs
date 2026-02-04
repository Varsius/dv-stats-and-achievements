using DV.ServicePenalty.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsAndAchievements.CareerManagerScreens
{
	public interface IModularScreen
	{
		public IModularScreen? Parent { get; }
		public ModularScreenHost? Host { get; }
		public ShowScreen? Show { get; }
		public HideScreen? Hide { get; }
		public ScreenInput? Input { get; }

		public delegate void ShowScreen(IModularScreen? previous);
		public delegate void HideScreen(IModularScreen? next = null);
		public delegate void ScreenInput(InputAction action);
	}
}
