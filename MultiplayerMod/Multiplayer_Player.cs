using UnityEngine;
using System.Collections;

public class Multiplayer_Player : MonoBehaviour {
    int id;

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
