using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace StatsAndAchievements.CareerManagerScreens;

public class LinesScrollerScreen((TextMeshPro lhs, TextMeshPro rhs)[] range, Color regularColor, Color highlightColor)
{
	private const string Leader = "\u2193";

	public int SelectedIndex
	{
		get => RangeSelectedIndex + ScrollOffset;
		set
		{
			var prevCanEnter = SelectedIndex < Options.Length && SelectedIndex >= 0 ? Options[SelectedIndex].canEnter : null;
			var prevRangeSelection = RangeSelectedIndex;
			RangeSelectedIndex += value - SelectedIndex;

			var actualRangeLength = Range.Length - 1;

			var maxRelative = Math.Min(Options.Length, actualRangeLength) - 1;
			var minRelative = Math.Min(maxRelative, 0);
			if (RangeSelectedIndex > maxRelative)
			{
				ScrollOffset = Math.Min( // MIN(1, 1)
					ScrollOffset + RangeSelectedIndex - maxRelative, // 0 + 9 - 8 = 1 
					Math.Max(Options.Length - actualRangeLength, 0) // MAX(10 - 9, 0) = MAX(1, 0) = 1
				);
				RangeSelectedIndex = maxRelative;
				SyncOptionsToTMPros();
			}
			if (RangeSelectedIndex < minRelative)
			{
				ScrollOffset = Math.Max(ScrollOffset - minRelative + RangeSelectedIndex, 0);
				RangeSelectedIndex = minRelative;
				SyncOptionsToTMPros();
			}

			if (prevRangeSelection > -1)
			{
				Range[prevRangeSelection].lhs.color = prevCanEnter?.Invoke() ?? true ? TextColors.deselect : TextColors.deselect * 0.5f;
				if (ColorRHS)
					Range[prevRangeSelection].rhs.color = prevCanEnter?.Invoke() ?? true ? TextColors.deselect : TextColors.deselect * 0.5f;
			}

			if (RangeSelectedIndex > -1)
			{
				var curCanEnter = Options[SelectedIndex].canEnter;
				Range[RangeSelectedIndex].lhs.color = curCanEnter?.Invoke() ?? true ? TextColors.select : TextColors.select * 0.5f;
				if (ColorRHS)
					Range[RangeSelectedIndex].rhs.color = curCanEnter?.Invoke() ?? true ? TextColors.select : TextColors.select * 0.5f;
			}
		}
	}
	public (OptionParser? lhs, OptionParser? rhs, CanEnter? canEnter)[] Options
	{
		get => options ?? [];
		set
		{
			options = value;
			SyncOptionsToTMPros();
			SelectedIndex = value.Length > 0 ? 0 : -1;
		}
	}

	public TextMeshPro ArrowLeader => Range.Last().lhs;

	private (TextMeshPro lhs, TextMeshPro rhs)[] Range { get; } = range;
	private (Color deselect, Color select) TextColors { get; } = (regularColor, highlightColor);
	private int RangeSelectedIndex { get; set; } = -1;
	private int ScrollOffset { get; set; } = 0;
	private bool ColorRHS { get; set; } = true;

	private (OptionParser? lhs, OptionParser? rhs, CanEnter? canEnter)[]? options;

	public delegate void OptionParser(TextMeshPro option);
	public delegate bool CanEnter();

	public void SetOptions((OptionParser?, OptionParser?, CanEnter? canEnter)[]? ops = null) => Options = ops ?? [];
	public void SetOptions(IEnumerable<(OptionParser?, OptionParser?, CanEnter? canEnter)> ops) => Options = ops.ToArray();
	public void Up() => SelectedIndex--;
	public void Down() => SelectedIndex++;

	private void SyncOptionsToTMPros()
	{
		ArrowLeader.text = Options.Length > Range.Length - 1 && ScrollOffset != Options.Length - (Range.Length - 1) ? Leader : string.Empty; // 10 - 11 - 2
		for (var idx = 0; idx < Math.Min(Range.Length - 1, Options.Length); idx++)
		{
			var curCanEnter = Options[idx + ScrollOffset].canEnter?.Invoke() ?? true;
			var color = idx == RangeSelectedIndex ? TextColors.select : TextColors.deselect;
			Range[idx].lhs.color = curCanEnter ? color : color * 0.5f;
			if (ColorRHS)
				Range[idx].rhs.color = curCanEnter ? color : color * 0.5f;
			Options[idx + ScrollOffset].lhs?.Invoke(Range[idx].lhs);
			Options[idx + ScrollOffset].rhs?.Invoke(Range[idx].rhs);
		}
	}
}
