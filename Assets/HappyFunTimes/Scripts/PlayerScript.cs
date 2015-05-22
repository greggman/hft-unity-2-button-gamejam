using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;
using CSSParse;

namespace HappyFunTimesExample {

class PlayerScript : MonoBehaviour {
    private class MessageColor : MessageCmdData {
        public MessageColor(Color _color) {
            color = _color;
        }
        public Color color;
    };

    private class MessageButton : MessageCmdData {
        public int id = 0;
        public bool pressed = false;
    };

    void Init() {
        if (m_renderer == null) {
            m_renderer = gameObject.GetComponent<Renderer>();
            m_position = gameObject.transform.localPosition;
        }
    }

    void InitializeNetPlayer(SpawnInfo spawnInfo) {
        Init();

        // Save the netplayer object so we can use it send messages to the phone
        m_netPlayer = spawnInfo.netPlayer;

        // Register handler to call if the player disconnects from the game.
        m_netPlayer.OnDisconnect += Remove;
        m_netPlayer.OnNameChange += ChangeName;

        // Setup events for the different messages.
        m_netPlayer.RegisterCmdHandler<MessageButton>("button", OnButton);

        GameSettings settings = GameSettings.settings();
        m_position = new Vector3(Random.Range(0.0f, settings.areaWidth), 0, Random.Range(0.0f, settings.areaHeight));
        transform.localPosition = m_position;

        SetName(m_netPlayer.Name);

        // NOTE: the problem with picking a random color
        // is we get similar colors quite often. It
        // might be better to pick a random starting color
        // and then offset at least 20 degrees each time
        // or some other heuristic to try to not have like
        // colors
        float hue = Random.value;
        float saturation = 0.4f;
        float value = 1.0f;
        float alpha = 1.0f;
        SetColor(ColorUtils.HSVAToColor(new Vector4(hue, saturation, value, alpha)));

        // Send the color to the controller.
        m_netPlayer.SendCmd("color", new MessageColor(m_color));
    }

    void Start() {
        Init();
    }

    float Wrap(float value, float max, float edgeSize) {
      if (value < -edgeSize) {
        value = max + edgeSize - 1;
      } else if (value > max + edgeSize) {
        value = -edgeSize + 1;
      }
      return value;
    }

    void Update() {
        GameSettings settings = GameSettings.settings();
        float speed = settings.playerSpeed * Time.deltaTime;
        m_position.x += m_buttonState[0] ? speed : -speed;
        m_position.z += m_buttonState[1] ? speed : -speed;

        float fudge = 10;
        m_position.x = Wrap(m_position.x, settings.areaWidth, fudge);
        m_position.z = Wrap(m_position.z, settings.areaHeight, fudge);

        gameObject.transform.localPosition = m_position;
    }

    void OnGUI()
    {
        Vector2 size = m_guiStyle.CalcSize(m_guiName);
        Vector3 coords = Camera.main.WorldToScreenPoint(transform.position);
        m_nameRect.x = coords.x - size.x * 0.5f - 5.0f;
        m_nameRect.y = Screen.height - coords.y - 30.0f;
        GUI.Box(m_nameRect, m_name, m_guiStyle);
    }

    void SetName(string name) {
        m_name = name;
        gameObject.name = "Player-" + m_name;
        m_guiName = new GUIContent(m_name);
        m_guiStyle.normal.textColor = Color.black;
        m_guiStyle.contentOffset = new Vector2(4.0f, 2.0f);
        Vector2 size = m_guiStyle.CalcSize(m_guiName);
        m_nameRect.width  = size.x + 12;
        m_nameRect.height = size.y + 5;
    }

    void SetColor(Color color)
    {
        m_color = color;
        m_renderer.material.color = m_color;
        Color[] pix = new Color[1];
        pix[0] = color;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixels(pix);
        tex.Apply();
        m_guiStyle.normal.background = tex;
    }

    public void OnTriggerEnter(Collider other) {
        // Because of physics layers we can only collide with the goal
        // We might give the player a point here.
        // The goal takes care if itself.
    }

    private void Remove(object sender, System.EventArgs e) {
        Destroy(gameObject);
    }

    private void OnButton(MessageButton data) {
        m_buttonState[data.id] = data.pressed;
    }

    private void ChangeName(object sender, System.EventArgs e) {
        SetName(m_netPlayer.Name);
    }

    private NetPlayer m_netPlayer;
    private Renderer m_renderer;
    private Vector3 m_position;
    private bool[] m_buttonState = new bool[2];
    private Color m_color;
    private string m_name;
    private GUIStyle m_guiStyle = new GUIStyle();
    private GUIContent m_guiName = new GUIContent("");
    private Rect m_nameRect = new Rect(0,0,0,0);
}

}  // namespace HappyFunTimesExample

