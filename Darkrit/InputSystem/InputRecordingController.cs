// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Darkrit.InputSystem.Providers;

namespace Darkrit.InputSystem;

/// <summary>
/// Allows to record all kind of Input. This class depends on <see cref="Core"/>
/// orchestration and it can't be instantiated outside the engine.
/// </summary>
public class InputRecordingController
{
    private readonly Input _input;
    private readonly RecordInputProvider _recordInputProvider;
    private readonly ReplayInputProvider _replayInputProvider;

    private bool _requestedRecording;

    /// <summary>
    /// Whether the recorder is recording
    /// </summary>
    public bool IsRecording => _recordInputProvider.IsRecording;
    
    /// <summary>
    /// Whether the replayer is replying
    /// </summary>
    public bool IsReplaying => _replayInputProvider.IsReplaying;
    
    /// <summary>
    /// Wnether the recorded currently has any recording
    /// </summary>
    public bool HasRecording => _recordInputProvider.HasRecording;

    /// <summary>
    /// Amount of recording frames in the current recording
    /// </summary>
    public int RecordedFramesCount => _recordInputProvider.RecordedFrames;
    
    /// <summary>
    /// Current frame that is replying. 0 if there is no replay ongoing
    /// </summary>
    public int CurrentFrameIndex => _replayInputProvider.CurrentFrame;
    
    /// <summary>
    /// Amount of frames in the current replay
    /// </summary>
    public int ReplayFramesCount => _replayInputProvider.TotalFrames;

    /// <summary>
    /// Whether a recording has been requested but not yet processed.
    /// </summary>
    public bool RecordingRequested => _requestedRecording;

    internal InputRecordingController() => throw new InvalidOperationException("This class is not meant to be instantiated.");

    internal InputRecordingController(
        Input input,
        ActivatableInputProvider activatableInputProvider)
    {
        _input = input;

        _recordInputProvider = new(activatableInputProvider);
        _replayInputProvider = new();

        _replayInputProvider.OnPlaybackFinished += OnInputPlaybackFinished;
    }

    /// <summary>
    /// Requests an input recording. It will start when the game is focused
    /// </summary>
    public void RequestRecording() => _requestedRecording = true;

    /// <summary>
    /// If there is no recording ongoing but one was requested, this function cancels it.
    /// In any other case it does nothing.
    /// </summary>
    public void CancelRecordingRequest() => _requestedRecording = false;

    /// <summary>
    /// Sets the Input Provider to the record input provider to actually start recording input
    /// </summary>
    internal void StartRecording()
    {
        _input.SetProvider(_recordInputProvider);
        _recordInputProvider.StartRecording();
        _requestedRecording = false;
    }


    /// <summary>
    /// Stops the current recording, if any
    /// </summary>
    public void StopRecording() => _recordInputProvider.StopRecording();

    /// <summary>
    /// Replays the last stored recording
    /// </summary>
    public void StartReplay()
    {
        _input.SetProvider(_replayInputProvider);
        _replayInputProvider.StartReplay(_recordInputProvider.GetRecordedFrames());
    }

    private void OnInputPlaybackFinished() => _input.SetProvider(_recordInputProvider);
}