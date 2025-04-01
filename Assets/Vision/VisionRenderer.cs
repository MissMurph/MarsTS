using System.Threading;
using UnityEngine;

namespace MarsTS.Vision
{
    public class VisionRenderer : MonoBehaviour
    {
        private static VisionRenderer _instance;

        private GameVision _vision;

        private Texture2D _render;

        [SerializeField] private Material fogMaterial;

        private Color[] _texture;

        private Thread _currentThread;

        private bool _running;

        private bool _dirty;
        private bool _doRender;

        [SerializeField] private float interpolation;

        private float _fixedDelta;

        private void Awake()
        {
            _instance = this;

            _vision = GetComponent<GameVision>();

            _render = new Texture2D(_vision.GridSize.x, _vision.GridSize.y);
            _render.filterMode = FilterMode.Point;

            _texture = new Color[_vision.GridSize.x * _vision.GridSize.y];

            _running = true;
            Application.quitting += Quitting;

            _dirty = false;
            _doRender = false;
        }

        private void Start()
        {
            fogMaterial.mainTexture = _render;

            ThreadStart workerThread = PrepareRender;

            _currentThread = new Thread(workerThread);
            _currentThread.Start();
        }

        private void Update()
        {
            if (_dirty)
            {
                _render.SetPixels(_texture);
                _render.Apply();
                _dirty = false;
            }
        }

        private void FixedUpdate()
        {
            _fixedDelta = Time.fixedDeltaTime;
            _doRender = true;
        }

        private void Quitting()
        {
            _running = false;
            _currentThread.Abort();
        }

        private void PrepareRender()
        {
            while (_running)
                if (_doRender)
                {
                    Render();
                    _doRender = false;
                }
        }

        private void Render()
        {
            for (int x = 0; x < _vision.GridSize.x; x++)
            for (int y = 0; y < _vision.GridSize.y; y++)
            {
                float redValue = 0f;

                if ((_vision.Nodes[x, y] & _vision.CurrentMask) == _vision.CurrentMask)
                    redValue = 1f;
                else if ((_vision.Visited[x, y] & _vision.CurrentMask) == _vision.CurrentMask) redValue = 0.5f;

                redValue = Mathf.LerpUnclamped(_texture[x + y * _vision.GridSize.x].r, redValue,
                    interpolation * _fixedDelta);

                _texture[x + y * _vision.GridSize.x] = new Color(redValue, 0, 0, 1f);
            }

            _dirty = true;
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}