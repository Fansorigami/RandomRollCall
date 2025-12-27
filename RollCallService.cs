using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public sealed class RollCallService
{
    private readonly Random _rng = new();
    private readonly string _path;

    private List<string> _all = new();
    private List<string> _bag = new();
    private bool _noRepeatMode = true;

    public RollCallService(string path)
    {
        _path = path;
        Reload();
    }

    public bool NoRepeatMode => _noRepeatMode;

    public void SetNoRepeat(bool enabled)
    {
        _noRepeatMode = enabled;
        if (_noRepeatMode) ResetBag();
    }

    public void Reload()
    {
        if (!File.Exists(_path))
            File.WriteAllText(_path, "张三\n李四\n王五\n");

        _all = File.ReadAllLines(_path)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct()
            .ToList();

        ResetBag();
    }

    public void ResetBag()
    {
        _bag = _all.ToList();
        Shuffle(_bag);
    }

    public string Pick()
    {
        if (_all.Count == 0) return "（名单为空）";

        if (!_noRepeatMode)
            return _all[_rng.Next(_all.Count)];

        if (_bag.Count == 0) ResetBag();

        var name = _bag[0];
        _bag.RemoveAt(0);
        return name;
    }

    public int RemainingInRound => _noRepeatMode ? _bag.Count : -1;
    public int Total => _all.Count;

    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}