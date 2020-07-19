using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilderGrabPlayer : UIElement
{
    private GameObject grabTarget;
    private ISnapper snapper;
    public GameObject GrabTarget
    {
        get => grabTarget;
        set
        {
            snapper = null;
            grabTarget = value;
        }
    }

    private Vector2 cameraAngles;
    private EditSpace movementSpace = EditSpace.grabTarget;
    private EditSpace rotationSpace = EditSpace.grabTarget;

    private enum EditSpace
    {
        camera,
        grabTarget,
        world
    }

    private enum EditMode
    {
        Move,
        Rotate
    }

    public void PushOnStack(GameObject grabTarget, UIStack stack)
    {
        this.GrabTarget = grabTarget;
        var cameraEuler = transform.rotation.eulerAngles;
        cameraAngles = Vector2.zero;
        cameraAngles.x = cameraEuler.y;
        cameraAngles.y = -cameraEuler.x;

        stack.Push(this);
    }



    public override bool OnEvent(object sender, CEvent e)
    {
        if (e is GamepadInputEvent inputEvent)
        {
            UpdateInput(inputEvent.gamepad);
        }

        return true;
    }

    public void UpdateInput(GamepadPoll poll)
    {
        Vector3 newTargetPosition;
        Quaternion newTargetRotation;
        Quaternion newCameraRotation;

        var editMode = GetEditoMode(poll);
        switch (editMode)
        {
            case EditMode.Move:
                (newTargetPosition, newTargetRotation, newCameraRotation) = UpdateMoveMode(poll);
                break;
            case EditMode.Rotate:
                (newTargetPosition, newTargetRotation, newCameraRotation) = UpdateRotateMode(poll);
                break;
            default:
                throw new InvalidOperationException($"Unkown edit mode {editMode}");
        }

        if (newTargetPosition != grabTarget.transform.position)
        {
            snapper = null;
        }
        if(newTargetRotation != grabTarget.transform.rotation)
        {
            snapper = null;
        }

        grabTarget.transform.position = newTargetPosition;
        grabTarget.transform.rotation = newTargetRotation;

        transform.rotation = newCameraRotation;
        transform.position = grabTarget.transform.position - transform.forward * 10f;



        if (poll.GetButtonDown(Gamepad.Button.A))
        {
            Pop(false);
        }
        if (poll.GetButtonDown(Gamepad.Button.B))
        {
            Destroy(grabTarget);
            Pop(false);
        }
        if (poll.GetButtonDown(Gamepad.Button.RightStickButton))
        {
            if (snapper == null)
            {
                SnappingSetOwner snappingSetOwner = grabTarget.GetComponent<SnappingSetOwner>();
                if (snappingSetOwner != null)
                {
                    snapper = new AutoSnapper(snappingSetOwner);
                    snapper.Snap(grabTarget, Snapper.SnapMode.both);
                }
            }
            else
            {
                snapper.Itterate();
                snapper.Snap(grabTarget, Snapper.SnapMode.both);
            }
        }
        if (poll.GetButtonDown(Gamepad.Button.LeftStickButton))
        {
            grabTarget.transform.rotation = Quaternion.identity;
        }
    }

    private (Vector3 targetPosition, Quaternion targetRotation, Quaternion cameraRotation) UpdateMoveMode(GamepadPoll poll)
    {
        cameraAngles += GetInputCameraRotationVector(poll) * Time.deltaTime;

        Quaternion cameraRot = Quaternion.Euler(-cameraAngles.y, cameraAngles.x, 0f);

        Quaternion positionRot = GetEditSpaceRotation(cameraRot, movementSpace);

        Vector3 positionInput = GetInputPositionVector(poll);

        if (poll.GetButtonDown(Gamepad.Button.Back))
        {
            movementSpace = GetNextEditSpace(movementSpace);
        }

        return
            (
            grabTarget.transform.position + positionRot * positionInput * 10f * Time.deltaTime,
            grabTarget.transform.rotation,
            cameraRot
            );
    }

    private (Vector3 targetPosition, Quaternion targetRotation, Quaternion cameraRotation) UpdateRotateMode(GamepadPoll poll)
    {
        Vector3 euler = GetInputTargetRotationVector(poll);
        Quaternion cameraRot = Quaternion.Euler(-cameraAngles.y, cameraAngles.x, 0f);
        cameraRot = GetEditSpaceRotation(cameraRot, rotationSpace);
        Quaternion deltaRot = Quaternion.Euler(euler * Time.deltaTime * 90f);
        deltaRot = cameraRot * deltaRot * Quaternion.Inverse(cameraRot);

        if (poll.GetButtonDown(Gamepad.Button.Back))
        {
            rotationSpace = GetNextEditSpace(rotationSpace);
        }

        return (grabTarget.transform.position, deltaRot * grabTarget.transform.rotation, transform.rotation);
    }

    private Quaternion GetEditSpaceRotation(Quaternion cameraRot, EditSpace mode)
    {
        switch (mode)
        {
            case EditSpace.grabTarget:
                return Quaternion.Euler(0f, cameraAngles.x, 0f);
            case EditSpace.camera:
                return cameraRot;
            case EditSpace.world:
                return Quaternion.identity;
            default:
                throw new ArgumentException($"{mode} is not a known movement mode");
        }
    }

    private static Vector3 GetInputPositionVector(GamepadPoll poll)
    {
        Vector3 positionInput = Vector3.zero;
        positionInput.x += poll.LeftStick.x;
        positionInput.z += poll.LeftStick.y;
        positionInput.y += poll.RightTrigger;
        positionInput.y -= poll.LeftTrigger;
        return positionInput;
    }

    private Vector2 GetInputCameraRotationVector(GamepadPoll poll)
    {
        return poll.RightStick * 90f;
    }

    private Vector3 GetInputTargetRotationVector(GamepadPoll poll)
    {
        Vector3 output = Vector3.zero;
        output.x -= poll.RightStick.y;
        output.y += poll.RightStick.x;
        output.z += poll.RightTrigger;
        output.z -= poll.LeftTrigger;
        return output;
    }


    private EditMode GetEditoMode(GamepadPoll poll)
    {
        if (poll.GetButton(Gamepad.Button.LeftShoulder))
        {
            return EditMode.Rotate;
        }
        else
        {
            return EditMode.Move;
        }
    }

    public override void OnEventInactive(object sender, CEvent e)
    {

    }

    private static EditSpace GetNextEditSpace(EditSpace space)
    {
        switch (space)
        {
            case EditSpace.camera:
                return EditSpace.grabTarget;
            case EditSpace.grabTarget:
                return EditSpace.world;
            case EditSpace.world:
                return EditSpace.camera;
            default:
                throw new ArgumentException($"Unkown Edit Space {space}");
        }
    }
}
