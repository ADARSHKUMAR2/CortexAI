using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Google;

namespace CortexAI
{
    public class FirebaseAuthManager : MonoBehaviour
    {
        // PASTE YOUR WEB CLIENT ID HERE
        public string webClientId = "YOUR_WEB_CLIENT_ID_FROM_FIREBASE.apps.googleusercontent.com"; 

        public string firebaseToken;

        private FirebaseAuth auth;

        private void Start()
        {
            // 1. Initialize Firebase
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    Debug.Log("Firebase Initialized Successfully.");
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
            
            // 2. Configure Google Sign In
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = webClientId
            };
        }

        // Call this method from your UI Button
        public async Task<string> SignInWithGoogleAsync()
        {
            try
            {
                // 1. Prompt Google Login
                GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();
                
                // 2. Pass Google ID Token to Firebase
                Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
                AuthResult authResult = await auth.SignInAndRetrieveDataWithCredentialAsync(credential);
                
                FirebaseUser user = authResult.User;
                Debug.Log($"Firebase Auth Success! User: {user.DisplayName}");

                // 3. Get the Firebase ID Token to send to your Python Backend
                string firebaseIdToken = await user.TokenAsync(true);
                return firebaseIdToken;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Google Sign-In Failed: {ex.Message}");
                return null;
            }
        }

        public void SignOut()
        {
            if (auth != null) auth.SignOut();
            GoogleSignIn.DefaultInstance.SignOut();
        }
    }
}
