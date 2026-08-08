// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using Darkrit.InputSystem.Bindings;
using Darkrit.InputSystem.ReplaySystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.Providers;

/// <summary>
/// Input provider that records the result of other input provider.
/// 
/// </summary>
internal class RecordInputProvider(ISerializableInputProvider providerToRecord) : IInputProvider
{
    private readonly List<InputFrame> _frames = new(60 * 60 * 10); // 60fps * 10 minutes

    private bool recording = false;
    
    public bool IsRecording => recording;

    public bool HasRecording => !IsRecording && _frames.Count > 0;

    public int RecordedFrames => _frames.Count;

    public void StartRecording()
    {
        recording = true;
        _frames.Clear();
    }

    public void StopRecording()
    {
        recording = false;
    }

    public IReadOnlyList<InputFrame> GetRecordedFrames() => _frames;

    public void Update(GameTime gameTime)
    {
        providerToRecord.Update(gameTime);
        
        if(recording)
            _frames.Add(providerToRecord.CaptureFrame());
    }

    // Keyboard
    public bool IsKeyDown(Keys key) => providerToRecord.IsKeyDown(key);
    public bool IsKeyUp(Keys key) => providerToRecord.IsKeyUp(key);
    public bool WasKeyJustPressed(Keys key) => providerToRecord.WasKeyJustPressed(key);
    public bool WasKeyJustReleased(Keys key) => providerToRecord.WasKeyJustReleased(key);
    public Keys[] GetPressedKeys() => providerToRecord.GetPressedKeys();

    // Mouse
    public Point GetMousePosition() => providerToRecord.GetMousePosition();
    public int GetMouseScrollWheelValue() => providerToRecord.GetMouseScrollWheelValue();
    public bool IsMouseButtonDown(MouseButton button) => providerToRecord.IsMouseButtonDown(button);
    public bool IsMouseButtonUp(MouseButton button) => providerToRecord.IsMouseButtonUp(button);
    public bool WasMouseButtonJustPressed(MouseButton button) => providerToRecord.WasMouseButtonJustPressed(button);
    public bool WasMouseButtonJustReleased(MouseButton button) => providerToRecord.WasMouseButtonJustReleased(button);
    public Point GetMousePositionDelta() => providerToRecord.GetMousePositionDelta();

    // Gamepad
    public bool IsGamepadConnected(PlayerIndex playerIndex) => providerToRecord.IsGamepadConnected(playerIndex);
    public bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button) => providerToRecord.IsGamepadButtonDown(playerIndex, button);
    public bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button) => providerToRecord.IsGamepadButtonUp(playerIndex, button);
    public bool WasGamepadButtonJustPressed(PlayerIndex playerIndex, Buttons button) => providerToRecord.WasGamepadButtonJustPressed(playerIndex, button);
    public bool WasGamepadButtonJustReleased(PlayerIndex playerIndex, Buttons button) => providerToRecord.WasGamepadButtonJustReleased(playerIndex, button);
    public Vector2 GetGamepadLeftStick(PlayerIndex playerIndex) => providerToRecord.GetGamepadLeftStick(playerIndex);
    public Vector2 GetGamepadRightStick(PlayerIndex playerIndex) => providerToRecord.GetGamepadRightStick(playerIndex);
    public float GetGamepadLeftTrigger(PlayerIndex playerIndex) => providerToRecord.GetGamepadLeftTrigger(playerIndex);
    public float GetGamepadRightTrigger(PlayerIndex playerIndex) => providerToRecord.GetGamepadRightTrigger(playerIndex);
    public float GetGamepadAxis(PlayerIndex playerIndex, GamepadAxis axis) => providerToRecord.GetGamepadAxis(playerIndex, axis);
}
