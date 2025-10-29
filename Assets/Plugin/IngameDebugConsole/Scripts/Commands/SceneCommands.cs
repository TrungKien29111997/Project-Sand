using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace IngameDebugConsole.Commands
{
	public class SceneCommands
	{
		[ConsoleMethod( "scene.load", "Loads a scene" ), UnityEngine.Scripting.Preserve]
		public static void LoadScene( string sceneName )
		{
			LoadSceneInternal( sceneName, false, LoadSceneMode.Single );
		}

		[ConsoleMethod( "scene.load", "Loads a scene" ), UnityEngine.Scripting.Preserve]
		public static void LoadScene( string sceneName, LoadSceneMode mode )
		{
			LoadSceneInternal( sceneName, false, mode );
		}

		[ConsoleMethod( "scene.loadasync", "Loads a scene asynchronously" ), UnityEngine.Scripting.Preserve]
		public static void LoadSceneAsync( string sceneName )
		{
			LoadSceneInternal( sceneName, true, LoadSceneMode.Single );
		}

		[ConsoleMethod( "scene.loadasync", "Loads a scene asynchronously" ), UnityEngine.Scripting.Preserve]
		public static void LoadSceneAsync( string sceneName, LoadSceneMode mode )
		{
			LoadSceneInternal( sceneName, true, mode );
		}

		private static void LoadSceneInternal( string sceneName, bool isAsync, LoadSceneMode mode )
		{
			if( SceneManager.GetSceneByName( sceneName ).IsValid() )
			{
				Debug.Log( "Scene " + sceneName + " is already loaded" );
				return;
			}

			if( isAsync )
				SceneManager.LoadSceneAsync( sceneName, mode );
			else
				SceneManager.LoadScene( sceneName, mode );
		}

		[ConsoleMethod( "scene.unload", "Unloads a scene" ), UnityEngine.Scripting.Preserve]
		public static void UnloadScene( string sceneName )
		{
			SceneManager.UnloadSceneAsync( sceneName );
		}

		[ConsoleMethod( "scene.restart", "Restarts the active scene" ), UnityEngine.Scripting.Preserve]
		public static void RestartScene()
		{
			SceneManager.LoadScene( SceneManager.GetActiveScene().name, LoadSceneMode.Single );
		}

        [ConsoleMethod("ui.hide", "Hide UI for recording video"), UnityEngine.Scripting.Preserve]
        public static void UIHide()
        {
            PlayerPrefs.SetInt("UIHide", PlayerPrefs.GetInt("UIHide", 0) == 0 ? 1 : 0);

			if (SceneManager.GetActiveScene().name != "Init") RestartScene();

			DebugLogManager.Instance.RefreshHide();
        }

        [ConsoleMethod("audio.bypass", "Bypass all audio mixer effects"), UnityEngine.Scripting.Preserve]
        public static void AudioBypass()
        {
            PlayerPrefs.SetInt("AudioBypass", PlayerPrefs.GetInt("AudioBypass", 0) == 0 ? 1 : 0);

            DebugLogManager.Instance.onCmdAudioBypass?.Invoke();
        }
    }
}