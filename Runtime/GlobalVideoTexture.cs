using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream.Modules.GlobalVideoTexture
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class GlobalVideoTexture : YamaPlayerModule
  {
    private const string GLOBAL_VIDEO_TEXTURE = "_Udon_VideoTex";
    private const string GLOBAL_VIDEO_DATA = "_Udon_VideoData";

    private readonly Vector4 _defaultTextureST = new Vector4(1f, 1f, 0f, 0f);
    private readonly Vector4 _disabledTexelSize = new Vector4(1f / 16f, 1f / 16f, 16f, 16f);

    [SerializeField] private bool _globalTextureEnabled = true;

    private int _globalVideoTextureId;
    private int _globalVideoTextureSTId;
    private int _globalVideoTextureTexelSizeId;
    private int _globalVideoDataId;
    private bool _shaderPropertiesInitialized;
    private Texture _currentTexture;

    public bool GlobalTextureEnabled => _globalTextureEnabled;

    public override void Start()
    {
      base.Start();
      InitializeShaderProperties();

      if (Utilities.IsValid(_controller))
      {
        _currentTexture = _controller.Texture;
      }

      if (_globalTextureEnabled)
      {
        PublishTexture(_currentTexture);
        PublishVideoData();
      }
      else
      {
        ClearGlobalProperties();
      }
    }

    private void Update()
    {
      if (!_globalTextureEnabled || !Utilities.IsValid(_controller)) return;
      PublishVideoData();
    }

    public override void AfterTextureUpdated(Texture texture)
    {
      _currentTexture = texture;
      if (!_globalTextureEnabled) return;

      PublishTexture(texture);
      PublishVideoData();
    }

    public override void AfterVideoStopped()
    {
      _currentTexture = null;
      if (!_globalTextureEnabled) return;

      PublishTexture(null);
      PublishVideoData();
    }

    public void _EnableGlobalTexture()
    {
      if (_globalTextureEnabled) return;

      _globalTextureEnabled = true;
      if (Utilities.IsValid(_controller))
      {
        _currentTexture = _controller.Texture;
      }

      PublishTexture(_currentTexture);
      PublishVideoData();
    }

    public void _DisableGlobalTexture()
    {
      if (!_globalTextureEnabled) return;

      _globalTextureEnabled = false;
      ClearGlobalProperties();
    }

    public void _ToggleGlobalTexture()
    {
      if (_globalTextureEnabled) _DisableGlobalTexture();
      else _EnableGlobalTexture();
    }

    // Backward-compatible ProTV action aliases.
    public void _EnableGSV() => _EnableGlobalTexture();
    public void _DisableGSV() => _DisableGlobalTexture();
    public void _ToggleGSV() => _ToggleGlobalTexture();

    public void OnDisable()
    {
      if (_globalTextureEnabled) ClearGlobalProperties();
    }

    private void InitializeShaderProperties()
    {
      if (_shaderPropertiesInitialized) return;

      _globalVideoTextureId = VRCShader.PropertyToID(GLOBAL_VIDEO_TEXTURE);
      _globalVideoTextureSTId = VRCShader.PropertyToID(GLOBAL_VIDEO_TEXTURE + "_ST");
      _globalVideoTextureTexelSizeId = VRCShader.PropertyToID(GLOBAL_VIDEO_TEXTURE + "_TexelSize");
      _globalVideoDataId = VRCShader.PropertyToID(GLOBAL_VIDEO_DATA);
      _shaderPropertiesInitialized = true;
    }

    private void PublishTexture(Texture texture)
    {
      InitializeShaderProperties();

      if (!Utilities.IsValid(texture))
      {
        VRCShader.SetGlobalTexture(_globalVideoTextureId, null);
        VRCShader.SetGlobalVector(_globalVideoTextureSTId, _defaultTextureST);
        VRCShader.SetGlobalVector(_globalVideoTextureTexelSizeId, _disabledTexelSize);
        return;
      }

      int width = texture.width;
      int height = texture.height;
      if (width <= 0 || height <= 0)
      {
        VRCShader.SetGlobalTexture(_globalVideoTextureId, null);
        VRCShader.SetGlobalVector(_globalVideoTextureSTId, _defaultTextureST);
        VRCShader.SetGlobalVector(_globalVideoTextureTexelSizeId, _disabledTexelSize);
        return;
      }

      VRCShader.SetGlobalTexture(_globalVideoTextureId, texture);
      VRCShader.SetGlobalVector(_globalVideoTextureSTId, _defaultTextureST);
      VRCShader.SetGlobalVector(
        _globalVideoTextureTexelSizeId,
        new Vector4(1f / width, 1f / height, width, height));
    }

    private void PublishVideoData()
    {
      InitializeShaderProperties();
      if (!Utilities.IsValid(_controller))
      {
        VRCShader.SetGlobalMatrix(_globalVideoDataId, Matrix4x4.zero);
        return;
      }

      int flags = 0;
      flags |= _controller.Mute ? 1 << 1 : 0;
      flags |= _controller.IsLive ? 1 << 2 : 0;
      flags |= _controller.IsLoading ? 1 << 3 : 0;

      // ProTV-compatible state values: WAITING=0, STOPPED=1, PLAYING=2, PAUSED=3.
      int state = 1;
      if (_controller.IsPlaying || _controller.IsLoading) state = 2;
      else if (_controller.Paused) state = 3;

      // YamaPlayer currently exposes a single error flag, so map it to FAILED=3.
      int errorState = _controller.IsError ? 3 : 0;

      float seekPercent = 0f;
      float duration = _controller.Duration;
      if (!_controller.IsLive && duration > 0f)
      {
        seekPercent = Mathf.Clamp01(_controller.VideoTime / duration);
      }

      Matrix4x4 videoData = Matrix4x4.zero;
      videoData.m00 = flags;
      videoData.m01 = state;
      videoData.m02 = errorState;
      videoData.m03 = 1f;
      videoData.m10 = _controller.Volume;
      videoData.m11 = seekPercent;
      videoData.m12 = _controller.Speed;
      // m30 is the ProTV 3D mode. YamaPlayer currently publishes a 2D texture.
      videoData.m30 = 0f;

      VRCShader.SetGlobalMatrix(_globalVideoDataId, videoData);
    }

    private void ClearGlobalProperties()
    {
      InitializeShaderProperties();
      VRCShader.SetGlobalTexture(_globalVideoTextureId, null);
      VRCShader.SetGlobalVector(_globalVideoTextureSTId, _defaultTextureST);
      VRCShader.SetGlobalVector(_globalVideoTextureTexelSizeId, _disabledTexelSize);
      VRCShader.SetGlobalMatrix(_globalVideoDataId, Matrix4x4.zero);
    }
  }
}

