using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Foranj.SDK.GUI
{
    public abstract class WindowBase : GUIElement<WindowBase>
    {
        private Animator aAnimator;
        private Animation aAnimation;

        public const string HIDE_TRIGGER = "Hide";
        public const string SHOW_TRIGGER = "Show";

        protected const float AnimationTime = 0.01f;
        protected const float OffScreenTime = 0.5f;

        public static event Func<WindowBase, bool> CanOpenGlobal;
        public event Func<bool> CanOpen;

        [NonSerialized, HideInInspector] public bool isClosing;
        [NonSerialized, HideInInspector] public bool isLoading; //false - когда все загружено
        public bool IsOpened => Opened.Contains(this);

        private static Transform topTransform;

        /// <summary>
        /// Самый верхний элемент иерархии окон
        /// </summary>
        private static Transform TopTransform
        {
            get
            {
                if (topTransform == null)
                {
                    if (GUIManager.RectTransform && GUIManager.RectTransform.childCount > 0)
                    {
                        topTransform = GUIManager.RectTransform.GetChild(0);
                        return topTransform;
                    }

                    foreach (var pair in Existing)
                    {
                        if (pair.Value.WindowIndex == 0)
                        {
                            topTransform = pair.Value.transform;
                            return topTransform;
                        }
                    }
                }

                return topTransform;
            }
        }

        /// <summary>
        /// Порядковый номер окна. Соответствует глобальному порядку в иерархии
        /// </summary>
        public int WindowIndex { get; internal set; } = -1;

        /// <summary>
        /// Глобальный счетчик для назначения индексов окнам в порядке их инициализации
        /// </summary>
        private static int windowIndexCounter = -1;

        protected sealed override void Initialize()
        {
            WindowIndex = ++windowIndexCounter;
            aAnimator = GetComponent<Animator>();
            aAnimation = GetComponent<Animation>();
        }

        private bool AnimPlay(string _name)
        {
            if (aAnimation != null)
            {
                aAnimation.Play(_name);
                return true;
            }

            if (aAnimator != null)
            {
                AnimatorStateInfo state = aAnimator.GetCurrentAnimatorStateInfo(0);

                if (state.length <= 0f || state.normalizedTime >= 1f)
                    aAnimator.Play(_name);
                else
                {
                    if (state.IsName(_name))
                        return true;
                    aAnimator.Play(_name, 0, 1f - state.normalizedTime);
                }

                aAnimator.Update(0f);
                return true;
            }

            return false;
        }

        private bool CheckForOtherOpenWindow()
        {
            WindowBase[] windows = GetComponents<WindowBase>().ToArray();
            if (windows != null)
                return windows.FirstOrDefault(window => window.IsOpened) != null;
            return false;
        }

        /// <summary>
        /// Вызывается один раз при первом запуске игры. Пустая, так что base можно не вызывать
        /// </summary>
        protected virtual void Awake()
        {
        }

        #region Close

        [SerializeField] protected string CloseSound;

        /// <summary>
        /// Скрыть подложку и окно с анимацией 
        /// </summary>
        public void Close(bool _forced)
        {
            if (!IsOpened)
            {
                if (gameObject.activeSelf && !CheckForOtherOpenWindow())
                    gameObject.SetActive(false);
                return;
            }

            if (!_forced && !TryClose())
                return;

            isClosing = true;
            if (!AnimPlay(HIDE_TRIGGER))
                AnimatorFinish();
            AddClosed();

            RefreshWindowBackground();

            if (!string.IsNullOrEmpty(CloseSound))
                AudioController.Play(CloseSound);

            OnClose();
            CallClosed();
            OnAfterClose();
        }

        /// <summary>
        /// Скрыть подложку и окно с анимацией 
        /// </summary>
        public void Close()
        {
            Close(false);
        }

        /// <summary>
        /// Форсированное закрытие окна без анимаций и прочего
        /// </summary>
        public void CloseNow()
        {
            Close();
            AnimatorFinish();
        }

        /// <summary>
        /// Скрыть только окно
        /// </summary>
        public void CloseAll()
        {
            Close();
        }

        /// <summary>
        /// Вызывается в самом начале функции Close. Если вернуть false, то окно не закроется
        /// </summary>
        /// <returns></returns>
        protected virtual bool TryClose()
        {
            return true;
        }

        /// <summary>
        /// Вызывается в конце функции Close, когда анимация закрытия уже запущена
        /// </summary>
        public virtual void OnClose()
        {
        }

        /// <summary>
        /// Вызывается после вызова всех событий закрытия окна
        /// </summary>
        public virtual void OnAfterClose()
        {
        }

        /// <summary>
        /// Скрывает окно указанного типа
        /// </summary>
        /// <typeparam name="T">Тип окна</typeparam>
        public static void Close<T>()
            where T : WindowBase
        {
            GUIManager.GetWindow<T>().Close();
        }

        /// <summary>
        /// Вызывается аниматором по окончании анимации закрытия
        /// </summary>
        public void AnimatorFinish()
        {
            if (!isClosing)
                return;
            isClosing = false;
            OnAnimatorFinish();
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        protected virtual void OnAnimatorFinish()
        {
        }

        /// <summary>
        /// Вызывается когда окно полностью скрывается (обычно после окончания анимации)
        /// </summary>
        protected virtual void OnDisable()
        {
        }

        #endregion Close

        #region Open

        [SerializeField] protected string OpenSound;

        /// <summary>
        /// Функция с результатом void для вызова через Unity GUI
        /// </summary>
        public virtual void _OpenUGUI()
        {
            Open();
        }

        /// <summary>
        /// Открывает окно
        /// </summary>
        /// <returns>Истина если окно было открыто</returns>
        public bool Open()
        {
            return Open(false);
        }

        /// <summary>
        /// Принудительно открывает окно, игнорируя все условия
        /// </summary>
        /// <returns>Истина если окно было открыто</returns>
        public bool OpenForced()
        {
            return Open(true);
        }

        /// <summary>
        /// Открывает окно
        /// </summary>
        /// <returns>Истина если окно было открыто</returns>
        protected bool Open(bool _forced)
        {
            if (IsOpened)
                return false;

            //GUIManager.CloseAllWindows();

            isClosing = false;
            gameObject.SetActive(true);

            AnimPlay(SHOW_TRIGGER);

            AddOpened();

            RefreshWindowBackground();

//                GUIManager.UIHideAll(this, remainingUIParts);

            if (!string.IsNullOrEmpty(OpenSound))
                AudioController.Play(OpenSound);

            OnOpen();
            CallOpened();
            return true;
        }

        protected void CallShowAnimation()
        {
            AnimPlay(SHOW_TRIGGER);
        }

        /// <summary>
        /// Вызывается в самом начале функции Open. Если вернуть false, то окно не отроектся
        /// </summary>
        /// <returns></returns>
        protected virtual bool TryOpen()
        {
            return true;
        }

        /// <summary>
        /// Вызывается после успешного открытия окна, прямо перед событиями открытия окна
        /// </summary>
        public virtual void OnOpen()
        {
        }

        /// <summary>
        /// Открывает окно указанного типа
        /// </summary>
        /// <typeparam name="T">Тип окна</typeparam>
        public static T Open<T>()
            where T : WindowBase
        {
            T wnd = GUIManager.GetWindow<T>();
            wnd.Open();
            return wnd;
        }

        #endregion Open

        private void RefreshWindowBackground()
        {
            //WindowBackground.Show( Opened );
        }
    }
}