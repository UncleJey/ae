using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Объект содержит функционал вызова методов с задержками в статичном контексте, а так же их асинхронных методов
/// </summary>
public class Delay : MonoBehaviour
{
    /// <summary>
    /// Ретранстляция перехода в паууз для использования статичными объектами
    /// </summary>
    public static Action<bool> OnPaused;

    public enum Priority
    {
        Minimum = -3,
        VeryLow = -2,
        Low = -1,
        Normal = 0,
        High = 1,
        VeryHigh = 2,
        Maximum = 3
    }

    #region Base

    public static Delay Queue { get; private set; }

    void Awake()
    {
        Queue = this;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        OnPaused?.Invoke(hasFocus);
    }

    /// <summary>
    /// Нужно всё уничтожить.
    /// Немного раньше, чем событие дестрой пойдёт
    /// </summary>
    void OnDisable()
    {
        gotCalls = false;
        StopAllCoroutines();

        foreach (var usage in conditionUsages)
        {
            usage.Key.Unsubscribe();
        }

        conditionUsages.Clear();

        while (queue.Count > 0)
            RemoveFromQueue(0);

        calls.Clear();
        Queue = null;
    }

    void Update()
    {
        if (!gotCalls)
            return;

        List<Action> callsTmp;
        lock (callsLock)
        {
            callsTmp = new List<Action>(calls);
            calls.Clear();
        }

        gotCalls = false;

        for (int i = callsTmp.Count - 1; i >= 0; i--)
        {
            string error = callsTmp[i].TryExecute();
            callsTmp.RemoveAt(i);
            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);
        }
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Запускает корутину
    /// </summary>
    public static Coroutine Coroutine(IEnumerator _routine)
    {
        return Queue.StartCoroutine(_routine);
    }

    private static IEnumerator Caller(Action _event, float _seconds)
    {
        if (_seconds > 0f)
            yield return new WaitForSeconds(_seconds);
        else
            yield return null;
        Call(() => _event?.Invoke());
    }

    /// <summary>
    /// Останавливает корутину
    /// </summary>
    public static void StopCoroutineNow(ref Coroutine _routine)
    {
        if (_routine != null && Queue != null)
        {
            Queue.StopCoroutine(_routine);
            _routine = null;
        }
    }

    /// <summary>
    /// Выполняет событие через указанное число секунд. Запускается через корутину, так что оно больше подходит для маленьких таймингов вроде пары секунд. Thread-safe.
    /// </summary>
    public static void CallIn(Action _event, float _seconds)
    {
        if (Queue != null)
            Queue.StartCoroutine(Caller(_event, _seconds));
    }

    #endregion

    #region Call

    private static List<Action> calls = new List<Action>(10);
    private static bool gotCalls;
    private static object callsLock = new object();

    /// <summary>
    /// Вызовет указанный метод на следующий вызов Update. Thread-safe.
    /// </summary>
    public static void Call(Action _event)
    {
        lock (callsLock)
        {
            calls.Add(_event);
            gotCalls = true;
        }
    }

    #endregion

    #region Queue

    //Данные методы не статичны т.к. их список расширяется через Extensions. И потому удобнее, если все они будут в одной куче

    #region Classes

    /// <summary>
    /// Условие задержки вызова
    /// </summary>
    public abstract class Condition
    {
        /// <summary>
        /// Возвращает истинку когда условие разрешает вызов метода
        /// </summary>
        public abstract bool IsValid { get; }

        /// <summary>
        /// Подписаться на события, необходимые для работы условия
        /// </summary>
        public abstract void Subscribe();

        /// <summary>
        /// Отписаться от событий, необходимый для работы условия
        /// </summary>
        public abstract void Unsubscribe();

        /// <summary>
        /// Следует вызвать данный метод чтобы сообщить очереди, что произошли изменения в условиях
        /// </summary>
        protected void CallConditionModified()
        {
            Queue.CheckQueue();
        }

        /// <summary>
        /// Список условий по-умолчанию
        /// </summary>
        public static List<Condition> Defaults = new List<Condition>();
    }

    /// <summary>
    /// Элемент очереди вызовов
    /// </summary>
    private class QueueElem
    {
        /// <summary>
        /// Владелец данного вызова. Может быть null
        /// </summary>
        public object Owner;

        /// <summary>
        /// Список условий выполнения данного вызова
        /// </summary>
        public IList<Condition> Conditions;

        /// <summary>
        /// Выполняемый метод
        /// </summary>
        public Action Act;

        /// <summary>
        /// Приоритет вызова. Больше значение - раньше в очереди
        /// </summary>
        public Priority Priority = Priority.Normal;

        public QueueElem(object _owner, Action _act, IList<Condition> _conditions,
            Priority _priority = Priority.Normal)
        {
            Owner = _owner;
            Act = _act;
            Conditions = _conditions;
            Priority = _priority;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Вызвать метод
    /// </summary>
    /// <param name="pAction">Метод</param>
    /// <param name="pOwner">Экземпляр владельца</param>
    /// <param name="pDeltaSec">Через сколько секунд</param>
    /// <param name="pConditions">Дополнительные условия для вызова</param>
    /// <param name="pPriority">Приоритет</param>
    public static void CallInQueueLate(Action pAction, object pOwner = null, int pDeltaSec = 0,
        IList<Condition> pConditions = null,
        Priority pPriority = Priority.Normal)
    {
        CallInQueueLate(pAction, pOwner, (long) (pDeltaSec > 0 ? Clocks.time + pDeltaSec : 0),
            pConditions,
            pPriority);
    }

    /// <summary>
    /// Вызвать метод
    /// </summary>
    /// <param name="pAction">Метод</param>
    /// <param name="pOwner">Экземпляр владельца</param>
    /// <param name="pTime">Во сколько запустить</param>
    /// <param name="pConditions">Дополнительные условия для вызова</param>
    /// <param name="pPriority">Приоритет</param>
    public static void CallInQueueLate(Action pAction, object pOwner = null, long pTime = 0,
        IList<Condition> pConditions = null,
        Priority pPriority = Priority.Normal)
    {
        if (Queue == null)
        {
            Debug.LogError("Delay not loaded Yet");
            return;
        }

        if (pTime > 0)
        {
            if (pConditions == null)
            {
                pConditions = new List<Condition>();
            }

            pConditions.Add(new DelayCondition() {StartTime = pTime});
        }

        Queue.CallInQueue(pOwner, pAction, pConditions, pPriority);
    }


    /// <summary>
    /// Вызывает указанное событие при выполнении списка условий
    /// </summary>
    /// <param name="_action">Событие для вызова</param>
    /// <param name="_conditions">Набор условий</param>
    /// <param name="_priority">Приоритет вызова. Больше значение - раньше вызов</param>
    public void CallInQueue(Action _action, IList<Condition> _conditions, Priority _priority = Priority.Normal)
    {
        CallInQueue(null, _action, _conditions, _priority);
    }

    /// <summary>
    /// Вызывает указанное событие при выполнении общего списка условий
    /// </summary>
    /// <param name="_action">Событие для вызова</param>
    /// <param name="_priority">Приоритет вызова. Больше значение - раньше вызов</param>
    public void CallInQueue(Action _action, Priority _priority = Priority.Normal)
    {
        CallInQueue(null, _action, _priority);
    }

    /// <summary>
    /// Вызывает указанное событие при выполнении общего списка условий
    /// Переписывает текущее событие если событие от указанного владельца уже имеется
    /// </summary>
    /// <param name="_owner">Владелец события</param>
    /// <param name="_action">Событие для вызова</param>
    /// <param name="_priority">Приоритет вызова. Больше значение - раньше вызов</param>
    public void CallInQueue(object _owner, Action _action, Priority _priority = Priority.Normal)
    {
        CallInQueue(_owner, _action, null, _priority);
    }

    /// <summary>
    /// Вызывает указанное событие при выполнении списка условий
    /// Переписывает текущее событие если событие от указанного владельца уже имеется
    /// </summary>
    /// <param name="_owner">Владелец события</param>
    /// <param name="_action">Событие для вызова</param>
    /// <param name="_conditions">Набор условий</param>
    /// <param name="_priority">Приоритет вызова. Больше значение - раньше вызов</param>
    public void CallInQueue(object _owner, Action _action, IList<Condition> _conditions,
        Priority _priority = Priority.Normal)
    {
        if (_owner != null)
        {
            int existingInex = queue.IndexOf(t => t.Owner == _owner);
            if (existingInex >= 0)
                RemoveFromQueue(existingInex);
        }

        if (_conditions == null)
            _conditions = Condition.Defaults;

        var newElement = new QueueElem(_owner, _action.Execute, _conditions, _priority);

        bool added = false;
        for (int i = 0; i < queue.Count; i++)
        {
            if (_priority > queue[i].Priority)
            {
                queue.Insert(i, newElement);
                added = true;
                break;
            }
        }

        if (!added)
            queue.Add(newElement);

        for (int i = _conditions.Count - 1; i >= 0; i--)
            ConditonAdd(_conditions[i]);

        CheckQueue();
    }

    /// <summary>
    /// Возвращает истинку когда условия любого из элементов очереди уже выполнены и событие будет выполнено в ближайшем цикле
    /// </summary>
    public bool IsWaitingForCall()
    {
        if (queue.Count > 0)
        {
            for (int i = queue.Count-1; i>=0; i--)
            {
                QueueElem elem = queue[i];
                if (!CheckQueueValid(elem.Conditions))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Возвращает истину если очередь вызовов пуста
    /// </summary>
    public bool IsEmpty()
    {
        return queue.Count == 0;
    }

    /// <summary>
    /// Проверяет находится ли данный объект в очереди
    /// </summary>
    public bool IsQueued(object _owner)
    {
        return queue.IndexOf(t => t.Owner == _owner) >= 0;
    }

    /// <summary>
    /// Удаляет событие для указанного объекта из очереди
    /// Возвращает истину если событие было удалено
    /// </summary>
    public static bool RemoveQueue(object _owner)
    {
        return Queue != null && Queue.RemoveFromQueue(_owner);
    }

    /// <summary>
    /// Удаляет событие для указанного объекта из очереди
    /// Возвращает истину если событие было удалено
    /// </summary>
    public bool RemoveFromQueue(object _owner)
    {
        int index = queue.IndexOf(t => t.Owner == _owner);
        if (index < 0)
            return false;

        return RemoveFromQueue(index);
    }

    /// <summary>
    /// Удаляет событие на указанном индексе из очереди
    /// Возвращает истину если событие было удалено
    /// </summary>
    private bool RemoveFromQueue(int _index)
    {
        if (!queue.IsInRange(_index))
            return false;

        var elem = queue[_index];
        for (int j = elem.Conditions.Count - 1; j >= 0; j--)
            ConditionRemove(elem.Conditions[j]);

        queue.RemoveAt(_index);
        return true;
    }

    /// <summary>
    /// Проверяет список условий
    /// </summary>
    public bool CheckQueueValid(IList<Condition> _conditions)
    {
        for (int i = _conditions.Count - 1; i >= 0; i--)
        {
            if (!_conditions[i].IsValid)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Проверяет список условий
    /// </summary>
    public bool CheckQueueValid(params Condition[] _conditions)
    {
        if (_conditions == null)
            return CheckQueueValid();

        return CheckQueueValid((IList<Condition>) _conditions);
    }

    /// <summary>
    /// Проверяет все общие условия (QueueCondition.Defaults)
    /// </summary>
    public bool CheckQueueValid()
    {
        return CheckQueueValid(Condition.Defaults);
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Текущая очередь вызовов
    /// </summary>
    private List<QueueElem> queue = new List<QueueElem>();

    /// <summary>
    /// Счетчик использований условий. Применяется для вызова событий Subscribe/Unsubscribe
    /// </summary>
    private Dictionary<Condition, int> conditionUsages = new Dictionary<Condition, int>();

    /// <summary>
    /// Атикнвая корутина проверки очереди
    /// </summary>
    private Coroutine checkerRoutine;

    /// <summary>
    /// Подписывается на условие
    /// </summary>
    private void ConditonAdd(Condition _condition)
    {
        if (conditionUsages.TryGetValue(_condition, out int count))
            conditionUsages[_condition] = count + 1;
        else
        {
            conditionUsages.Add(_condition, 1);
            _condition.Subscribe();
        }
    }

    /// <summary>
    /// Отписывается от условия
    /// </summary>
    private void ConditionRemove(Condition _condition)
    {
        if (conditionUsages.TryGetValue(_condition, out int count))
        {
            if (--count > 0)
            {
                conditionUsages[_condition] = count;
                return;
            }

            conditionUsages.Remove(_condition);
            _condition.Unsubscribe();
        }
    }

    /// <summary>
    /// Проверяет очередь и выполняет события, которые удовлетворяют условиям
    /// </summary>
    private void CheckQueue()
    {
        IEnumerator CheckerRoutine()
        {
            //Небольшая задержка, примерно в 1 кадр
            yield return new WaitForSeconds(0.05f);

            if (queue.Count > 0)
            {
                for (int i = 0; i < queue.Count; i++)
                {
                    QueueElem elem = queue[i];

                    if (!CheckQueueValid(elem.Conditions))
                        continue;

                    RemoveFromQueue(i--);
                    // Чтобы выполнилось в основном потоке
#if UNITY_EDITOR
                    if (elem.Act == null)
                    {
                        Debug.Log("Empty event in " + (elem.Owner?.ToString()));
                    }
#endif
                    Call(() => elem.Act?.Invoke());
                }
            }


            if (queue.Count == 0)
            {
                //Проверка не сломалось ли чего в коде
                foreach (var usage in conditionUsages)
                {
                    usage.Key.Unsubscribe();
#if UNITY_EDITOR
                    Debug.LogError($"There is still a usage of {usage.Key}");
#endif
                }

                conditionUsages.Clear();
            }

            checkerRoutine = null;
        }

        if (checkerRoutine == null)
            checkerRoutine = StartCoroutine(CheckerRoutine());
    }

    #endregion

    #endregion

    #region Editor

#if UNITY_EDITOR

    /// <summary>
    /// Функционал вызова методов в очереди для редактора
    /// Аналогичен EditorApplication.delay, но с приоритетами
    /// </summary>
    public static class Editor
    {
        private class LeveledMethod
        {
            public Action Method;
            public Priority Priority;
        }

        private static List<LeveledMethod> queue;

        private static object callLock = new object();

        /// <summary>
        /// Вызывать метод с задержкой внутри редактора
        /// </summary>
        /// <param name="_event">Метод для вызова</param>
        /// <param name="_priority">Приоритет. Выше значение - раньше вызов</param>
        public static void Call(Action _event, Priority _priority = Priority.Normal)
        {
            LeveledMethod newElement = new LeveledMethod {Method = _event, Priority = _priority};

            if (queue == null)
            {
                queue = new List<LeveledMethod>();
                foreach (var call in queue)
                {
                    try
                    {
                        call.Method();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                queue = null;
            }

            bool added = false;
            for (int i = 0; i < queue.Count; i++)
            {
                if (_priority > queue[i].Priority)
                {
                    queue.Insert(i, newElement);
                    added = true;
                    break;
                }
            }

            if (!added)
                queue.Add(newElement);
        }

        /// <summary>
        /// Вызывает метод с задержкой внутри редактора
        /// Безопасно вызывать из нескольких параллельных потоков
        /// </summary>
        public static void CallTheadSafe(Action _event)
        {
//                Sirenix.OdinInspector.Editor.UnityEditorEventUtility.DelayActionThreadSafe(_event);
        }
    }

#endif

    #endregion
}