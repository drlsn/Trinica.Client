﻿using Core.Auth;
using Core.Basic;
using Core.Collections;
using Core.Net.Http;
using Core.Security;
using Core.Unity.Storage;
using Firebase.Auth;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Unity.Auth
{
    public class FirebaseEmailAuthentication : IAuthentication
    {
        private const string AccessTokenKey = "access-token";

        public string UserId => "";
        public string UserName => "";

        private string _accessToken;

        public string Email { set; get; }
        public string Password { set; get; }

        private readonly HttpClient _client;
        private readonly string _apiKey;

        public FirebaseEmailAuthentication(HttpClient client, string apiKey)
        {
            _client = client;
            _apiKey = apiKey;
        }

        public async Task<Result<string>> GetAccessToken()
        {
            var result = Result<string>.Success();

            if (Application.internetReachability == NetworkReachability.NotReachable)
                return "No internet connection";

            if (!_accessToken.IsNullOrEmpty() && Jwt.ValidateAndDecodeToken(_accessToken))
                return result.With(_accessToken);

            _accessToken = SecurePlayerPrefs.GetString(AccessTokenKey);
            if (!_accessToken.IsNullOrEmpty() && Jwt.ValidateAndDecodeToken(_accessToken))
                return result.With(_accessToken);

            SecurePlayerPrefs.DeleteKey(AccessTokenKey);
            if (IsNotValidCredentials())
                return result.Fail("Bad credentials");

            var signInResult = await SignIn();
            if (!signInResult.IsSuccess)
                return result.Fail(signInResult.Messages);

            _accessToken = await FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(forceRefresh: true);
            if (!_accessToken.IsNullOrEmpty())
                SecurePlayerPrefs.SetString(AccessTokenKey, _accessToken);

            return result.With(_accessToken);
        }

        static bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");

        private bool IsNotValidCredentials() =>
            Email.IsNullOrEmpty() || !IsValidEmail(Email) || Password.IsNullOrEmpty() || Password.Length < 6;

        public async Task<Result> SignIn()
        {
            if (!_accessToken.IsNullOrEmpty() && Jwt.ValidateAndDecodeToken(_accessToken))
                return Result.Success();

            if (IsNotValidCredentials())
                return Result.Failure("Invalid credentials");

            try {
                var signUpResult = await _client.PostAsJsonExpectError<SignInRequest, SignInResponse, SignInResponseErrorTopmost>(
                    resourcePath: $"/v1/accounts:signUp?key={_apiKey}",
                    body: new(Email, Password),
                    ct: default);

                if (signUpResult.Error.Error.Errors.IsNullOrEmpty())
                {
                    return SetAccessToken(signUpResult.Body.IdToken);
                }

                var signInResult = await _client.PostAsJsonExpectError<SignInRequest, SignInResponse, SignInResponseErrorTopmost>(
                    resourcePath: $"/v1/accounts:signInWithPassword?key={_apiKey}",
                    body: new(Email, Password),
                    ct: default);

                if (signInResult.Error?.Error is not null && !signInResult.Error.Error.Errors.IsNullOrEmpty())
                    return Result.Failure(signInResult.Error.Error.Message);

                if (!signInResult.Response.StatusCode.IsSuccess())
                    return Result.Failure("Could not sign in");

                return SetAccessToken(signInResult.Body.IdToken);

                Result SetAccessToken(string token)
                {
                    _accessToken = token;
                    if (_accessToken.IsNullOrEmpty())
                        return Result.Failure("Id token is empty");

                    SecurePlayerPrefs.SetString(AccessTokenKey, _accessToken);

                    return Result.Success();
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        [Serializable]
        record SignInRequest(string email, string password, bool returnSecureToken = true);

        [Serializable]
        record SignInResponse(
            string Kind,
            string LocalId,
            string Email,
            string DisplayName,
            string IdToken,
            bool Registered,
            string RefreshToken,
            int ExpiresIn
        );

        public record SignInResponseErrorTopmost(SignInResponseErrorDetails Error);
        public record SignInResponseErrorDetails(int Code, string Message, SignInResponseError[] Errors);

        public record SignInResponseError(string Message, string Domain, string Reason);
    }
}
