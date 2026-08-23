using Darkrit.Math;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using System.Text;

namespace Darkrit.Editor;

public static class EditorFieldDrawer
{
    public static bool IsSupported(FieldInfo field)
    {
        Type type = field.FieldType;

        return type == typeof(int) ||
               type == typeof(float) ||
               type == typeof(bool) ||
               type == typeof(Vector2);
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
            if (DrawVector2(field.Name, ref vector2Value))
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

        changed |= DrawVector2($"{id}Scale", ref value.Scale);

        return changed;
    }

    private static bool DrawVector2(string id, ref Vector2 value)
    {
        float availableWidth = ImGui.GetContentRegionAvail().X;
        float spacing = ImGui.GetStyle().ItemInnerSpacing.X;

        const float minWidthForInline = 220.0f;

        bool changed = false;

        if (availableWidth < minWidthForInline)
        {
            changed |= DrawVectorComponent($"{id}X", ref value.X, "X", true);
            changed |= DrawVectorComponent($"{id}Y", ref value.Y, "Y", false);

            return changed;
        }

        float labelWidth = ImGui.CalcTextSize("X").X;

        float componentWidth = (availableWidth - labelWidth * 2.0f - spacing * 4.0f) * 0.5f;

        changed |= DrawVectorComponent($"{id}X", ref value.X, "X", true, componentWidth);

        ImGui.SameLine();

        changed |= DrawVectorComponent($"{id}Y", ref value.Y, "Y", false, componentWidth);

        return changed;
    }

    private static bool DrawVectorComponent(string id, ref float value, string label, bool isX, float width = -1.0f)
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

        ImGui.PopStyleColor();

        return changed;
    }

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