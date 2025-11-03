using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AgoraIntegration.Core;

namespace AgoraIntegration.Demo
{
    /// <summary>
    /// Demo controller showcasing Agora integration
    /// Follows MVVM-like pattern with clear separation of concerns
    /// Demonstrates proper event handling and async operations
    /// </summary>
    public class AgoraDemoController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_InputField _appIdInput;
        [SerializeField] private TMP_InputField _channelInput;
        [SerializeField] private TMP_InputField _tokenInput;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _userCountText;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private Button _muteButton;
        [SerializeField] private Image _muteButtonIndicator;

        [Header("Visual Settings")]
        [SerializeField] private Color _activeColor = Color.green;
        [SerializeField] private Color _inactiveColor = Color.white;
        [SerializeField] private Color _mutedColor = Color.red;
        [SerializeField] private Color _errorColor = Color.yellow;

        private IAgoraService _agoraService;
        private int _userCount = 0;

        private void Start()
        {
            // Dependency resolution - could be replaced with DI container
            _agoraService = GetComponent<IAgoraService>() ?? FindObjectOfType<AgoraService>();

            if (_agoraService == null)
            {
                Debug.LogError("No IAgoraService implementation found!");
                return;
            }

            SetupUI();
            SubscribeToEvents();
            UpdateUIState(false);

            UpdateStatus("Ready to initialize Agora service");
        }

        private void SetupUI()
        {
            _joinButton.onClick.AddListener(JoinChannel);
            _leaveButton.onClick.AddListener(LeaveChannel);
            _muteButton.onClick.AddListener(ToggleMute);

            // Set placeholder values for demo
            _channelInput.text = "demo-channel";
            _appIdInput.text = "your_app_id_here";
            _tokenInput.text = "optional_token";

            _userCountText.text = "Users in channel: 0";
        }

        private void SubscribeToEvents()
        {
            _agoraService.OnLogMessage += OnLogMessage;
            _agoraService.OnUserJoined += OnUserJoined;
            _agoraService.OnUserLeft += OnUserLeft;
            _agoraService.OnConnectionStateChanged += OnConnectionStateChanged;
        }

        private async void JoinChannel()
        {
            var config = new ChannelConfig(
                channelName: _channelInput.text.Trim(),
                token: string.IsNullOrEmpty(_tokenInput.text) ? null : _tokenInput.text.Trim()
            );

            if (string.IsNullOrEmpty(config.ChannelName))
            {
                UpdateStatus("Please enter a channel name", true);
                return;
            }

            try
            {
                _agoraService.Initialize(_appIdInput.text.Trim());
                var result = await _agoraService.JoinChannelAsync(config);

                if (result.Success)
                {
                    UpdateUIState(true);
                    UpdateStatus($"Joined as user {result.UserId}");
                }
                else
                {
                    UpdateStatus($"Join failed: {result.ErrorMessage}", true);
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}", true);
            }
        }

        private async void LeaveChannel()
        {
            await _agoraService.LeaveChannelAsync();
            UpdateUIState(false);
            _userCount = 0;
            UpdateUserCount();
        }

        private async void ToggleMute()
        {
            await _agoraService.MuteAudioAsync(!_agoraService.IsAudioMuted);
            UpdateMuteButton();
        }

        private void OnUserJoined(uint userId)
        {
            _userCount++;
            UpdateUserCount();
            UpdateStatus($"User {userId} joined!");
        }

        private void OnUserLeft(uint userId)
        {
            _userCount = Mathf.Max(0, _userCount - 1);
            UpdateUserCount();
            UpdateStatus($"User {userId} left");
        }

        private void OnConnectionStateChanged(bool connected)
        {
            UpdateStatus(connected ? "Connected to voice channel!" : "Disconnected");
        }

        private void OnLogMessage(string message)
        {
            Debug.Log($"[Demo] {message}");
        }

        private void UpdateUIState(bool connected)
        {
            _joinButton.interactable = !connected;
            _leaveButton.interactable = connected;
            _muteButton.interactable = connected;

            _joinButton.image.color = connected ? _inactiveColor : _activeColor;
            _leaveButton.image.color = connected ? _activeColor : _inactiveColor;

            UpdateMuteButton();
        }

        private void UpdateMuteButton()
        {
            _muteButtonIndicator.color = _agoraService.IsAudioMuted ? _mutedColor : _activeColor;
        }

        private void UpdateUserCount()
        {
            _userCountText.text = $"Users in channel: {_userCount}";
        }

        private void UpdateStatus(string status, bool isError = false)
        {
            _statusText.text = status;
            _statusText.color = isError ? _errorColor : Color.white;
        }

        private void OnDestroy()
        {
            // Proper event unsubscription
            if (_agoraService != null)
            {
                _agoraService.OnLogMessage -= OnLogMessage;
                _agoraService.OnUserJoined -= OnUserJoined;
                _agoraService.OnUserLeft -= OnUserLeft;
                _agoraService.OnConnectionStateChanged -= OnConnectionStateChanged;
            }
        }
    }
}
