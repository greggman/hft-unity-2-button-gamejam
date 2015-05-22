using UnityEngine;

namespace HappyFunTimesExample {

// A Singlton-ish class for some global settings.
// PS: I know Singltons suck but dependency injection is hard
// in Unity.
public class GameSettings : MonoBehaviour {

    public int areaWidth = 300;
    public int areaHeight = 300;
    public float playerSpeed = 64;

    public static GameSettings settings() {
        return s_settings;
    }

    static private GameSettings s_settings;

    void Awake() {
        if (s_settings != null) {
            throw new System.InvalidProgramException("there is more than one game settings object!");
        }
        s_settings = this;
    }

    void Cleanup() {
        s_settings = null;
    }

    void OnDestroy() {
        Cleanup();
    }

    void OnApplicationExit() {
        Cleanup();
    }
}

}  // namespace HappyFunTimesExample

