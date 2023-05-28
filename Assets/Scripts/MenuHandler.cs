using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class MenuHandler : MonoBehaviour
    {
        public enum MenuState
        {
            Main,
            Create,
            Join,
            Options,
            HowTo,
            CardSets,
            GameCards,
            GamePlayers
        }
        public enum MenuCategory
        {
            Start,
            Game,
            All
        }

        public static MenuHandler Instance;
        public static Camera GameCamera;
        public static Canvas CanvasBack;
        public static Canvas CanvasFront;
        public static Canvas VectorCanvas;
        public static MenuState PreviousMenu;
        public static MenuState CurrentMenu;

        public UI_ChoiceSelectionDialog UI_ChoiceSelectionDialog;
        public UI_InputDialog UI_InputDialog;
        public UI_GameEndDialog UI_GameEndDialog;
        public UI_WarningDialog UI_WarningDialog;
        public UI_CardsDialog UI_CardsDialog;
        public UI_ResultDialog UI_ResultDialog;
        public UI_PointAwardedDialog UI_PointAwardedDialog;

        public static GameObject StartMenuCategory;
        public static GameObject GameMenuCategory;
        public static AllMenuContainer AllMenuCategory;

        public static MainMenu MainMenuObj;
        public static CreateMenu CreateMenuObj;
        public static JoinMenu JoinMenuObj;
        public static GamePlayersMenu GamePlayersMenuObj;
        public static GameCardsMenu GameCardsMenuObj;
        public static OptionsMenu OptionsMenuObj;
        public static CardSetsMenu CardSetsMenuObj;

        public Image DialogsBackgroundImg;
        public static DateTime StartTime = DateTime.Now;

        public static void SetMenuState(MenuState menu)
        {
            if (menu != PreviousMenu) { PreviousMenu = CurrentMenu; }
            CurrentMenu = menu;
        }

        private void Awake()
        {
            StartTime = DateTime.Now;
            Instance = this;
            GameCamera = Helpers.Find<Camera>(nameof(GameCamera));
            CanvasBack = Helpers.Find<Canvas>(nameof(CanvasBack));
            CanvasFront = Helpers.Find<Canvas>(nameof(CanvasFront));
            VectorCanvas = Helpers.Find<Canvas>(nameof(VectorCanvas));
            StartMenuCategory = Helpers.Find<GameObject>(nameof(StartMenuCategory));
            GameMenuCategory = Helpers.Find<GameObject>(nameof(GameMenuCategory));
            AllMenuCategory = Helpers.Find<AllMenuContainer>(nameof(AllMenuCategory));
            MainMenuObj = Helpers.Find<MainMenu>(nameof(MainMenu));
            CreateMenuObj = Helpers.Find<CreateMenu>(nameof(CreateMenu));
            JoinMenuObj = Helpers.Find<JoinMenu>(nameof(JoinMenu));
            OptionsMenuObj = Helpers.Find<OptionsMenu>(nameof(OptionsMenu));
            GamePlayersMenuObj = Helpers.Find<GamePlayersMenu>(nameof(GamePlayersMenu));
            GameCardsMenuObj = Helpers.Find<GameCardsMenu>(nameof(GameCardsMenu));
            CardSetsMenuObj = Helpers.Find<CardSetsMenu>(nameof(CardSetsMenu));
            DialogsBackgroundImg = Helpers.Find<Image>(nameof(DialogsBackgroundImg));
            UI_ChoiceSelectionDialog = Helpers.Find<UI_ChoiceSelectionDialog>("ChoiceSelectionDialog");
            UI_InputDialog = Helpers.Find<UI_InputDialog>("InputDialog");
            UI_GameEndDialog = Helpers.Find<UI_GameEndDialog>("GameEndDialog");
            UI_WarningDialog = Helpers.Find<UI_WarningDialog>("WarningDialog");
            UI_CardsDialog = Helpers.Find<UI_CardsDialog>("CardsDialog");
            UI_ResultDialog = Helpers.Find<UI_ResultDialog>("ResultDialog");
            UI_PointAwardedDialog = Helpers.Find<UI_PointAwardedDialog>("PointAwardedDialog");
        }
        private void Start()
        {
            if (Application.isMobilePlatform)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
            QualitySettings.vSyncCount = 2;

#if UNITY_IOS || UNITY_ANDROID
            Newtonsoft.Json.Utilities.AotHelper.EnsureDictionary<int, CardData>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<int>();
#endif

            DataEmail.LoadJson();
            DataCardSet.SetOnLoadCallback(() => MainMenu.Instance?.UpdateUI());
            TaskController.Create("DataCardSet.LoadJson", DataCardSet.LoadJson());
            DataLanguages.LoadJson();
            SelectMenu(MenuState.Main);
        }
        public bool IsAnyDialogActive()
        {
            return UI_ChoiceSelectionDialog.gameObject.activeSelf || UI_InputDialog.gameObject.activeSelf || UI_GameEndDialog.gameObject.activeSelf || UI_WarningDialog.gameObject.activeSelf || UI_CardsDialog.gameObject.activeSelf || UI_ResultDialog.gameObject.activeSelf;
        }

        public void SelectMenu(MenuState menu)
        {
            SetMenuState(menu);
            var menuCategory = GetMenuCategory(menu);
            if (menuCategory != MenuCategory.All)
            {
                StartMenuCategory.SetActive(menuCategory == MenuCategory.Start);
                GameMenuCategory.SetActive(menuCategory == MenuCategory.Game);
            }
            switch (menu) // needed to call respective menu's Awake()
            {
                case MenuState.Main:
                    MainMenuObj.gameObject.SetActive(true);
                    break;
                case MenuState.Create:
                    CreateMenuObj.gameObject.SetActive(true);
                    break;
                case MenuState.Join:
                    JoinMenuObj.gameObject.SetActive(true);
                    break;
                case MenuState.GamePlayers:
                    GamePlayersMenuObj.gameObject.SetActive(true);
                    break;
                case MenuState.GameCards:
                    GameCardsMenuObj.gameObject.SetActive(true);
                    break;
                case MenuState.Options:
                    OptionsMenuObj.gameObject.SetActive(true);
                    break;
                case MenuState.CardSets:
                    CardSetsMenuObj.gameObject.SetActive(true);
                    break;
            }
            MainMenu.Instance?.Activate(menu == MenuState.Main);
            CreateMenu.Instance?.Activate(menu == MenuState.Create);
            JoinMenu.Instance?.Activate(menu == MenuState.Join);
            GamePlayersMenu.Instance?.Activate(menu == MenuState.GamePlayers);
            GameCardsMenu.Instance?.Activate(menu == MenuState.GameCards);
            OptionsMenu.Instance?.Activate(menu == MenuState.Options);
            CardSetsMenu.Instance?.Activate(menu == MenuState.CardSets);

            GameMenuSharedContainer.Instance?.Color();
            AllMenuContainer.Instance?.Color();
        }

        public MenuCategory GetMenuCategory(MenuState menu)
        {
            switch (menu)
            {
                case MenuState.Main:
                case MenuState.Create:
                case MenuState.Join:
                case MenuState.HowTo:
                case MenuState.CardSets:
                    return MenuCategory.Start;
                case MenuState.GameCards:
                case MenuState.GamePlayers:
                    return MenuCategory.Game;
                case MenuState.Options:
                    return MenuCategory.All;
                default:
                    return MenuCategory.Start;
            }
        }

        public void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }
}
