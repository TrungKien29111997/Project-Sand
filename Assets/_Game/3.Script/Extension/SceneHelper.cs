using System.Collections;
using System.Collections.Generic;
using TrungKien.Core.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrungKien.Core
{
    public class SceneHelper : Singleton<SceneHelper>
    {
        public IEnumerator IEChangeSceneLoading()
        {
            DebugCustom.Log("ChangeScene Loading");
            SceneManager.LoadScene(Constant.SCENE_LOADING);
            yield return null;
            DebugCustom.Log("ChangeScene Loading Done");
        }
        IEnumerator IEChangeSceneHome()
        {
            DebugCustom.Log("ChangeScene Home");
            SceneManager.LoadScene(Constant.SCENE_MAIN_UI);
            yield return null;
            DebugCustom.Log("ChangeScene Home Done");
            UIManager.Instance.OpenUI<CanvasHome>();
        }

        public IEnumerator IEGoGameplay(Transform focusTransform = null)
        {
            yield return StartCoroutine(IEChangeSceneLoading());
            yield return StartCoroutine(IEChangeSceneGameplay());
            yield return StartCoroutine(IELoadGamePlay());
            GameManager.Instance.SetGameState(EGameState.Gameplay);
            //yield return StartCoroutine(UIManager.Instance.IEGameInit());
            //yield return StartCoroutine(LoadingPanel.Instance.IEEndTransition());
            //IAchievementController.Instance.UpdateAchievement(EAchievement.PlayGame, 1);
        }
        IEnumerator IEChangeSceneGameplay()
        {
            DebugCustom.Log("ChangeScene Gameplay");
            SceneManager.LoadScene(Constant.SCENE_GAME_PLAY);
            yield return null;
            DebugCustom.Log("ChangeScene Gameplay Done");
        }
        public IEnumerator IELoadGamePlay()
        {
            yield return new WaitUntil(() => LevelControl.Instance);
            // yield return new WaitUntil(() => WorkshopIngame.Instance);
            // yield return new WaitUntil(() => EnemyManager.Instance);
            // UIManager.Instance.OnInit();
            // WorkshopIngame.Instance.InitLevel();
            // EnemyManager.Instance.OnInit();
            // GamePlayManager.Instance.SetUp(IChapterController.Instance.GetCurrentMode(), IChapterController.Instance.GetCurrentChapter());
            // UIManager.Instance.OpenUI<CanvasGamePlay>().SetCanvas(IChapterController.Instance.GetCurrentChapter());
        }
        public IEnumerator IEReturnHome(Transform focusTransform = null)
        {
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName(Constant.SCENE_LOADING))
            {
                yield return StartCoroutine(IEChangeSceneLoading());
            }
            yield return StartCoroutine(IEChangeSceneHome());
            //yield return StartCoroutine(LoadingPanel.Instance.IEEndTransition());
            GameManager.Instance.SetGameState(EGameState.Home);
        }

    }
}