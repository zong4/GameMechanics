using System.Collections.Generic;
using Data;
using UnityEngine;

public class PathValidator : MonoBehaviour
{
    public List<string> Validate(List<GridNode> path, PuzzleData puzzle)
    {
        var errors = new List<string>();
        if (path.Count < 2)
        {
            errors.Add("路径太短");
            return errors;
        }

        // 构建路径节点集合

        // ① 必须经过所有强制点
        foreach (var dot in puzzle.RequiredDots)
        {
            if (!path.Contains(dot))
            {
                errors.Add("未经过必须点");
                return errors;
            }
        }

        // ② 计算用户路径划分的区域
        var regions = RegionSolver.ComputeRegions(path, puzzle.GridSize);

        // ③ 黑白格分离校验
        var regionTypes = new Dictionary<int, HashSet<CellType>>();
        foreach (var kv in puzzle.Cells)
        {
            if (kv.Value != CellType.White && kv.Value != CellType.Black) continue;
            if (!regions.TryGetValue(kv.Key, out int reg)) continue;
            if (!regionTypes.ContainsKey(reg)) regionTypes[reg] = new HashSet<CellType>();
            regionTypes[reg].Add(kv.Value);
        }

        foreach (var kv in regionTypes)
        {
            if (kv.Value.Contains(CellType.White) && kv.Value.Contains(CellType.Black))
            {
                errors.Add("黑白格在同一区域");
                return errors;
            }
        }

        // ④ 星形符号配对校验
        var starsByRegion = new Dictionary<int, Dictionary<CellType, int>>();
        foreach (var kv in puzzle.Cells)
        {
            if (kv.Value != CellType.StarGold && kv.Value != CellType.StarTeal) continue;
            if (!regions.TryGetValue(kv.Key, out int reg)) continue;
            if (!starsByRegion.ContainsKey(reg)) starsByRegion[reg] = new Dictionary<CellType, int>();
            starsByRegion[reg].TryGetValue(kv.Value, out int cnt);
            starsByRegion[reg][kv.Value] = cnt + 1;
        }

        foreach (var region in starsByRegion)
        {
            foreach (var kv in region.Value)
            {
                if (kv.Value != 2)
                {
                    errors.Add($"星形符号未正确配对（{kv.Key}）");
                    return errors;
                }
            }
        }

        return errors; // 空 = 全部通过
    }
}