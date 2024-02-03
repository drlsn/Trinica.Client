﻿using Core.Auth;
using Core.Collections;
using Core.Unity.Storage;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace CardRPG.UI.Features.LoadingScreen
{
    public class SignInPanel : MonoBehaviour
    {
        [SerializeField] private EmailSignInPanel _emailSignInPanel;
        [SerializeField] private GameObject _idpSignInPanel;
        [SerializeField] private TMP_Text _msgText;
        [SerializeField] private Button _retryButton;

        [Inject] private IAuthentication _authentication;

        private async void Awake()
        {
            _idpSignInPanel.SetActive(false);
            _emailSignInPanel.gameObject.SetActive(false);
            _retryButton.gameObject.SetActive(false);

            await Authenticate();

            _retryButton.onClick.AddListener(async () =>
            {
                _msgText.text = "Loading..";
                _retryButton.gameObject.SetActive(false);
                await Authenticate();
            });

#if UNITY_EDITOR || UNITY_STANDALONE
            _emailSignInPanel.gameObject.SetActive(true);
            _idpSignInPanel.SetActive(false);
#elif UNITY_ANDROID
            _emailSignInPanel.gameObject.SetActive(false);
            _idpSignInPanel.SetActive(true);
#endif
        }

        private async Task Authenticate()
        {
            var tokenResult = await _authentication.GetAccessToken();
            if (!tokenResult.IsSuccess)
            {
                _msgText.text = tokenResult.Message;
#if UNITY_ANDROID && !UNITY_EDITOR
                _retryButton.gameObject.SetActive(true);
#endif
                return;
            }

            _emailSignInPanel.gameObject.SetActive(false);
            _idpSignInPanel.SetActive(false);

            var result = await _authentication.SignIn();
            if (result.IsSuccess)
            {
                _msgText.text = "Sign In Success";
                SceneManager.LoadScene("Menu");
            }
            else
                _msgText.text = result.Message;

            return;
        }
    }
}
