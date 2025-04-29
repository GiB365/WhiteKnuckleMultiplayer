using UnityEngine;
using System.Collections;

public class Multiplayer_DebugPlayer : Multiplayer_Player {
    public void FixedUpdate() {
        transform.position += new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
        transform.eulerAngles += new Vector3(0, Random.Range(-5f, 5f), 0);
    }
}
