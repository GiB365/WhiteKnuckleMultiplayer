using UnityEngine;
using System.Collections;

public class Multiplayer_Player : MonoBehaviour {
    int id;

    public void UpdatePosition(int newX, int newY, int newZ) {
        transform.position.x = newX;
        transform.position.y = newY;
        transform.position.z = newZ;
    }
}
