using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.Base;
using Darkrit.DevTools.Logger.Renderers;
using Darkrit.Scenes;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace Darkrit.Editor.Windows;

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
