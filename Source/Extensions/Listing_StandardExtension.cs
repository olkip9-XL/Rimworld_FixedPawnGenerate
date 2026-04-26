using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace FixedPawnGenerate;

public static class Listing_StandardExtension
{

    public static string curId;
    public static string curBuffer;

    public static void TextFieldNumericLabeled<T>(this Listing_Standard listing, string label, ref T value, ref string buffer, float labelPct = 0.5f, string tooltip = null, float min = 0, float max = 1E+09f) where T : struct
    {
        Rect rect = listing.GetRect(Text.LineHeight);

        Rect leftRect = rect.LeftPart(labelPct);
        Rect rightRect = rect.RightPart(1 - labelPct);

        if (!listing.BoundingRectCached.HasValue || rect.Overlaps(listing.BoundingRectCached.Value))
        {
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(leftRect))
                {
                    Widgets.DrawHighlight(leftRect);
                }

                TooltipHandler.TipRegion(leftRect, tooltip);
            }

            Widgets.Label(leftRect, label);
            Widgets.TextFieldNumeric(rightRect, ref value, ref buffer, min, max);
        }

        listing.Gap(listing.verticalSpacing);
    }

    public static bool ButtonTextCenter(this Listing_Standard listing, string label, string highlightTag = null, float widthPct = 1f)
    {
        Rect rect = listing.GetRect(30f);

        Rect centerRect = rect;
        centerRect.width = rect.width * widthPct;
        centerRect = centerRect.CenteredOnXIn(rect);

        bool result = false;
        if (!listing.BoundingRectCached.HasValue || rect.Overlaps(listing.BoundingRectCached.Value))
        {
            result = Widgets.ButtonText(centerRect, label);
            if (highlightTag != null)
            {
                UIHighlighter.HighlightOpportunity(centerRect, highlightTag);
            }
        }

        listing.Gap(listing.verticalSpacing);
        return result;
    }

    public static string TextEntryLabeled(this Listing_Standard listing, string label, string text, string tooltip = null, float labelPct = 0.5f, int lineCount = 1)
    {
        Rect rect = listing.GetRect(Text.LineHeight * (float)lineCount);

        Rect leftRect = rect.LeftPart(labelPct);
        Rect rightRect = rect.RightPart(1 - labelPct);

        if (!tooltip.NullOrEmpty())
        {
            if (Mouse.IsOver(leftRect))
            {
                Widgets.DrawHighlight(leftRect);
            }

            TooltipHandler.TipRegion(leftRect, tooltip);
        }

        Widgets.Label(leftRect, label);

        string result = "";
        if (rect.height <= 30f)
        {
            result = Widgets.TextField(rightRect, text);
        }
        else
        {
            result = Widgets.TextArea(rightRect, text);
        }

        listing.Gap(listing.verticalSpacing);

        return result;
    }

    public static void FieldLine<T>(this Listing_Standard listing, TaggedString label, ref T value, float fieldWidth = 100f, string tooltip = null, float indent = 0, float min = float.MinValue, float max = float.MaxValue) where T : struct
    {
        Rect rect = listing.GetRect(Text.LineHeight);
        rect.x += indent;
        rect.width -= indent;

        Widgets.Label(rect, label);

        if (value.GetType() == typeof(bool))
        {
            Rect fieldRect = rect.RightPartPixels(rect.height);
            bool temp = (bool)(object)value;

            Widgets.Checkbox(new Vector2(fieldRect.x, fieldRect.y), ref temp);
            if (temp != (bool)(object)value)
            {
                value = (T)(object)temp;
            }
        }
        else
        {
            Rect fieldRect = rect.RightPartPixels(fieldWidth);

            //Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer, min, max);
            TextField(fieldRect, ref value, min, max);
        }

        Widgets.DrawHighlightIfMouseover(rect);

        if (!tooltip.NullOrEmpty())
        {
            TooltipHandler.TipRegion(rect, tooltip);
        }

        listing.Gap(listing.verticalSpacing);
    }

    private static void TextField<T>(Rect rect, ref T value, float min = float.MinValue, float max = float.MaxValue) where T : struct
    {
        T temp = value;
        string buffer = value.ToString();

        string id = "TextField" + rect.y.ToString("F0") + rect.x.ToString("F0");

        if (curId == id)
        {
            if (curBuffer == null)
            {
                curBuffer = buffer;
            }

            Widgets.TextFieldNumeric(rect, ref temp, ref curBuffer, min, max);
            buffer = curBuffer;
        }
        else
        {
            Widgets.TextFieldNumeric(rect, ref temp, ref buffer, min, max);
        }

        if (buffer.NullOrEmpty())
        {
            temp = default(T);
        }

        if (value.ToString() != buffer)
        {
            value = temp;
        }
    }

    public static void GUITick()
    {
        string newId = GUI.GetNameOfFocusedControl();
        if (curId != newId)
        {
            curId = newId;
            curBuffer = null;
        }
    }

}
