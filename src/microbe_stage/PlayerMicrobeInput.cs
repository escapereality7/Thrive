﻿using System;
using Godot;

/// <summary>
///   Handles key input in the microbe stage
/// </summary>
public class PlayerMicrobeInput : NodeWithInput
{
    private bool autoMove;

    /// <summary>
    ///   A reference to the stage is kept to get to the player object
    ///   and also the cloud spawning.
    /// </summary>
    private MicrobeStage stage;

    public override void _Ready()
    {
        // Not the cleanest that the parent has to be MicrobeState type...
        stage = (MicrobeStage)GetParent();
    }

    [RunOnKeyDown("g_hold_forward")]
    public void ToggleAutoMove()
    {
        autoMove = !autoMove;
    }

    [RunOnAxis(new[] { "g_move_forward", "g_move_backwards" }, new[] { -1.0f, 1.0f })]
    [RunOnAxis(new[] { "g_move_left", "g_move_right" }, new[] { -1.0f, 1.0f })]
    [RunOnAxisGroup(InvokeAlsoWithNoInput = true)]
    public void OnMovement(float delta, float forwardMovement, float leftRightMovement)
    {
        _ = delta;
        const float epsilon = 0.01f;

        // Reset auto move if a key was pressed
        if (Math.Abs(forwardMovement) + Math.Abs(leftRightMovement) > epsilon)
        {
            autoMove = false;
        }

        if (stage.Player != null)
        {
            if (stage.Player.MicrobeMode == Microbe.MicrobeAction.UNBINDING)
                return;

            var movement = new Vector3(leftRightMovement, 0, forwardMovement);

            stage.Player.MovementDirection = autoMove ? new Vector3(0, 0, -1) : movement.Normalized();

            stage.Player.LookAtPoint = stage.Camera.CursorWorldPos;
        }
    }

    [RunOnKeyDown("g_fire_toxin")]
    public void EmitToxin()
    {
        stage.Player?.EmitToxin();
    }

    [RunOnKeyDown("g_toggle_engulf")]
    public void ToggleEngulf()
    {
        if (stage.Player == null)
            return;

        if (stage.Player.MicrobeMode == Microbe.MicrobeAction.ENGULF)
            stage.Player.MicrobeMode = Microbe.MicrobeAction.NONE;
        else if (!stage.Player.Membrane.Type.CellWall)
            stage.Player.MicrobeMode = Microbe.MicrobeAction.ENGULF;
    }

    [RunOnKeyDown("g_toggle_binding")]
    public void ToggleBinding()
    {
        if (stage.Player == null)
            return;

        if (stage.Player.MicrobeMode == Microbe.MicrobeAction.BINDING)
            stage.Player.MicrobeMode = Microbe.MicrobeAction.NONE;
        else if (stage.Player.CanBind())
            stage.Player.MicrobeMode = Microbe.MicrobeAction.BINDING;
    }

    [RunOnKeyDown("g_toggle_unbinding")]
    public void ToggleUnbinding()
    {
        if (stage.Player == null)
            return;

        if (stage.Player.MicrobeMode == Microbe.MicrobeAction.UNBINDING)
            stage.Player.MicrobeMode = Microbe.MicrobeAction.NONE;
        else if (stage.Player.Colony != null)
            stage.Player.MicrobeMode = Microbe.MicrobeAction.UNBINDING;
    }

    [RunOnKeyDown("g_perform_unbinding", Priority = 1)]
    public bool AcceptUnbind()
    {
        if (stage.Player == null)
            return false;
        if (stage.Player.MicrobeMode != Microbe.MicrobeAction.UNBINDING)
            return false;
        if (stage.MicrobesAtMouse.Count == 0)
            return false;

        var target = stage.MicrobesAtMouse[0];

        target.Colony.RemoveFromColony();
        target.MicrobeMode = Microbe.MicrobeAction.NONE;

        return true;
    }

    [RunOnKeyDown("g_cheat_editor")]
    public void CheatEditor()
    {
        if (Settings.Instance.CheatsEnabled)
        {
            stage.HUD.ShowReproductionDialog();
        }
    }

    [RunOnKey("g_cheat_glucose")]
    public void CheatGlucose(float delta)
    {
        if (Settings.Instance.CheatsEnabled)
        {
            SpawnCheatCloud("glucose", delta);
        }
    }

    [RunOnKey("g_cheat_ammonia")]
    public void CheatAmmonia(float delta)
    {
        if (Settings.Instance.CheatsEnabled)
        {
            SpawnCheatCloud("ammonia", delta);
        }
    }

    [RunOnKey("g_cheat_phosphates")]
    public void CheatPhosphates(float delta)
    {
        if (Settings.Instance.CheatsEnabled)
        {
            SpawnCheatCloud("phosphates", delta);
        }
    }

    private void SpawnCheatCloud(string name, float delta)
    {
        stage.Clouds.AddCloud(SimulationParameters.Instance.GetCompound(name),
            8000.0f * delta, stage.Camera.CursorWorldPos);
    }
}
