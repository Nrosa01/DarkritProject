using Darkrit.EntityModel;
using Darkrit.Math;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Darkrit.Editor;

public static class EditorFieldDrawer
{
    public static bool IsEditorFieldSupported(FieldInfo field)
    {
        Type type = field.FieldType;

        return type == typeof(int) ||
               type == typeof(float) ||
               type == typeof(bool) ||
               type == typeof(Vector2) ||
               type == typeof(Transform2D) ||
               type.IsEnum;
    }

    public static int GetEditorFieldCount(FieldInfo[] EditorFields)
    {
        int count = 0;

        foreach (FieldInfo field in EditorFields)
        {
            if (IsEditorFieldSupported(field))
                count++;
        }

        return count;
    }

    public static void DrawFields<T>(FieldInfo[] fields, ref T component) where T : struct
    {
        if (!ImGui.BeginTable("##Fields", 2))
            return;

        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 120.0f);
        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

        foreach (FieldInfo field in fields)
        {
            if (!IsEditorFieldSupported(field))
                continue;

            HeaderAttribute header = field.GetCustomAttribute<HeaderAttribute>();

            if (header != null)
            {
                ImGui.EndTable();

                ImGui.SeparatorText(header.Text);

                if (!ImGui.BeginTable("##Fields", 2))
                    return;

                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 120.0f);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
            }

            Draw(field, ref component);
        }

        ImGui.EndTable();
    }

    public static bool Draw<T>(FieldInfo field, ref T owner, bool showName = true) where T : struct
    {
        bool hideInInspector = field.IsDefined(typeof(EntityModel.HideInInspectorAttribute));

        if (hideInInspector)
            return false;
        
        string name = GetDisplayName(field);
        object value = field.GetValue(owner);

        bool readOnly = field.IsDefined(typeof(EntityModel.ReadOnlyAttribute));

        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        if (showName)
            ImGui.Text(name);

        ImGui.TableSetColumnIndex(1);

        if (readOnly)
            ImGui.BeginDisabled();

        bool changed = false;

        if (value is int intValue)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.DragInt($"##{field.Name}", ref intValue))
            {
                field.SetValueDirect(__makeref(owner), intValue);
                changed = true;
            }
        }
        else if (value is float floatValue)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.DragFloat($"##{field.Name}", ref floatValue))
            {
                field.SetValueDirect(__makeref(owner), floatValue);
                changed = true;
            }
        }
        else if (value is bool boolValue)
        {
            if (ImGui.Checkbox($"##{field.Name}", ref boolValue))
            {
                field.SetValueDirect(__makeref(owner), boolValue);
                changed = true;

                if (readOnly)
                    ImGui.BeginDisabled();
            }
        }
        else if (value is Vector2 vector2Value)
        {
            bool linkable = field.IsDefined(typeof(LinkableAttribute));

            if (DrawVector2(field.Name, ref vector2Value, linkable))
            {
                field.SetValueDirect(__makeref(owner), vector2Value);
                changed = true;
            }
        }
        else if (value is Transform2D transform)
        {
            if (DrawTransform2D(field.Name, ref transform))
            {
                field.SetValueDirect(__makeref(owner), transform);
                changed = true;
            }
        }
        else if (value is Enum enumValue)
        {
            if (DrawEnum(field.Name, enumValue, out Enum newValue))
            {
                field.SetValueDirect(__makeref(owner), newValue);
                changed = true;
            }
        }

        if (readOnly)
            ImGui.EndDisabled();

        return changed;
    }

    static bool DrawEnum(string id, Enum value, out Enum newValue)
    {
        newValue = value;

        string preview = value.ToString();

        ImGui.SetNextItemWidth(-1);

        if (!ImGui.BeginCombo($"##{id}", preview))
            return false;

        bool changed = false;
        Type enumType = value.GetType();

        foreach (Enum option in Enum.GetValues(enumType))
        {
            bool selected = Equals(option, value);

            if (ImGui.Selectable(option.ToString(), selected))
            {
                newValue = option;
                changed = true;
            }

            if (selected)
                ImGui.SetItemDefaultFocus();
        }

        ImGui.EndCombo();

        return changed;
    }

    private static bool DrawTransform2D(string id, ref Transform2D value)
    {
        bool changed = false;

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Position");
        ImGui.TableSetColumnIndex(1);

        changed |= DrawVector2($"{id}Position", ref value.Position);

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Rotation");
        ImGui.TableSetColumnIndex(1);

        float rotation = MathHelper.ToDegrees(value.Rotation);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.DragFloat($"##{id}Rotation", ref rotation))
        {
            value.Rotation = MathHelper.ToRadians(rotation);
            changed = true;
        }

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Scale");
        ImGui.TableSetColumnIndex(1);

        FieldInfo scaleField = typeof(Transform2D).GetField(nameof(Transform2D.Scale));
        bool linkable = scaleField.IsDefined(typeof(LinkableAttribute));

        changed |= DrawVector2($"{id}Scale", ref value.Scale, linkable);

        return changed;
    }

    private static bool DrawVector2(string id, ref Vector2 value, bool linkable = false)
    {
        float availableWidth = ImGui.GetContentRegionAvail().X;
        float spacing = ImGui.GetStyle().ItemInnerSpacing.X;

        const float minWidthForInline = 220.0f;

        bool linked = linkable;

        if (VectorLinkStates.TryGetValue(id, out VectorLinkState state))
            linked = state.Linked;

        float oldX = value.X;
        float oldY = value.Y;

        bool changedX;
        bool changedY;
        bool activeX;
        bool activeY;

        // Helper to draw the link checkbox and modify the linked state in one go
        static void DrawLinkCheckbox(string id, ref bool linked)
        {
            if (ImGui.Checkbox($"##{id}Link", ref linked))
            {
                if (VectorLinkStates.TryGetValue(id, out VectorLinkState state))
                    VectorLinkStates[id] = new VectorLinkState(state.Ratio, state.XIsReference, linked);
                else
                    VectorLinkStates[id] = new VectorLinkState(0.0f, true, linked);
            }
        }

        if (availableWidth < minWidthForInline)
        {
            changedX = DrawVectorComponent($"{id}X", ref value.X, "X", true, out activeX);
            changedY = DrawVectorComponent($"{id}Y", ref value.Y, "Y", false, out activeY);

            if (linkable)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Link");
                ImGui.SameLine(0.0f, 4.0f);
                DrawLinkCheckbox(id, ref linked);
            }
        }
        else
        {
            float labelWidth = ImGui.CalcTextSize("X").X;
            float linkWidth = linkable ? ImGui.GetFrameHeight() + spacing : 0.0f;

            float componentWidth = (availableWidth - labelWidth * 2.0f - spacing * 4.0f - linkWidth) * 0.5f;

            changedX = DrawVectorComponent($"{id}X", ref value.X, "X", true, out activeX, componentWidth);

            ImGui.SameLine();

            changedY = DrawVectorComponent($"{id}Y", ref value.Y, "Y", false, out activeY, componentWidth);

            if (linkable)
            {
                ImGui.SameLine();
                DrawLinkCheckbox(id, ref linked);
            }
        }

        bool changed = changedX || changedY;

        if (linked && changed)
        {
            if (activeX && !activeY)
            {
                if (!VectorLinkStates.TryGetValue(id, out state) || !state.Linked)
                {
                    float ratio = oldX != 0.0f ? oldY / oldX : 1.0f;
                    state = new VectorLinkState(ratio, true, true);
                    VectorLinkStates[id] = state;
                }

                value.Y = value.X * state.Ratio;
            }
            else if (activeY && !activeX)
            {
                if (!VectorLinkStates.TryGetValue(id, out state) || !state.Linked)
                {
                    float ratio = oldY != 0.0f ? oldX / oldY : 1.0f;
                    state = new VectorLinkState(ratio, false, true);
                    VectorLinkStates[id] = state;
                }

                value.X = value.Y * state.Ratio;
            }
        }

        if (!activeX && !activeY && VectorLinkStates.TryGetValue(id, out state) && state.Linked != linked)
            VectorLinkStates[id] = new VectorLinkState(state.Ratio, state.XIsReference, linked);

        return changed;
    }

    private readonly struct VectorLinkState(float ratio, bool xIsReference, bool linked)
    {
        public readonly float Ratio = ratio;
        public readonly bool XIsReference = xIsReference;
        public readonly bool Linked = linked;
    }

    private static readonly Dictionary<string, VectorLinkState> VectorLinkStates = [];

    private static bool DrawVectorComponent(string id, ref float value, string label, bool isX, out bool active, float width = -1.0f)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);
        ImGui.SameLine(0.0f, 4.0f);

        ImGui.SetNextItemWidth(width);

        ImGui.PushStyleColor(
            ImGuiCol.FrameBg,
            isX
                ? new System.Numerics.Vector4(0.35f, 0.10f, 0.10f, 1f)
                : new System.Numerics.Vector4(0.10f, 0.35f, 0.10f, 1f));

        bool changed = ImGui.DragFloat($"##{id}", ref value);
        active = ImGui.IsItemActive();

        ImGui.PopStyleColor();

        return changed;
    }

    /// <summary>
    /// Gets a human-readable display name for the specified field.
    /// </summary>
    /// <param name="field"> The field for which the display name is generated. </param>
    /// <returns> The field name converted to a display-friendly format. </returns>
    public static string GetDisplayName(FieldInfo field) => ToDisplayName(GetFieldName(field));

    /// <summary>
    /// Gets the name of the specified field, removing compiler-generated backing
    /// field suffixes and common private field prefixes.
    /// </summary>
    /// <param name="field"> The field whose name is retrieved. </param>
    /// <returns>
    /// The field name with compiler-generated backing field syntax and common
    /// private field prefixes removed.
    /// </returns>
    public static string GetFieldName(FieldInfo field)
    {
        const string suffix = "k__BackingField";

        if (field.Name.StartsWith('<') && field.Name.EndsWith(suffix))
            return field.Name[1..^(suffix.Length + 1)];

        string name = field.Name;

        if (name.StartsWith("m_", StringComparison.Ordinal) ||
            name.StartsWith("s_", StringComparison.Ordinal))
        {
            return name[2..];
        }

        if (name.StartsWith('_'))
            return name[1..];

        return name;
    }

    /// <summary>
    /// Converts a name written in PascalCase or camelCase into a display-friendly
    /// name by inserting spaces before uppercase characters and capitalizing
    /// the first character.
    /// </summary>
    /// <param name="name"> The name to convert. </param>
    /// <returns>
    /// The converted display name, or the original value when the input is
    /// <see langword="null"/> or empty.
    /// </returns>
    public static string ToDisplayName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        StringBuilder builder = new(name.Length + 8);

        builder.Append(char.ToUpperInvariant(name[0]));

        for (int i = 1; i < name.Length; i++)
        {
            char current = name[i];

            if (char.IsUpper(current))
                builder.Append(' ');

            builder.Append(current);
        }

        return builder.ToString();
    }

    // Buttons

    private static readonly System.Numerics.Vector4 ButtonColor = new(0.15f, 0.30f, 0.55f, 1.0f);

    private static readonly System.Numerics.Vector4 ButtonHoveredColor = new(0.20f, 0.40f, 0.70f, 1.0f);

    private static readonly System.Numerics.Vector4 ButtonActiveColor = new(0.10f, 0.25f, 0.45f, 1.0f);

    public static bool DrawButton<T>(MethodInfo method, ref T owner) where T : struct
    {
        if (!method.IsDefined(typeof(ButtonAttribute)))
            return false;

        ButtonAttribute attribute = method.GetCustomAttribute<ButtonAttribute>();
        string name = attribute?.Name ?? ToDisplayName(method.Name);

        ImGui.Dummy(new System.Numerics.Vector2(0.0f, 6.0f));

        ImGui.PushStyleColor(ImGuiCol.Button, ButtonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ButtonHoveredColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ButtonActiveColor);

        bool clicked = ImGui.Button(name, new System.Numerics.Vector2(-1.0f, 0.0f));

        ImGui.PopStyleColor(3);

        ImGui.Dummy(new System.Numerics.Vector2(0.0f, 6.0f));

        if (!clicked)
            return false;

        object boxedOwner = owner;
        method.Invoke(boxedOwner, null);
        owner = (T)boxedOwner;

        return true;
    }
}