using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSelectionGroup : UIElement
{
    private Dictionary<(int x, int y), (Texture2D texture, object o)> grid
        = new Dictionary<(int x, int y), (Texture2D texture, object o)>();

    private Dictionary<(int x, int y), RawImage> images
        = new Dictionary<(int x, int y), RawImage>();

    public (int x, int y) currentSelection;

    private event Action<Texture2D, object, GamepadPoll> OnSelectionEventHandler;

    public static ObjectSelectionGroup Create(
        GameObject parent,
        int iconSize,
        int padding,
        Action<Texture2D, object, GamepadPoll> OnSelectionEventHandler,
        int selectionGroupWidth = 5,
        IEnumerable<(Texture2D texture, object o)> objects = null)
    {
        return Create(parent, iconSize, padding, OnSelectionEventHandler, selectionGroupWidth, objects.ToArray());
    }

    public static ObjectSelectionGroup Create(
        GameObject parent,
        int iconSize,
        int padding,
        Action<Texture2D, object, GamepadPoll> OnSelectionEventHandler,
        int selectionGroupWidth = 5,
        params (Texture2D texture, object o)[] objects)
    {
        var output = parent.AddComponent<ObjectSelectionGroup>();
        return output.Init(iconSize, padding, OnSelectionEventHandler, selectionGroupWidth, objects);
    }

    private ObjectSelectionGroup Init(
        int iconSize,
        int padding,
        Action<Texture2D, object, GamepadPoll> OnSelectionEventHandler,
        int selectionGroupWidth,
        (Texture2D texture, object o)[] objects)
    {
        this.OnSelectionEventHandler = OnSelectionEventHandler;
        var indexes = GetIndexes(selectionGroupWidth);
        foreach (var pair in objects)
        {
            indexes.MoveNext();
            var currentIndex = indexes.Current;
            grid[currentIndex] = pair;
        }

        PopulateCanvas(iconSize, padding);

        SetSelection(0, 0);

        return this;
    }

    private void PopulateCanvas(int iconSize, int padding)
    {
        foreach (var pair in grid)
        {
            var (x, y) = pair.Key;
            var (texture, t) = pair.Value;
            var image = GameObjectFactory.Create<RawImage>(
                $"({x}, {y})",
                new Vector3(x * iconSize + padding, -y * iconSize + padding),
                parent: transform);

            images[(x, y)] = image;

            image.texture = texture;
        }
    }

    private static IEnumerator<(int x, int y)> GetIndexes(int width = 5)
    {
        int y = 0;
        while (true)
        {
            for (int x = 0; x < width; x++)
            {
                yield return (x, y);
            }
            y++;
        }
    }

    public override bool OnEvent(object sender, CEvent e)
    {
        if (e is GamepadInputEvent inputEvent)
        {
            var gamepad = inputEvent.gamepad;
            if (gamepad.GetButtonDown(Gamepad.Button.DPadLeft))
            {
                SetSelection(currentSelection.x - 1, currentSelection.y);
            }
            if (gamepad.GetButtonDown(Gamepad.Button.DPadRight))
            {
                SetSelection(currentSelection.x + 1, currentSelection.y);
            }
            if (gamepad.GetButtonDown(Gamepad.Button.DPadUp))
            {
                SetSelection(currentSelection.x, currentSelection.y - 1);
            }
            if (gamepad.GetButtonDown(Gamepad.Button.DPadDown))
            {
                SetSelection(currentSelection.x, currentSelection.y + 1);
            }
            if (gamepad.GetAnyButtonDown(Gamepad.Button.X, Gamepad.Button.Y, Gamepad.Button.B))
            {
                var (texture, o) = grid[currentSelection];
                OnSelectionEventHandler(texture, o, gamepad);
            }
            if (gamepad.GetButtonDown(Gamepad.Button.A))
            {
                Pop(true);
            }
        }

        return true;
    }

    private bool SetSelection(int x, int y)
    {
        if (grid.ContainsKey((x, y)))
        {
            images[currentSelection].color = Color.white;
            images[(x, y)].color = Color.green;
            currentSelection = (x, y);
            return true;
        }
        return false;
    }

    public override void OnEventInactive(object sender, CEvent e)
    {

    }
}