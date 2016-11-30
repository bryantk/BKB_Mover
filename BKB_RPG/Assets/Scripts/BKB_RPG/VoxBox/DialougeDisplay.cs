using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using BKB_RPG;

namespace BKB_TEXT
{
    public class DialougeDisplay : MonoBehaviour
    {
        public GameObject windowPrefab;
        public UIWindow mainWindow;
        public UIWindow nameWindow;

        private const int MAX_CHOICES = 8;
        private const float MinReadTime = 0.25f;

        private float _nextTime;
        private float _oldVerticalRate;
        private int _selectedChoice;
        private VoxData _currentMessageData;
        private MessageLocation _messagePosition;
        private List<DialougeSubWindow> _subWindows;

        private static DialougeDisplay _instance;

        void Awake()
        {
            if (_instance != null)
                Destroy(this);
            else
                _instance = this;
            VoxBox.onEnter += OnVoxEnable;
            VoxBox.onBuildWindow += OnBuildWindow;
            VoxBox.onTextUpdate += OnTextUpdate;
            VoxBox.onWindowTeardown += OnWindowTearDown;
            VoxBox.onExit += OnExit;
            _subWindows = new List<DialougeSubWindow>();
            for (int i = 0; i < MAX_CHOICES; i++)
            {
                GameObject go = Instantiate(windowPrefab);
                go.transform.SetParent(transform, false);
                go.name = "Choice_" + i;
                _subWindows.Add(go.GetComponent<DialougeSubWindow>());
                _subWindows[i].Active = false;
            }
            HideAll();
        }

        public static void EnterState()
        {
            InputMaster.okButtonEvent += _instance.OKButton;
            InputMaster.cancleButtonEvent += _instance.CancleButton;
            InputMaster.moveEvent += _instance.InputMaster_moveEvent;
            _instance._oldVerticalRate = InputMaster.verticalRepeater.TriggerRate;
            InputMaster.verticalRepeater.TriggerRate = 0.25f;
        }

        public static void ExitState()
        {
            InputMaster.okButtonEvent -= _instance.OKButton;
            InputMaster.cancleButtonEvent -= _instance.CancleButton;
            InputMaster.moveEvent -= _instance.InputMaster_moveEvent;
            InputMaster.verticalRepeater.TriggerRate = _instance._oldVerticalRate;
        }

        private void InputMaster_moveEvent(object sender, InfoEventArgs<Vector2> e)
        {
            if (!_currentMessageData.hasChoices)
                return;
            int temp = _selectedChoice;
            _selectedChoice = Mathf.Clamp(_selectedChoice - (int) e.info.y, 0, _currentMessageData.choices.Count - 1);
            if (temp != _selectedChoice)
            {
                if (temp != -1)
                    _subWindows[temp].UnHighlight();
                _subWindows[_selectedChoice].Highlight();
            }
        }

        void HideAll()
        {
            mainWindow.root.gameObject.SetActive(false);
            nameWindow.root.gameObject.SetActive(false);
        }

        void CancleButton()
        {
            if (_currentMessageData.hasChoices)
                Continue();
        }


        void OKButton()
        {
            if (Time.time >= _nextTime)
            {
                if (_currentMessageData.hasChoices && _selectedChoice == -1)
                {
                    return;
                }
                Continue();
            }
        }

        void Continue()
        {
            foreach (var w in _subWindows)
            {
                w.UnHighlight();
                w.Active = false;
            }
            GameVariables.SetFloat("MessageChoice", _selectedChoice);
            VoxBox.ContinueMessages();
        }


        void OnVoxEnable()
        {
            BKB_FSM.StateManager.Push("Vox");
            _nextTime = Time.time + MinReadTime;
        }

        IEnumerator OnBuildWindow(object d)
        {
            mainWindow.SetText("");
            nameWindow.SetText("");
            _currentMessageData = (d as VoxData);
            _messagePosition = _currentMessageData.position;
            if (_messagePosition == MessageLocation.Auto)
            {
                Vector3 playerPosition = GameMaster._instance.playerData.gameObject.transform.position;
                playerPosition = Camera.main.WorldToScreenPoint(playerPosition);
                _messagePosition = playerPosition.y < 250 ? MessageLocation.Top : MessageLocation.Bottom;
            }
            switch (_messagePosition)
            {
            case MessageLocation.Top:
                mainWindow.Position = Vector3.zero;
                break;
            case MessageLocation.Middle:
                mainWindow.Position = Vector3.down * 175;
                break;
            case MessageLocation.Bottom:
                mainWindow.Position = Vector3.down * 367;
                break;
            }
            mainWindow.Transparent = nameWindow.Transparent = _currentMessageData.useTexture;
            mainWindow.Active = true;
            yield return null;
        }

        void OnTextUpdate(object d)
        {
            _nextTime = Time.time + MinReadTime;

            _currentMessageData = d as VoxData;
            SetNameWindow();
            mainWindow.SetText(_currentMessageData.message);
            _selectedChoice = -1;
            if (_currentMessageData.choices.Count > 0)
            {
                _selectedChoice = _currentMessageData.defaultChoice;
                for (int i = 0; i < _currentMessageData.choices.Count; i++)
                {
                    DialougeSubWindow choice = _subWindows[i];
                    choice.Active = true;
                    choice.SetText(_currentMessageData.choices[i]);
                    Vector2 position = Vector2.zero;
                    if (_currentMessageData.position == MessageLocation.Top)
                        position.y = -140 - 50 * i;
                    else
                        position.y = mainWindow.Position.y + 50 * (_currentMessageData.choices.Count - i);

                    position.x = 490;
                    if (!string.IsNullOrEmpty(_currentMessageData.name) &&
                        _currentMessageData.nameLocation == HorizontalAlignment.Right)
                    {
                        position.x = 0;
                    }
                    choice.Position = position;
                    if (i == _selectedChoice)
                        choice.Highlight();
                }
            }
        }

        IEnumerator OnWindowTearDown(bool instant)
        {
            if (_currentMessageData == null || !_currentMessageData.noTearDown)
                HideAll();
            _currentMessageData = null;
            yield return null;
        }

        void OnExit()
        {
            BKB_FSM.StateManager.Pop();
        }


        public static void CloseMessages()
        {
            if (_instance.mainWindow.Active)
                VoxBox.EndMessages();
        }

        // Helpers

        private void SetNameWindow()
        {
            Vector2 namePosition = Vector2.zero;
            switch (_messagePosition)
            {
            case MessageLocation.Top:
                namePosition.y = -142;
                break;
            case MessageLocation.Middle:
                namePosition.y = -134;
                break;
            case MessageLocation.Bottom:
                namePosition.y = -324;
                break;
            }
            switch (_currentMessageData.nameLocation)
            {
            case HorizontalAlignment.Left:
                namePosition.x = 8;
                break;
            case HorizontalAlignment.Center:
                namePosition.x = 220;
                break;
            case HorizontalAlignment.Right:
                namePosition.x = 484;
                break;
            }
            nameWindow.Position = namePosition;
            nameWindow.Active = !string.IsNullOrEmpty(_currentMessageData.name);
            nameWindow.SetText(_currentMessageData.name);
        }

    }
}