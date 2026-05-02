using DV.ServicePenalty.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace StatsAndAchievements.CareerManagerScreens;

public abstract class ModularScreenHost(IModularScreen? parent = null) : MonoBehaviour, IDisplayScreen, IModularScreen
{
	public IModularScreen? Parent { get; } = parent;
	public ModularScreenHost? Host { get; set; }
	public IModularScreen? Active { get; set; }

	public IModularScreen.ShowScreen? Show { get; protected set; }
	public IModularScreen.HideScreen? Hide { get; protected set; }
	public IModularScreen.ScreenInput? Input { get; protected set; }
	public event Action? Clear;

	public CareerManagerMainScreen MainScreen { get; private set; } = null!;
	public (TextMeshPro lhs, TextMeshPro rhs)[] Lines { get; private set; } = null!;
	public (TextMeshPro ParagraphA, TextMeshPro ParagraphB) Paragraphs { get; private set; }
	public LinesScrollerScreen? Scroller { get; private set; }

	private (string lhsState, string rhsState)[] LineStates { get; set; } = null!;
	private (string stateA, string stateB) ParagraphStates { get; set; }

	private void Start()
	{
		MainScreen = transform.parent.gameObject.GetComponentInChildren<CareerManagerMainScreen>();
		SetupTMPros();
		Scroller = new LinesScrollerScreen(Lines.Skip(2).ToArray(), RegularTextColor, HighlightedTextColor);
	}

	public Color RegularTextColor => MainScreen.screenSwitcher.REGULAR_COLOR;
	public Color HighlightedTextColor => MainScreen.screenSwitcher.HIGHLIGHTED_COLOR;

	public TextMeshPro? Title => Lines.Length > 0 ? Lines[0].lhs : null;
	public TextMeshPro? AltTitle => Lines.Length > 0 ? Lines[0].rhs : null;
	public TextMeshPro? Subtitle => Lines.Length > 1 ? Lines[1].lhs : null;
	public TextMeshPro? AltSubtitle => Lines.Length > 1 ? Lines[1].rhs : null;

	public void OnClear()
	{
		RestoreTMProStates();
	}
	public void OnHide(IModularScreen? next)
	{
	}


	public void Activate(IDisplayScreen previousScreen)
	{
		Show?.Invoke(Active);
		Active = this;
	}
	public void Disable()
	{
		Active?.Hide?.Invoke();

		Clear?.Invoke();
	}
	public void HandleInputAction(InputAction input)
	{
		Active?.Input?.Invoke(input);
	}

	public void SwitchToScreen(IModularScreen screen)
	{
		Active?.Hide?.Invoke(screen);

		Clear?.Invoke();

		screen.Show?.Invoke(Active);
		Active = screen;
	}

	public void Exit()
	{
		MainScreen.screenSwitcher.SetActiveDisplay(MainScreen);
	}
	private void SetupTMPros()
	{
		List<(int, TextMeshPro)> TMProLHSs = [];
		List<(int, TextMeshPro)> TMProRHSs = [];

		TextMeshPro? lowerParagraph = null;
		TextMeshPro? upperParagraph = null;
		var lastParagraphIdx = 69;
		foreach (var comp in MainScreen.title.transform.parent.GetComponentsInChildren<TextMeshPro>())
			if (comp.gameObject.name.StartsWith("line") && int.TryParse(comp.gameObject.name.Substring("line".Length), out var idx))
				TMProLHSs.Add((idx - 1, comp));
			else if (comp.gameObject.name.StartsWith("value") && int.TryParse(comp.gameObject.name.Substring("value".Length), out idx))
				TMProRHSs.Add((idx - 1, comp));
			else if (comp.gameObject.name.StartsWith("paragraph-line") && int.TryParse(comp.gameObject.name.Substring("paragraph-line".Length), out idx))
			{
				var oldLower = lowerParagraph;
				lowerParagraph = idx > lastParagraphIdx ? lowerParagraph : comp;
				upperParagraph = idx > lastParagraphIdx ? comp : oldLower;
				lastParagraphIdx = idx;
			}

		TMProLHSs = TMProLHSs.OrderBy((tuple => tuple.Item1)).ToList();
		TMProRHSs = TMProRHSs.OrderBy((tuple => tuple.Item1)).ToList();

		var lineCount = Math.Min(TMProLHSs.Count, TMProRHSs.Count);
		Main.Log($"Initialized {lineCount} line(s) & " +
						   $"{(lowerParagraph != null && lowerParagraph ? 1 : 0) + (upperParagraph != null && upperParagraph ? 1 : 0)} paragraph(s).");

		Lines = new (TextMeshPro, TextMeshPro)[lineCount];

		for (var idx = 0; idx < lineCount; idx++)
		{
			Lines[idx] = (TMProLHSs[idx].Item2, TMProRHSs[idx].Item2);
			Lines[idx].lhs.text = string.Empty;
			Lines[idx].rhs.text = string.Empty;
		}

		if (lowerParagraph is null || upperParagraph is null)
		{
			Main.Log("Failed to locate both paragraph elements during LeaseScreen setup!");
			Exit();
			return;
		}

		Paragraphs = (lowerParagraph, upperParagraph);
		Paragraphs.ParagraphA.text = string.Empty;
		Paragraphs.ParagraphB.text = string.Empty;

		SaveTMProStates();
	}
	private void SaveTMProStates()
	{
		LineStates = new (string, string)[Lines.Length];
		for (var idx = 0; idx < Lines.Length; idx++)
			LineStates[idx] = (JsonUtility.ToJson(Lines[idx].lhs), JsonUtility.ToJson(Lines[idx].rhs));

		ParagraphStates = (JsonUtility.ToJson(Paragraphs.ParagraphA), JsonUtility.ToJson(Paragraphs.ParagraphB));
	}
	private void RestoreTMProStates()
	{
		for (var idx = 0; idx < LineStates.Length; idx++)
		{
			JsonUtility.FromJsonOverwrite(LineStates[idx].lhsState, Lines[idx].lhs);
			JsonUtility.FromJsonOverwrite(LineStates[idx].rhsState, Lines[idx].rhs);
			Lines[idx].lhs.SetAllDirty();
			Lines[idx].rhs.SetAllDirty();
		}
		JsonUtility.FromJsonOverwrite(ParagraphStates.stateA, Paragraphs.ParagraphA);
		JsonUtility.FromJsonOverwrite(ParagraphStates.stateB, Paragraphs.ParagraphB);
		Paragraphs.ParagraphA.SetAllDirty();
		Paragraphs.ParagraphB.SetAllDirty();
	}
}
