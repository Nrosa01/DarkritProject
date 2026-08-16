// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Darkrit.Editor;

/// <summary>
/// Performance overlay that tracks memory, cpu, gpu, drawcalls...
/// </summary>
/// <param name="GraphicsDevice">The GraphicsDevice to use</param>
internal class CoreStats(GraphicsDevice GraphicsDevice)
{
    private readonly ProcessStats ProcessStats = new(Process.GetCurrentProcess());

    private const int HistorySize = 240;
    private readonly float[] _cpuRenderHistory = new float[HistorySize];
    private readonly float[] _cpuProcessHistory = new float[HistorySize];
    private int _historyIndex;

    public void AddCpuRenderTime(float ms)
    {
        _cpuRenderHistory[_historyIndex] = ms;
        _historyIndex = (_historyIndex + 1) % HistorySize;
    }

    private readonly Stopwatch _frameTimer = new();
    private readonly Stopwatch _processTimer = new();

    private float _cpuRenderMs;
    private float _cpuRenderAverageMs;

    private float _cpuProcessMs;
    private float _cpuProcessAverageMs;

    private float _fps;

    private readonly float[] _fpsHistory = new float[HistorySize];
    private int _fpsHistoryIndex;

    [Conditional("EDITOR_BUILD")]
    public void DrawStats()
    {
        ImGui.Begin("Renderer Stats");

        ImGui.Text($"FPS              : {_fps:0.0}");
        ImGui.Text($"CPU Compute Time : {_cpuProcessAverageMs:0.00} ms");
        ImGui.Text($"CPU Render Time  : {_cpuRenderAverageMs:0.00} ms");
        ImGui.Text($"Memory Usage     : {ProcessStats.Process.WorkingSet64 * 1e-6:F3}MB");
        ImGui.Text($"Peak Memory Usage: {ProcessStats.Process.PeakWorkingSet64 * 1e-6:F3}MB");
        ImGui.Text($"GC Memory        : {GC.GetTotalMemory(false) * 1e-6:F1}MB");
        ImGui.Text($"GC Memory Thread : {GC.GetAllocatedBytesForCurrentThread() * 1e-6:F1}MB");
        ImGui.Text($"Draw Calls       : {GraphicsDevice.Metrics.DrawCount}");
        ImGui.Text($"Sprites          : {GraphicsDevice.Metrics.SpriteCount}");
        ImGui.Text($"Primitives       : {GraphicsDevice.Metrics.PrimitiveCount}");
        ImGui.Text($"Textures         : {GraphicsDevice.Metrics.TextureCount}");
        ImGui.Text($"Targets          : {GraphicsDevice.Metrics.TargetCount}");
        ImGui.Text($"Clears           : {GraphicsDevice.Metrics.ClearCount}");

        ImGui.Separator();

        ImGui.PlotLines(
            "FPS",
            ref _fpsHistory[0],
            _fpsHistory.Length,
            _fpsHistoryIndex,
            $"{_fps:0.00}",
            0,
            60,
            new Vector2(0, 60).ToNumerics());

        ImGui.PlotLines(
            "CPU Process (ms)",
            ref _cpuProcessHistory[0],
            _cpuProcessHistory.Length,
            _historyIndex,
            $"{_cpuProcessMs:0.00} ms",
            0,
            20,
            new Vector2(0, 60).ToNumerics());

        ImGui.PlotLines(
            "CPU Frame (ms)",
            ref _cpuRenderHistory[0],
            _cpuRenderHistory.Length,
            _historyIndex,
            $"{_cpuRenderMs:0.00} ms",
            0,
            20,
            new Vector2(0, 60).ToNumerics());

        ImGui.End();
    }

    [Conditional("EDITOR_BUILD")]
    public void Update(GameTime gameTime)
    {
        ProcessStats.Update(gameTime.ElapsedGameTime.TotalSeconds);
    }

    [Conditional("EDITOR_BUILD")]
    internal void ProfileStartLogic()
    {
        _processTimer.Restart();
    }

    [Conditional("EDITOR_BUILD")]
    internal void ProfileEndLogic(GameTime gameTime)
    {
        _processTimer.Stop();

        _cpuProcessMs = (float)_processTimer.Elapsed.TotalMilliseconds;
        _fps = (float)(1.0 / (gameTime.ElapsedGameTime.TotalSeconds + _processTimer.Elapsed.TotalSeconds));

        _cpuProcessHistory[_historyIndex] = _cpuProcessMs;
        _historyIndex = (_historyIndex + 1) % HistorySize;

        const float alpha = 0.05f;
        _cpuProcessAverageMs += (_cpuProcessMs - _cpuProcessAverageMs) * alpha;
    }

    [Conditional("EDITOR_BUILD")]
    internal void ProfileStartRender()
    {
        _frameTimer.Restart();
    }


    [Conditional("EDITOR_BUILD")]
    internal void ProfileEndRender(GameTime gameTime)
    {
        _frameTimer.Stop();

        _cpuRenderMs = (float)_frameTimer.Elapsed.TotalMilliseconds;
        _fps = (float)(1.0 / (gameTime.ElapsedGameTime.TotalSeconds + _frameTimer.Elapsed.TotalSeconds));

        _cpuRenderHistory[_historyIndex] = _cpuRenderMs;
        _historyIndex = (_historyIndex + 1) % HistorySize;

        _fpsHistory[_fpsHistoryIndex] = _fps;
        _fpsHistoryIndex = (_fpsHistoryIndex + 1) % HistorySize;

        const float alpha = 0.05f;
        _cpuRenderAverageMs += (_cpuRenderMs - _cpuRenderAverageMs) * alpha;
    }
}
