using DV.JObjectExtstensions;

namespace StatsAndAchievements;

public sealed class SavedBool
{
	private readonly string _key;
	private readonly bool _defaultValue;
	private bool _hasLoaded;
	private bool _value;

	public SavedBool(string key, bool defaultValue = false)
	{
		_key = key;
		_defaultValue = defaultValue;
	}

	public bool Value
	{
		get
		{
			EnsureLoaded();
			return _hasLoaded ? _value : _defaultValue;
		}
		set
		{
			_value = value;
			_hasLoaded = true;
			Main.saaSaveData.SetBool(_key, value);
		}
	}

	public void SetTrue()
	{
		Value = true;
	}

	public static implicit operator bool(SavedBool savedBool) => savedBool.Value;

	private void EnsureLoaded()
	{
		if (_hasLoaded)
		{
			return;
		}

		_value = Main.saaSaveData.GetBool(_key) ?? _defaultValue;
		_hasLoaded = true;
	}
}

public sealed class SavedFloat
{
	private readonly string _key;
	private readonly float _defaultValue;
	private bool _hasLoaded;
	private float _value;

	public SavedFloat(string key, float defaultValue = 0.0f)
	{
		_key = key;
		_defaultValue = defaultValue;
	}

	public float Value
	{
		get
		{
			EnsureLoaded();
			return _hasLoaded ? _value : _defaultValue;
		}
		set
		{
			_value = value;
			_hasLoaded = true;
			Main.saaSaveData.SetFloat(_key, value);
		}
	}

	public static implicit operator float(SavedFloat savedFloat) => savedFloat.Value;

	private void EnsureLoaded()
	{
		if (_hasLoaded)
		{
			return;
		}

		_value = Main.saaSaveData.GetFloat(_key) ?? _defaultValue;
		_hasLoaded = true;
	}
}

public sealed class SavedInt
{
	private readonly string _key;
	private readonly int _defaultValue;
	private bool _hasLoaded;
	private int _value;

	public SavedInt(string key, int defaultValue = 0)
	{
		_key = key;
		_defaultValue = defaultValue;
	}

	public int Value
	{
		get
		{
			EnsureLoaded();
			return _hasLoaded ? _value : _defaultValue;
		}
		set
		{
			_value = value;
			_hasLoaded = true;
			Main.saaSaveData[_key] = value;
		}
	}

	public static implicit operator int(SavedInt savedInt) => savedInt.Value;

	private void EnsureLoaded()
	{
		if (_hasLoaded)
		{
			return;
		}

		_value = Main.saaSaveData.Value<int?>(_key) ?? _defaultValue;
		_hasLoaded = true;
	}
}
