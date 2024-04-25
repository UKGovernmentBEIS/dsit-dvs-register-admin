﻿using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

public class CognitoClient
{
    public readonly string _userPoolId;
    public readonly string _clientId;
    public readonly string _region;
    public readonly AmazonCognitoIdentityProviderClient _provider;

    public CognitoClient(string userPoolId, string clientId, string region)
    {
        _userPoolId = userPoolId;
        _clientId = clientId;
        _region = region;

        // Initialize the Amazon Cognito Identity Provider client
        _provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), RegionEndpoint.EUWest2);
    }

    private async Task<string> GetAccessToken(string email, string password)
    {
        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _clientId,
            AuthParameters = new Dictionary<string, string>
                {
                    {"USERNAME", email},
                    {"PASSWORD", password}
                }
        };

        var authResponse = await _provider.InitiateAuthAsync(authRequest);

        return authResponse.AuthenticationResult.AccessToken;
    }

    public async Task<string> ForgotPassword(string email)
    {
        try
        {
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = email
            };
            var forgotPasswordResponse = await _provider.ForgotPasswordAsync(forgotPasswordRequest);
            return forgotPasswordResponse.HttpStatusCode.ToString();
        }
        catch(Exception e)
        {
            Console.WriteLine($"{e.Message}");
            return "KO";
        }
    }

    public async Task<string> ConfirmPasswordAndGenerateMFAToken(string email, string password, string oneTimePassCode)
    {
        var confirmForgotPasswordRequest = new ConfirmForgotPasswordRequest
        {
            ClientId = _clientId, 
            Username = email,
            Password = password,
            ConfirmationCode = oneTimePassCode 
        };


        // Confirm Password Request
        try
        {
            await _provider.ConfirmForgotPasswordAsync(confirmForgotPasswordRequest);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error confirming password : { ex.Message}");
            return "KO";
        }
        

        // Generate MFA Token Request
        try
        {
            var associateSoftwareTokenRequest = new AssociateSoftwareTokenRequest
            {
                AccessToken = await GetAccessToken(email, password)
            };
            var associateSoftwareTokenResponse = await _provider.AssociateSoftwareTokenAsync(associateSoftwareTokenRequest);

            return associateSoftwareTokenResponse.SecretCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding MFA: {ex.Message}");
            return "KO";
        }
    }

    public async Task<string> MFARegistrationConfirmation(string email, string password, string mfaCode)
    {
        try
        {
            var verifySoftwareTokenRequest = new VerifySoftwareTokenRequest
            {
                AccessToken = await GetAccessToken(email, password),
                UserCode = mfaCode,
                FriendlyDeviceName = "MFAEnabled-" + email
            };

            var verifySoftwareTokenResponse = await _provider.VerifySoftwareTokenAsync(verifySoftwareTokenRequest);
            if(verifySoftwareTokenResponse.HttpStatusCode.ToString() == "OK")
            {
                    var mfaOptions = new SetUserMFAPreferenceRequest()
                    {
                        AccessToken = await GetAccessToken(email, password),
                        SoftwareTokenMfaSettings = new SoftwareTokenMfaSettingsType
                        {
                            Enabled = true,
                            PreferredMfa = true
                        }
                    };

                    var enableSoftwareTokenResponse = await _provider.SetUserMFAPreferenceAsync(mfaOptions);
                    return "OK";
            }
            else
            {
                return "KO";
            }

        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error registering MFA Token: {ex.Message}");
            return "KO";
        }
    }

    public async Task<string> SignInAndWaitForMfa(string email, string password)
    {
        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _clientId,
            AuthParameters = new Dictionary<string, string>
                {
                    {"USERNAME", email},
                    {"PASSWORD", password}
                }
        };

        try
        {
            var authResponse = await _provider.InitiateAuthAsync(authRequest);
            return authResponse.Session.ToString();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error in logging in {ex.Message} ");
            return "";
        }
    }

    public async Task<string> ConfirmMFAToken(string session, string email, string token)
    {
        var challengeResponse = new RespondToAuthChallengeRequest
        {
            ChallengeName = ChallengeNameType.SOFTWARE_TOKEN_MFA,
            ClientId = _clientId,
            Session = session,
            ChallengeResponses = new Dictionary<string, string>
                {
                    {"USERNAME", email},
                    {"SOFTWARE_TOKEN_MFA_CODE", token}
                }
        };

        try
        {
            var response = await _provider.RespondToAuthChallengeAsync(challengeResponse);
            return response.AuthenticationResult.IdToken;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error in logging in {ex.Message} ");
            return "";
        }
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}
