using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

public class LevelBuilder : UIElement
{
    Gamepad gamepad;
    public LevelBuilderPlayer player;
    public LevelBuilderGrabPlayer grabPlayer;
    public ButtonGroupController buttonGroup;
    public GameObject currentShip { get; private set; }

    UIStack stack = new UIStack();

    public static Dictionary<string, LevelBuilderAsset> assets;

    public string gamepadState;

    private void Start()
    {
        stack.Push(player);
        stack.Push(buttonGroup);
        stack.Push(this);

        currentShip = new GameObject("Current Ship");

        if (assets == null)
        {
            var blocks = Resources.LoadAll<GameObject>("Models/SetPieces");
            var textures = Resources.LoadAll<Texture2D>("Models/SetPieces");
            Dictionary<string, Texture2D> textureMap = textures.ToDictionary(x => x.name, x => x);
            assets = blocks.ToDictionary(x => x.name, x => new LevelBuilderAsset(x, textureMap[x.name], x.name));

        }

        gamepad = new Gamepad(XInputDotNetPure.PlayerIndex.One);

        player.levelBuilder = this;
    }

    private void Update()
    {
        var poll = gamepad.Poll();
        gamepadState = poll.ToString();

        stack.SendEvent(this, new GamepadInputEvent(poll));
    }

    public override bool OnEvent(object sender, CEvent e)
    {
        if (e is GamepadInputEvent inputEvent)
        {
            var gamepad = inputEvent.gamepad;

            if (gamepad.GetButtonDown(Gamepad.Button.Back)) 
            { 
                DisplayObjectSelector();
            }
            if(gamepad.GetAnyButtonDown(Gamepad.Button.X, Gamepad.Button.Y, Gamepad.Button.B))
            {
                TrySpawn(gamepad);
            }
            if (gamepad.GetButtonDown(Gamepad.Button.DPadLeft))
            {
                Save("Autosave.save");
            }
            if (gamepad.GetButtonDown(Gamepad.Button.DPadRight))
            {
                Open("Autosave.save");
            }
        }

        return false;
    }

    private void DisplayObjectSelector()
    {
        GameObject child = GameObjectFactory.Create("GameObjectSelector", parent: transform);
        var rectTransform = child.AddComponent<RectTransform>();
        
        stack.Push(
                ObjectSelectionGroup.Create(
                    child,
                    100,
                    10,
                    OnBindObjectToHotkey,
                    5,
                    assets.Values.Select(x => (x.icon, (object)x)).ToArray()));
    }

    private void TrySpawn(GamepadPoll gamepad)
    {
        if(buttonGroup.ShouldSpawn(gamepad, out var targetToSpawn))
        {
            var newObject = Instantiate(targetToSpawn.gameObject, parent: currentShip.transform);
            var builderObject = newObject.AddComponent<LevelBuilderObject>();
            builderObject.objectName = targetToSpawn.filename;
            newObject.transform.position = player.transform.position + player.transform.forward * 10f;
            Grab(newObject);
        }
    }

    public void Grab(GameObject target)
    {
        grabPlayer.PushOnStack(target, stack);
    }

    private void OnBindObjectToHotkey(Texture2D texture, object o, GamepadPoll gamepad)
    {
        buttonGroup.SetSelection(o as LevelBuilderAsset, gamepad);
    }

    public override void OnEventInactive(object sender, CEvent e)
    {
        
    }

    public void Save(string filename)
    {
        ShipSave save = new ShipSave(currentShip.GetComponentsInChildren<LevelBuilderObject>().Select(x => new ShipPart(x)));

        XmlSerializer serializer = new XmlSerializer(typeof(ShipSave));
        serializer.Serialize(File.Create(filename), save);
    }

    public void Open(string filename)
    {
        ShipSave save = new XmlSerializer(typeof(ShipSave)).Deserialize(File.Open(filename, FileMode.Open)) as ShipSave;
        if(save != null)
        {
            Destroy(currentShip);
            currentShip = new GameObject("Current Ship");
            foreach(var part in save.parts)
            {
                GameObject partObject = GameObjectFactory.Instantiate(assets[part.partName].gameObject, parent: currentShip.transform);
                var builderObject = partObject.AddComponent<LevelBuilderObject>();
                builderObject.objectName = part.partName;
                partObject.transform.localPosition = part.partPosition;
                partObject.transform.localRotation = part.partRotation;
            }
        }
    }

    [Serializable]
    public class ShipSave
    {
        public List<ShipPart> parts = new List<ShipPart>();

        public ShipSave()
        {

        }

        public ShipSave(IEnumerable<ShipPart> parts)
        {
            this.parts = parts.ToList();
        }
    }

    [Serializable]
    public class ShipPart
    {
        public string partName;
        public Vector3 partPosition;
        public Quaternion partRotation;

        public ShipPart()
        {

        }

        public ShipPart(LevelBuilderObject builderObject)
        {
            partName = builderObject.objectName;
            partPosition = builderObject.transform.localPosition;
            partRotation = builderObject.transform.localRotation;
        }
    }
}
