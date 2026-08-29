using UnityEngine;

namespace ScaryIslands.Multiplayer
{
    /// <summary>World-space multiplayer status board with methods ready for XR button binding.</summary>
    public sealed class MultiplayerTerminal : MonoBehaviour
    {
        private TextMesh display;

        private void Start()
        {
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Multiplayer Board";
            board.transform.SetParent(transform, false);
            board.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            board.transform.localScale = new Vector3(2.2f, 1.4f, 0.18f);

            GameObject label = new GameObject("Multiplayer Text");
            label.transform.SetParent(transform, false);
            label.transform.localPosition = new Vector3(-0.95f, 1.55f, -0.11f);
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            display = label.AddComponent<TextMesh>();
            display.fontSize = 44;
            display.characterSize = 0.05f;
            display.anchor = TextAnchor.UpperLeft;
            display.alignment = TextAlignment.Left;
            display.color = new Color(0.8f, 0.93f, 0.9f);
        }

        private void Update()
        {
            if (display == null) return;
            MultiplayerSession session = MultiplayerSession.Instance;
            display.text = session == null
                ? "MULTIPLAYER\nINITIALIZING..."
                : "MULTIPLAYER\n" +
                  session.Status + "\n\n" +
                  "HOST or JOIN\n" +
                  "UDP " + session.Port + "\n" +
                  "Direct IP / LAN";
        }

        public void Host()
        {
            MultiplayerSession.Instance?.Host();
        }

        public void JoinConfiguredAddress()
        {
            MultiplayerSession.Instance?.Join();
        }

        public void JoinLocalhost()
        {
            MultiplayerSession.Instance?.JoinLocalhost();
        }

        public void Disconnect()
        {
            MultiplayerSession.Instance?.Disconnect();
        }
    }
}
