using System;
using Darkrit;
using Darkrit.EntityModel;
using Darkrit.EntityModel.Components;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Bindings;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;
using GamepadButton = Microsoft.Xna.Framework.Input.Buttons;
using Key = Microsoft.Xna.Framework.Input.Keys;

namespace DarkritGame.Scenes;

[Component]
[InjectComponent(typeof(PhysicsBody))]
public partial struct PlayerController
{
    InputAction moveLeft;
    InputAction moveRight;
    InputAction jump;

    [ShowInInspector, ReadOnly] Vector2 direction;

    [SerializeField] readonly float maxSpeed = 180f;
    [SerializeField] readonly float acceleration = 1200f;
    [SerializeField] readonly float deceleration = 1600f;
    [SerializeField] float gravity = 3600f;
    [SerializeField] readonly float jumpSpeed = 1000f;

    [Button]
    void InvertGravity()
    {
        gravity = gravity * -1;
    }

    public void OnAdd()
    {
        moveLeft = Core.Input.CreateAction("Move Left").AddBindings([
            new KeyboardBinding(Key.Left),
            new KeyboardBinding(Key.A),
            new GamepadBinding(GamepadButton.DPadLeft),
            new GamepadBinding(GamepadButton.LeftThumbstickLeft),
        ]);

        moveRight = Core.Input.CreateAction("Move Right").AddBindings([
            new KeyboardBinding(Key.Right),
            new KeyboardBinding(Key.D),
            new GamepadBinding(GamepadButton.DPadRight),
            new GamepadBinding(GamepadButton.LeftThumbstickRight),
        ]);

        jump = Core.Input.CreateAction("Jump").AddBindings([
            new KeyboardBinding(Key.Space),
            new KeyboardBinding(Key.Up),
            new GamepadBinding(GamepadButton.A),
        ]);
    }

    bool jumpRequested = false;

    public void Update(GameTime gameTime)
    {
        jumpRequested |= jump.WasPressedThisFrame;
    }

    public void FixedUpdate(GameTime gameTime)
    {
        direction.X = 0;

        if (moveLeft.IsPressed)
            direction.X -= 1;

        if (moveRight.IsPressed)
            direction.X += 1;

        float targetSpeed = direction.X * maxSpeed;

        float rate = direction.X == 0 ? deceleration : acceleration;

        PhysicsBody.Velocity.X = MoveTowards(PhysicsBody.Velocity.X, targetSpeed, rate * gameTime.Delta);

        PhysicsBody.upDirection = gravity >= 0f ? new Vector2(0f, -1f) : new Vector2(0f, 1f);

        if (PhysicsBody.IsOnFloor)
        {
            if (jumpRequested)
            {
                jumpRequested = false;
                PhysicsBody.Velocity.Y = -jumpSpeed;
            }
            else if (PhysicsBody.Velocity.Y > 0)
                PhysicsBody.Velocity.Y = 0;
        }
        else
        {
            PhysicsBody.Velocity.Y += gravity * gameTime.Delta;
        }

        PhysicsBody.MoveAndSlide(gameTime);
    }

    static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;

        return current + MathF.Sign(target - current) * maxDelta;
    }
}
