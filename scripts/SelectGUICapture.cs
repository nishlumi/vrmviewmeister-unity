using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SelectGUICapture : MonoBehaviour
{
    [SerializeField, Tooltip("GUI�������_�����O���Ă���J����")]
    private Camera _guiCamera = null;

    [SerializeField, Tooltip("�L���v�`������^�C�~���O")]
    private CameraEvent _cameraEvent = CameraEvent.BeforeImageEffects;

    [SerializeField, Tooltip("�������ɖ��������UI�̃��C���[")]
    private LayerMask _captureTargetLayer = -1;

    private Camera _mainCamera = null;
    private RenderTexture _buf = null;
    private CommandBuffer _commandBuffer = null;

    #region ### MonoBehaviour ###
    private void Awake()
    {
        CreateBuffer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            TakeScreenshot();
        }
    }

    /// <summary>
    /// ����m�F�p��Gizmo�Ńe�N�X�`����\������
    /// </summary>
    private void OnGUI()
    {
        if (_buf == null) return;
        GUI.DrawTexture(new Rect(5f, 5f, Screen.width * 0.5f, Screen.height * 0.5f), _buf);
    }
    #endregion ### MonoBehaviour ###

    /// <summary>
    /// �o�b�t�@�𐶐�����
    /// </summary>
    private void CreateBuffer()
    {
        _buf = new RenderTexture(Screen.width, Screen.height, 0);

        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "CaptureScene";
        _commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _buf);
    }

    /// <summary>
    /// �X�N���[���V���b�g���B�e����
    /// </summary>
    public void TakeScreenshot()
    {
        AddCommandBuffer();

        StartCoroutine(WaitCapture());
    }

    /// <summary>
    /// �R�}���h�o�b�t�@�̏�����҂�
    /// </summary>
    private IEnumerator WaitCapture()
    {
        yield return new WaitForEndOfFrame();

        BlendGUI();

        RemoveCommandBuffer();
    }

    /// <summary>
    /// ���C���J�����ɃR�}���h�o�b�t�@��ǉ�����
    /// </summary>
    private void AddCommandBuffer()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        _mainCamera.AddCommandBuffer(_cameraEvent, _commandBuffer);
    }

    /// <summary>
    /// ���C���J��������R�}���h�o�b�t�@���폜����
    /// </summary>
    private void RemoveCommandBuffer()
    {
        if (_mainCamera == null)
        {
            return;
        }

        _mainCamera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
    }

    /// <summary>
    /// GUI�v�f���u�����h����
    /// </summary>
    private void BlendGUI()
    {
        _guiCamera.targetTexture = _buf;

        int tmp = _guiCamera.cullingMask;
        _guiCamera.cullingMask = _captureTargetLayer;

        _guiCamera.Render();

        _guiCamera.cullingMask = tmp;

        _guiCamera.targetTexture = null;
    }
}