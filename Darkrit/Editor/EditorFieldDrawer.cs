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
               type == typeof(Transform2D);
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

    public static bool Draw<T>(FieldInfo field, ref T owner, bool showName = true) where T : struct
    {
        string name = GetDisplayName(field);
        object value = field.GetValue(owner);

        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        if(showName)
            ImGui.Text(name);

        ImGui.TableSetColumnIndex(1);

        if (value is int intValue)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.DragInt($"##{field.Name}", ref intValue))
            {
                field.SetValueDirect(__makeref(owner), intValue);
                return true;
            }
        }
        else if (value is float floatValue)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.DragFloat($"##{field.Name}", ref floatValue))
            {
                field.SetValueDirect(__makeref(owner), floatValue);
                return true;
            }
        }
        else if (value is bool boolValue)
        {
            if (ImGui.Checkbox($"##{field.Name}", ref boolValue))
            {
                field.SetValueDirect(__makeref(owner), boolValue);
                return true;
            }
        }
        else if (value is Vector2 vector2Value)
        {
            bool linkable = field.IsDefined(typeof(LinkableAttribute));

            if (DrawVector2(field.Name, ref vector2Value, linkable))
            {
                field.SetValueDirect(__makeref(owner), vector2Value);
                return true;
            }
        }
        else if (value is Transform2D transform)
        {
            if (DrawTransform2D(field.Name, ref transform))
            {
                field.SetValueDirect(__makeref(owner), transform);
                return true;
            }
        }

        return false;
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

    private static void DrawLinkCheckbox(string id, ref bool linked)
    {
        if (ImGui.Checkbox($"##{id}Link", ref linked))
        {
            if (VectorLinkStates.TryGetValue(id, out VectorLinkState state))
                VectorLinkStates[id] = new VectorLinkState(state.Ratio, state.XIsReference, linked);
            else
                VectorLinkStates[id] = new VectorLinkState(0.0f, true, linked);
        }
    }

    private static void ApplyLinkedValue(string id, float oldX, float oldY, ref Vector2 value, bool changedX, bool changedY, bool linked)
    {
        if (!linked || (!changedX && !changedY))
            return;

        if (!VectorLinkStates.TryGetValue(id, out VectorLinkState state) || !state.Linked)
        {
            bool xIsReference = changedX;

            float ratio = xIsReference
                ? oldX != 0.0f ? oldY / oldX : 1.0f
                : oldY != 0.0f ? oldX / oldY : 1.0f;

            state = new VectorLinkState(ratio, xIsReference, linked);
            VectorLinkStates[id] = state;
        }

        if (state.XIsReference)
            value.Y = value.X * state.Ratio;
        else
            value.X = value.Y * state.Ratio;
    }

    private static bool DrawVectorComponent(string id, ref float value, string label, bool isX, out bool active, float width = -1.0f)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);
        ImGui.SameLine(0.0f, 4.0f);

        if (width > 0.0f)
            ImGui.SetNextItemWidth(width);
        else
            ImGui.SetNextItemWidth(-1.0f);

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


    private static string GetDisplayName(FieldInfo field) => ToDisplayName(GetFieldName(field));

    public static string GetFieldName(FieldInfo field)
    {
        const string suffix = "k__BackingField";

        if (field.Name.StartsWith('<') && field.Name.EndsWith(suffix))
            return field.Name[1..^(suffix.Length + 1)];

        return field.Name;
    }

    private static string ToDisplayName(string name)
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
}