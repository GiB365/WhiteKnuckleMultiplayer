using UnityEngine;
using System.Collections;

namespace WK_Multiplayer_Mod.MonoBehaviours;

public class Multiplayer_Player : MonoBehaviour {
    public void UpdatePosition(Vector3 new_position) {
        CommandConsole.Log(new_position.x.ToString());
        CommandConsole.Log(new_position.y.ToString());
        CommandConsole.Log(new_position.z.ToString());

        transform.position = new_position;
    }

    public void UpdateRotation(Vector3 new_rotation) {
        transform.eulerAngles = new_rotation;
    }
}
