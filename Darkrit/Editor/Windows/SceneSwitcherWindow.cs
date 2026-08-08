using System;
using System.Collections.Generic;
using Darkrit.Base;
using Darkrit.Scenes;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace Darkrit.Editor.Windows;

/// <summary>
/// Displays every class that derives from Scene as a button so changing scenes it's easy
/// </summary>
internal class SceneSwitcherWindow : IEditorOverlay
{
    readonly IReadOnlyList<Type> _sceneTypes = ReflectionUtils.FindAllDerivedTypes<Scene>();

    public void Draw(GameTime gameTime)
    {
        ImGui.Begin("Scene Switcher");

        foreach (var sceneType in _sceneTypes)
        {
            if (ImGui.Button(sceneType.Name))
                Core.ChangeScene((Scene)Activator.CreateInstance(sceneType));
        }

        ImGui.End();
    }
}
