using Darkrit.InputSystem.Providers;

namespace Darkrit.InputSystem;

internal class InputRecordingController
{
    private readonly Input _input;
    private readonly RecordInputProvider _recordInputProvider;
    private readonly ReplayInputProvider _replayInputProvider;

    private bool _requestedRecording;

    public bool IsRecording => _recordInputProvider.IsRecording;
    public bool IsReplaying => _replayInputProvider.IsReplaying;
    public bool HasRecording => _recordInputProvider.HasRecording;

    public int RecordedFrames => _recordInputProvider.RecordedFrames;
    public int CurrentFrame => _replayInputProvider.CurrentFrame;
    public int TotalFrames => _replayInputProvider.TotalFrames;

    public bool RecordingRequested => _requestedRecording;

    public InputRecordingController(
        Input input,
        ActivatableInputProvider activatableInputProvider)
    {
        _input = input;

        _recordInputProvider = new(activatableInputProvider);
        _replayInputProvider = new();

        _replayInputProvider.OnPlaybackFinished += OnInputPlaybackFinished;
    }

    public void RequestRecording() => _requestedRecording = true;

    public void CancelRecording() => _requestedRecording = false;

    public void StartRecording()
    {
        _input.SetProvider(_recordInputProvider);
        _recordInputProvider.StartRecording();
        _requestedRecording = false;
    }

    public void StopRecording() => _recordInputProvider.StopRecording();

    public void StartReplay()
    {
        _input.SetProvider(_replayInputProvider);
        _replayInputProvider.StartReplay(_recordInputProvider.GetRecordedFrames());
    }

    private void OnInputPlaybackFinished() => _input.SetProvider(_recordInputProvider);
}